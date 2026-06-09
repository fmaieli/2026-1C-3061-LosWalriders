using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TGC.MonoGame.TP.SourceCode.Components;

namespace TGC.MonoGame.TP.SourceCode.Entities.Character
{
    internal class Player
    {
        public Vector3 Position { get; private set; } = new Vector3(0, 50, 150);
        public float Rotation { get; private set; } = 0f;
        public bool IsHidden { get; set; } = false; // Estado que valida si se encuentra escondido o no
        public int? InteractableModelIndex { get; private set; } = null; // Indice para saber cual es el modelo con el cual el jugadr interactua
        public int CollectedKeys { get; private set; } = 0;
        public bool HasWon { get; private set; } = false;
        public bool IsLightActive => (nokiaLight != null && nokiaLight.IsActive) || (matchLight != null && matchLight.IsActive);
        public float CurrentLightDurabilityPercentage // Porcentaje de durabilidad
        {
            get
            {
                if (nokiaLight != null && nokiaLight.IsActive)
                    return nokiaLight.Durability / nokiaLight.MaxDurability;

                if (matchLight != null && matchLight.IsActive)
                    return matchLight.Durability / matchLight.MaxDurability;

                return 0f;
            }
        }

        // Variables de camara Free y No Clip (para debuguear)
        private float _cameraPitch = 0f;
        private bool _freeCameraMode = false;
        private bool _noClipMode = false;

        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

        public Matrix View { get; private set; }

        private Model _armsModel;
        private Effect _armsEffect;
        private Model _lockOpenModel;

        private LightSource nokiaLight;
        private LightSource matchLight;

        public void LoadContent(ContentManager content, Effect effect)
        {
            _armsEffect = content.Load<Effect>("Effects/ArmsShader");
            _armsModel = content.Load<Model>("Models/Player/PSX_Player_Arms");

            foreach (var mesh in _armsModel.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                    part.Effect = _armsEffect.Clone();
            }

            nokiaLight = new NokiaLight();
            matchLight = new MatchLight();

            nokiaLight.LoadContent(content, effect);
            matchLight.LoadContent(content, effect);

            // Cargo el modelo del candado abierto
            _lockOpenModel = content.Load<Model>("Models/Items/PSX_Item_Lock_Open");
            foreach (var mesh in _lockOpenModel.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = effect.Clone();
                }
            }
        }

        public void DrawArms(Matrix view, Matrix projection, GraphicsDevice graphicsDevice)
        {
            if (_armsModel == null || IsHidden) return;

            var bones = new Matrix[_armsModel.Bones.Count];
            _armsModel.CopyAbsoluteBoneTransformsTo(bones);

            // Hacia donde esta mirando el jugador y donde esta parado
            Matrix cameraWorld = Matrix.Invert(view);

            // Busco el lugar donde quede correctamente el modelo
            Vector3 armsOffset = new Vector3(-30f, -5f, -32f);

            foreach (var mesh in _armsModel.Meshes)
            {
                Matrix centerOffset = Matrix.CreateTranslation(-mesh.BoundingSphere.Center);
                float rotY = MathHelper.Pi; // Roto el modelo de los brazos 180°
                Matrix rotation = Matrix.CreateRotationY(rotY);

                Matrix world =
                    Matrix.CreateScale(0.9f) *
                    centerOffset *
                    rotation *
                    Matrix.CreateTranslation(armsOffset) *
                    cameraWorld;

                foreach (var part in mesh.MeshParts)
                {
                    var fx = (Effect)part.Effect;
                    fx.CurrentTechnique = fx.Techniques["BasicColorDrawing"];
                    fx.Parameters["World"]?.SetValue(bones[mesh.ParentBone.Index] * world);
                    fx.Parameters["View"]?.SetValue(view);
                    fx.Parameters["Projection"]?.SetValue(projection);
                    fx.Parameters["DiffuseColor"]?.SetValue(Color.Magenta.ToVector3());
                }

                mesh.Draw();
            }

            // Dibujo los objetos de luz
            nokiaLight?.Draw(view, projection, cameraWorld);
            matchLight?.Draw(view, projection, cameraWorld);
        }

