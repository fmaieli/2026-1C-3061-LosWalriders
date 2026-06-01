using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.SourceCode.Components;

namespace TGC.MonoGame.TP.SourceCode.Entities
{
    internal class Player
    {
        public Vector3 Position { get; private set; } = new Vector3(0, 50, 150);
        public float Rotation { get; private set; } = 0f;

        // Variables de camara Free y No Clip (para debuguear)
        private float _cameraPitch = 0f;
        private bool _freeCameraMode = false;
        private bool _noClipMode = false;

        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

        public Matrix View { get; private set; }

        private Model _armsModel;
        private Effect _armsEffect;

        private LightSource nokiaFlashlight;

        public void LoadContent(ContentManager content, Effect effect)
        {
            _armsEffect = content.Load<Effect>("Effects/ArmsShader");
            _armsModel = content.Load<Model>("Models/Player/PSX_Player_Arms");

            // Se aplica Clone a todas las partes del mesh para aplicarle el custom effect
            foreach (var mesh in _armsModel.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                    part.Effect = _armsEffect.Clone();
            }

            nokiaFlashlight = new NokiaFlashlight();
            nokiaFlashlight.LoadContent(content, effect);
        }

        public void DrawArms(Matrix view, Matrix projection, GraphicsDevice graphicsDevice)
        {
            if (_armsModel == null) return;

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

            // Dibujo el objeto Nokias
            nokiaFlashlight?.Draw(view, projection, cameraWorld);
        }

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 1. Manejo de Toggles (Estados)
            HandleToggles(keyboardState);

            nokiaFlashlight?.Update(elapsedTime);

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

            // Mirar hacia arriba y hacia abajo pero manteniendo la misma altura
            forward.Y = 0f;
            right.Y = 0f;
            // Normalizo los vectores para que no se note la diferencia de velocidad al mirar hacia arriba o abajo
            forward.Normalize();
            right.Normalize();

            Vector3 movement = Vector3.Zero;
            if (keyboardState.IsKeyDown(Keys.W)) movement += forward * moveSpeed * elapsedTime;
            if (keyboardState.IsKeyDown(Keys.S)) movement -= forward * moveSpeed * elapsedTime;
            if (keyboardState.IsKeyDown(Keys.A)) movement -= right * moveSpeed * elapsedTime;
            if (keyboardState.IsKeyDown(Keys.D)) movement += right * moveSpeed * elapsedTime;

            // Debug
            DebugMovementMode(keyboardState, forward, right, moveSpeed, elapsedTime);

            View = Matrix.CreateLookAt(Position, Position + Vector3.Transform(Vector3.Forward, cameraRotation), Vector3.Up);
            _previousKeyboardState = keyboardState;
        }

        private void HandleToggles(KeyboardState keyboardState)
        {
            // Free Camera (Ctrl + Shift + F)
            if (keyboardState.IsKeyDown(Keys.LeftControl) &&
                keyboardState.IsKeyDown(Keys.LeftShift) &&
                keyboardState.IsKeyDown(Keys.F) &&
                _previousKeyboardState.IsKeyUp(Keys.F))
            {
                _freeCameraMode = !_freeCameraMode;
            }

            // NoClip (Ctrl + Shift + C)
            if (keyboardState.IsKeyDown(Keys.LeftControl) &&
                keyboardState.IsKeyDown(Keys.LeftShift) &&
                keyboardState.IsKeyDown(Keys.C) &&
                _previousKeyboardState.IsKeyUp(Keys.C))
            {
                _noClipMode = !_noClipMode;
            }

            // Luz (Tecla 1)
            if (keyboardState.IsKeyDown(Keys.D1) && _previousKeyboardState.IsKeyUp(Keys.D1))
            {
                nokiaFlashlight?.Toggle();
            }
        }

        private void DebugMovementMode(KeyboardState keyboardState, Vector3 forward, Vector3 right, float moveSpeed, float elapsedTime)
        {
            Vector3 movement = Vector3.Zero;
            
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
            else
            {
                // El jugador siempre se mantiene pegado al suelo
                Position = new Vector3(Position.X, 50f, Position.Z);

                if (_noClipMode)
                {
                    // NoClip ignora la validación de paredes
                    Position += movement;
                }
                else
                {
                    Vector3 newPosX = new Vector3(Position.X + movement.X, Position.Y, Position.Z);
                    if (!IsColliding(newPosX)) Position = newPosX;

                    Vector3 newPosZ = new Vector3(Position.X, Position.Y, Position.Z + movement.Z);
                    if (!IsColliding(newPosZ)) Position = newPosZ;
                }
            }
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