        public void Update(GameTime gameTime, List<(Model Model, Matrix World, string Name)> models)
        {
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Manejo de Toggles
            HandleToggles(keyboardState);

            // Modelos con los que interactuar cercanos
            // Reviso antes de interactuar si el modelo es interactuable
            float closestDistance = 120f;

            if (!IsHidden) // Solo se puede interactuar con los objetos si no estamos escondidos
            {
                for (int i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    if (model.Name.Contains("PSX_Wooden_Closet") ||
                        model.Name.Contains("PSX_Item_Match_Box") ||
                        model.Name.Contains("PSX_Item_Lock_Locked") ||
                        model.Name.Contains("PSX_Item_Door"))
                    {
                        float distanceToModel = Vector3.Distance(Position, model.World.Translation);
                        if (distanceToModel < closestDistance)
                        {
                            closestDistance = distanceToModel;
                            InteractableModelIndex = i; // Guardo el indice del modelo mas cercano
                        }
                    }
                }
            }

            // Manejo de interaccion con modelos
            HandleInteraction(keyboardState, models);

            nokiaLight?.Update(elapsedTime);
            matchLight?.Update(elapsedTime);

            // En FreeCamera la velocidad es el doble
            float moveSpeed = _freeCameraMode ? 600f : 300f;
            float turnSpeed = 3f;

            // Implementacion vista desde mouse
            if (_previousMouseState != default)
            {
                float mouseSensitivity = 0.003f;
                // Calculo cuanto se movio el mouse desde el frame anterior
                int deltaX = mouseState.X - _previousMouseState.X;
                int deltaY = mouseState.Y - _previousMouseState.Y;

                // Multiplico el valor obtenido por la sensibilidad para modificar la nueva posicion de la camara
                Rotation -= deltaX * mouseSensitivity;      // Eje X
                _cameraPitch -= deltaY * mouseSensitivity;  // Eje Y
            }
            _previousMouseState = mouseState;

            // Rotacion de la camara para modo normal con el teclado
            if (keyboardState.IsKeyDown(Keys.Left)) Rotation += turnSpeed * elapsedTime;
            if (keyboardState.IsKeyDown(Keys.Right)) Rotation -= turnSpeed * elapsedTime;

            // Rotacion de la camara arriba y abajo
            if (_freeCameraMode)
            {
                if (keyboardState.IsKeyDown(Keys.Up)) _cameraPitch += turnSpeed * elapsedTime;
                if (keyboardState.IsKeyDown(Keys.Down)) _cameraPitch -= turnSpeed * elapsedTime;
            }

            // Limitamos el pitch para no dar una vuelta completa
            _cameraPitch = MathHelper.Clamp(_cameraPitch, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);

            // Vectores de direccion
            Matrix cameraRotation = Matrix.CreateFromYawPitchRoll(Rotation, _cameraPitch, 0f);
            Vector3 forward = Vector3.Transform(Vector3.Forward, cameraRotation);
            Vector3 right = Vector3.Transform(Vector3.Right, cameraRotation);

            // Mirar hacia arriba y hacia abajo pero manteniendo la misma altura en el plano XZ
            forward.Y = 0f;
            right.Y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 movement = Vector3.Zero;
            if (!IsHidden)
            {
                if (keyboardState.IsKeyDown(Keys.W)) movement += forward * moveSpeed * elapsedTime;
                if (keyboardState.IsKeyDown(Keys.S)) movement -= forward * moveSpeed * elapsedTime;
                if (keyboardState.IsKeyDown(Keys.A)) movement -= right * moveSpeed * elapsedTime;
                if (keyboardState.IsKeyDown(Keys.D)) movement += right * moveSpeed * elapsedTime;
            }            

            // Separo los tipos de modo de movimiento del jugador
            if (_freeCameraMode || _noClipMode)
            {
                ApplyDebugMovement(keyboardState, movement, moveSpeed, elapsedTime);
            }
            else if (!IsHidden)
            {
                ApplyNormalMovement(movement);
            }

            #region Recoleccion de llaves a traves de colisiones
            // Esfera del jugador
            BoundingSphere playerPickupSphere = new BoundingSphere(Position, 25f);

            for (int i = models.Count - 1; i >= 0; i--)
            {
                if (models[i].Name.Contains("PSX_Item_Key"))
                {
                    // Esfera de colision para las llaves
                    BoundingSphere keySphere = new BoundingSphere(models[i].World.Translation, 20f);

                    // Recolecto la llave
                    if (playerPickupSphere.Intersects(keySphere))
                    {
                        CollectedKeys++;
                        models.RemoveAt(i); // Elimino la llave del mundo
                        Debug.WriteLine($"¡Llave recolectada! ({CollectedKeys}/3)");
                    }
                }
            }
            #endregion

            View = Matrix.CreateLookAt(Position, Position + Vector3.Transform(Vector3.Forward, cameraRotation), Vector3.Up);
            _previousKeyboardState = keyboardState;
        }

        public void ResetStats()
        {
            Position = new Vector3(0, 50, 150); // Posicion inicial
            CollectedKeys = 0;
            HasWon = false;
            IsHidden = false;
        }

        private void HandleToggles(KeyboardState keyboardState)
        {
            // Free Camera (Ctrl + Shift + F)
            if (keyboardState.IsKeyDown(Keys.LeftControl) &&
                keyboardState.IsKeyDown(Keys.LeftShift) &&
                keyboardState.IsKeyDown(Keys.F) &&
                _previousKeyboardState.IsKeyUp(Keys.F))
            {
                Debug.WriteLine("FreeCamera On!");
                _freeCameraMode = !_freeCameraMode;
                // Desactivo NoClip para que no haga las 2 cosas al mismo tiempo
                if (_freeCameraMode) _noClipMode = false;
            }

            // NoClip (Ctrl + Shift + C)
            if (keyboardState.IsKeyDown(Keys.LeftControl) &&
                keyboardState.IsKeyDown(Keys.LeftShift) &&
                keyboardState.IsKeyDown(Keys.C) &&
                _previousKeyboardState.IsKeyUp(Keys.C))
            {
                Debug.WriteLine("NoClip On!");
                _noClipMode = !_noClipMode;
                // Desactivo FreeCamera
                if (_noClipMode) _freeCameraMode = false;
            }

            // El jugador no puede prender las luces cuando esta escondido
            if (!IsHidden)
            {
                // Nokia (Tecla 1)
                if (keyboardState.IsKeyDown(Keys.D1) && _previousKeyboardState.IsKeyUp(Keys.D1))
                {
                    nokiaLight?.Toggle();
                    if (nokiaLight.IsActive && matchLight != null) matchLight.IsActive = false;
                }

                // Fosforo (Tecla 2)
                if (keyboardState.IsKeyDown(Keys.D2) && _previousKeyboardState.IsKeyUp(Keys.D2))
                {
                    matchLight?.Toggle();
                    if (matchLight.IsActive && nokiaLight != null) nokiaLight.IsActive = false;
                }
            }            
        }

        private void HandleInteraction(KeyboardState keyboardState, List<(Model Model, Matrix World, string Name)> models)
        {
            if (keyboardState.IsKeyDown(Keys.E) && _previousKeyboardState.IsKeyUp(Keys.E))
            {
                if (IsHidden) // Si ya esta escondido, sale del armario
                {
                    IsHidden = false;
                    Matrix cameraRotation = Matrix.CreateFromYawPitchRoll(Rotation, 0f, 0f);
                    Position += Vector3.Transform(Vector3.Forward, cameraRotation) * 40f;
                    Debug.WriteLine("Saliste del escondite");
                    return;
                }

                int? modelIndexToRemove = null; // Se usa para borrar luego el modelo de match box con el que el jugador interactue

                // En vez de buscar por todos los modelos nuevamente,
                // Solo utilizo el valor que ya averigue anteriormente
                if (InteractableModelIndex.HasValue)
                {
                    var model = models[InteractableModelIndex.Value];

                    if (model.Name.Contains("PSX_Wooden_Closet"))
                    {
                        IsHidden = true;
                        // Teletransporto al jugador al centro del modelo
                        Position = new Vector3(model.World.Translation.X, 50f, model.World.Translation.Z);

                        // Apago las luces, por si ya las tenia prendidas el jugador en el momento de interactuar
                        if (nokiaLight != null) nokiaLight.IsActive = false;
                        if (matchLight != null) matchLight.IsActive = false;

                        Debug.WriteLine("Te escondiste en el armario!");
                    }
                    else if (model.Name.Contains("PSX_Item_Match_Box"))
                    {
                        // Me fijo la carga actual de matchLight
                        if (matchLight != null && matchLight.Durability <= 0f)
                        {
                            matchLight.Durability = matchLight.MaxDurability; // Recargo la durabilidad al maximo
                            models.RemoveAt(InteractableModelIndex.Value); // Borro el item directamente de la lista con el indice ya conocido

                            InteractableModelIndex = null;
                            Debug.WriteLine("Recogiste una caja de fosforos!");
                        }
                        else
                        {
                            Debug.WriteLine("Aun no se te acabaron los fosforos");
                        }
                    }
                    
                    else if (model.Name.Contains("PSX_Item_Lock_Locked") || model.Name.Contains("PSX_Item_Door"))
                    {
                        var lockIndices = new List<int>();
                        for (int i = 0; i < models.Count; i++)
                        {
                            if (models[i].Name.Contains("PSX_Item_Lock_Locked") || models[i].Name.Contains("PSX_Item_Lock_Open"))
                            {
                                lockIndices.Add(i);
                            }
                        }

                        // Ordeno de abajo hacia arriba en el eje Y
                        lockIndices.Sort((a, b) => models[a].World.Translation.Y
                                                    .CompareTo(models[b].World.Translation.Y));

                        // Reviso la lista de candados y segun la cantidad de llaves se abren
                        for (int i = 0; i < lockIndices.Count; i++)
                        {
                            int index = lockIndices[i];
                            bool hasEnoughKeys = CollectedKeys >= (i + 1);
                            bool isLocked = models[index].Name.Contains("PSX_Item_Lock_Locked");

                            // Si hay suficientes llaves para este candado y sigue cerrado, lo abrimos
                            if (hasEnoughKeys && isLocked)
                            {
                                Matrix world = models[index].World;
                                Vector3 position = world.Translation;

                                // Centro, lo escalo y lo vuelvo al lugar donde deberia de aparecer
                                world.Translation = Vector3.Zero;
                                world = Matrix.CreateScale(5f) * world;
                                world.Translation = position;

                                // Guardamos el nuevo estado del candado
                                models[index] = (_lockOpenModel, world, "Items/PSX_Item_Lock_Open");
                                Debug.WriteLine($"¡Candado {i + 1} desbloqueado!");
                            }
                        }

                        // Interaccion con la puerta del premio
                        if (model.Name.Contains("PSX_Item_Door"))
                        {
                            // Valido que todos los candados esten abiertos
                            bool allLocksOpen = lockIndices.Count > 0 &&
                                                lockIndices.All(idx => !models[idx].Name.Contains("PSX_Item_Lock_Locked"));

                            if (allLocksOpen)
                            {
                                Debug.WriteLine("¡Ganaste, conseguiste el premio!");
                                HasWon = true;

                                // Tomamos el modelo de la puerta actual
                                var door = models[InteractableModelIndex.Value];
                                Vector3 doorPosition = door.World.Translation;

                                // Roto 90° sobre su eje para abrirla
                                door.World.Translation = Vector3.Zero;
                                door.World *= Matrix.CreateRotationY(-MathHelper.PiOver2);
                                door.World.Translation = doorPosition;

                                // Cambio el nombre y guardo la puerta modificada
                                door.Name = "Items/PSX_Item_Door_Opened";
                                models[InteractableModelIndex.Value] = door;

                                InteractableModelIndex = null;
                            }
                            else
                            {
                                Debug.WriteLine("La puerta está firmemente trabada por los candados.");
                            }
                        }
                    }
                }

                // Elimino la caja de la lista
                if (modelIndexToRemove.HasValue)
                {
                    models.RemoveAt(modelIndexToRemove.Value);
                }
            }
        }

        private void ApplyDebugMovement(KeyboardState keyboardState, Vector3 movement, float moveSpeed, float elapsedTime)
        {
            // FreeCamera
            if (_freeCameraMode)
            {
                // Ascenso y descenso
                if (keyboardState.IsKeyDown(Keys.E)) Position += Vector3.Up * moveSpeed * elapsedTime;
                if (keyboardState.IsKeyDown(Keys.Q)) Position -= Vector3.Up * moveSpeed * elapsedTime;

                Position += movement;

                // Evitamos atravesar el piso
                if (Position.Y < 10f) Position = new Vector3(Position.X, 10f, Position.Z);
            }
            // NoClip
            else if (_noClipMode)
            {
                // El jugador siempre se mantiene pegado al suelo
                Position = new Vector3(Position.X, 50f, Position.Z);
                Position += movement;
            }
        }

        private void ApplyNormalMovement(Vector3 movement)
        {
            // El jugador siempre se mantiene pegado al suelo en modo normal
            Position = new Vector3(Position.X, 50f, Position.Z);

            Vector3 newPosX = new Vector3(Position.X + movement.X, Position.Y, Position.Z);
            if (!IsColliding(newPosX)) Position = newPosX;

            Vector3 newPosZ = new Vector3(Position.X, Position.Y, Position.Z + movement.Z);
            if (!IsColliding(newPosZ)) Position = newPosZ;
        }

        private bool IsColliding(Vector3 targetPosition)
        {
            BoundingSphere playerSphere = new BoundingSphere(targetPosition, 10f);

            foreach (var box in Helpers.LevelGeneratorHelper.WallColliders)
            {
                if (playerSphere.Intersects(box))
                {
                    return true;
                }
            }
            return false;
        }
    }
}