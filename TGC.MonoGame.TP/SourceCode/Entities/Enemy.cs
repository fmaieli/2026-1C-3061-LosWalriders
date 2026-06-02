using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Helpers;

namespace TGC.MonoGame.TP.SourceCode.Entities
{
    internal class Enemy
    {
        public Vector3 Position { get; set; }
        public Vector3 Forward { get; private set; } = Vector3.Forward;
        public EnemyState State { get; private set; } = EnemyState.Roaming;

        private float _roamSpeed = 80f;                         // Velocidad normal
        private float _chaseSpeed = 120f;                       // Velocidad persiguiendo al jugador
        private float _visionRadius = 400f;                     // Radio de vision
        private float _visionConeAngle = MathHelper.PiOver4;    // Cono de vision = 45° -> 45 a la izquierda y 45 a la derecha = 90° el cono de vision actual
        private float _catchRadius = 25f;

        // Tiempos
        private float _cooldownTimer = 5f;          // Enunciado Entrega 2: Pasado un tiempo arbitrario que consideren justo, este enemigo volverá a modo recorrido.
        private float _cooldownDuration = 5f;
        private float _changeDirectionTimer = 0f;   // Se usa para poder ir cambiando la direccion en la que el enemigo camina

        // Variables del modelo y efecto
        private Model _model;
        private Effect _effect;
        private float _scale = 0.5f;

        public void LoadContent(ContentManager content, Effect baseEffect)
        {
            _model = content.Load<Model>("Models/Enemy/Enemy_Barney");
            _effect = baseEffect;

            foreach (var mesh in _model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = _effect.Clone();
                }
            }
        }

        public void Draw(Matrix view, Matrix projection)
        {
            if (_model == null) return;

            float rotationY = (float)Math.Atan2(Forward.X, Forward.Z);

            Matrix world = Matrix.CreateScale(_scale) * Matrix.CreateRotationY(rotationY) * Matrix.CreateTranslation(Position);

            foreach (var mesh in _model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    var fx = (Effect)part.Effect;
                    fx.Parameters["World"]?.SetValue(mesh.ParentBone.Transform * world);
                    fx.Parameters["View"]?.SetValue(view);
                    fx.Parameters["Projection"]?.SetValue(projection);
                    fx.Parameters["UseVertexColor"]?.SetValue(false);                    
                    fx.Parameters["DiffuseColor"]?.SetValue(Color.DarkRed.ToVector3());
                }

                mesh.Draw();
            }
        }

        public void Update(GameTime gameTime, Vector3 playerPosition)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // A partir de EnemyState se ve en que estado se encuentra el enemigo y su logica
            switch (State)
            {
                case EnemyState.Cooldown:
                    _cooldownTimer -= elapsedTime;
                    if (_cooldownTimer <= 0f)
                    {
                        RespawnNearby(playerPosition);
                        State = EnemyState.Roaming;
                    }
                    break;

                case EnemyState.Roaming:
                    Roam(elapsedTime);
                    if (CanSeePlayer(playerPosition))
                    {
                        State = EnemyState.Chasing;
                    }
                    break;

                case EnemyState.Chasing:
                    Chase(elapsedTime, playerPosition);

                    if (Vector3.Distance(Position, playerPosition) < _catchRadius)
                    {
                        CatchPlayer();
                    }
                    else if (!CanSeePlayer(playerPosition))
                    {
                        State = EnemyState.Roaming;
                    }
                    break;
            }
        }

        private void Roam(float elapsedTime)
        {
            _changeDirectionTimer -= elapsedTime;
            if (_changeDirectionTimer <= 0f)
            {
                Random rng = new Random();
                float angle = (float)rng.NextDouble() * MathHelper.TwoPi;                   // A partir de un valor random calcula un nuevo angulo para donde dirigirse

                Forward = new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));   // A partir de este nuevo angulo utilizo Forward para que sepa hacia donde debe de ir
                _changeDirectionTimer = 3f + (float)rng.NextDouble() * 2f;                  // Entre 3 y 5 segundos para que vaya cambiando la direccion
            }

            if (!Move(Forward * _roamSpeed * elapsedTime))  // Utilizo Move para comprobar las colisiones y ver si tengo que reiniciar el valor a 0 para cambiar de direccion
            {
                _changeDirectionTimer = 0f;
            }
        }

        private void Chase(float elapsedTime, Vector3 target)
        {
            Vector3 direction = target - Position;
            direction.Y = 0;

            direction.Normalize();

            Forward = direction;
            Move(Forward * _chaseSpeed * elapsedTime); // Me muevo en la direccion calculada anteriormente con mas rapido ya que esta persiguiendo al jugador
        }

        private bool Move(Vector3 delta)
        {
            bool moved = false; // Variable para comprobar si el enemigo se movio o no

            Vector3 newPosX = new Vector3(Position.X + delta.X, Position.Y, Position.Z);    // Me muevo en el eje X
            if (!IsColliding(newPosX)) { Position = newPosX; moved = true; }                // Si no colisiono entonces devuelvo true

            Vector3 newPosZ = new Vector3(Position.X, Position.Y, Position.Z + delta.Z);    // Me muevo en el eje Z
            if (!IsColliding(newPosZ)) { Position = newPosZ; moved = true; }                // Si no colisiono entonces devuelvo true

            return moved;   // En caso de haber colisionado entonces devuelve el valor por defecto que es false
        }        

        private bool CanSeePlayer(Vector3 playerPos)
        {
            // Medir la distancia con el jugador
            Vector3 directionToPlayer = playerPos - Position;
            float distance = directionToPlayer.Length();        // Distancia hasta el jugador

            if (distance > _visionRadius) return false;         // Fuera del alcance del radio de vision

            directionToPlayer.Normalize();
            float dot = Vector3.Dot(Forward, directionToPlayer);    
            dot = MathHelper.Clamp(dot, -1f, 1f);
            float angleToPlayer = (float)Math.Acos(dot);            // Angulo de vision entre el enemigo y el jugador

            if (angleToPlayer > _visionConeAngle) return false;     // Fuera del angulo de la vision

            // Logica para comprobar si hay una pared en el medio
            Ray sightRay = new Ray(Position, directionToPlayer);                            // Se crea un rayo apuntando hacia la direccion donde se encuentra el jugador en el momento
            foreach (var box in LevelGeneratorHelper.WallColliders)
            {
                float? intersectionDistance = sightRay.Intersects(box);                     // Me fijo si el rayo choca contra alguna pared dentro del nivel
                if (intersectionDistance.HasValue && intersectionDistance.Value < distance) // Si tiene un valor y choco contra una pared Y la pared se encuentra a una distancia menor que el jugador
                {                                                                           // Entonces significa que hay una pared entre medio del jugador y el enemigo bloqueando
                    return false;
                }
            }

            return true; // Si todo lo otro no devolvio false entonces el enemigo te descubrio
        }

        private void CatchPlayer()
        {
            Debug.WriteLine("Barney te atrapo!");
            State = EnemyState.Cooldown;
            _cooldownTimer = _cooldownDuration;
            Position = new Vector3(0, -1000f, 0);
        }

        private void RespawnNearby(Vector3 playerPos)
        {
            if (LevelGeneratorHelper.ValidSpawnPoints.Count == 0) return;

            Random rng = new Random();
            List<Vector3> candidateSpawns = new List<Vector3>();

            // Filtramos las habitaciones para encontrar las que están a una distancia media
            foreach (var spawnPoint in LevelGeneratorHelper.ValidSpawnPoints)
            {
                float distanceToPlayer = Vector3.Distance(spawnPoint, playerPos);

                // Considero los spawn validos entre los 500 y 1200 unidades de distancia
                if (distanceToPlayer >= 400f && distanceToPlayer <= 800f)
                {
                    candidateSpawns.Add(spawnPoint);
                }
            }

            Vector3 chosenSpawn;

            if (candidateSpawns.Count > 0)
            {
                // Se elije al azar la habitacion de spawn
                chosenSpawn = candidateSpawns[rng.Next(candidateSpawns.Count)];
            }
            else
            {
                // En caso de que no tenga opciones tomo todos los puntos validos y randomizo para tomar uno
                chosenSpawn = LevelGeneratorHelper.ValidSpawnPoints[rng.Next(LevelGeneratorHelper.ValidSpawnPoints.Count)];
            }

            // Offset entre -50 y 50 para que sea un poco mas randomizado
            float offsetX = (float)(rng.NextDouble() * 100f - 50f);
            float offsetZ = (float)(rng.NextDouble() * 100f - 50f);

            Debug.WriteLine("Barney se volvio a generar!");
            Position = new Vector3(chosenSpawn.X + offsetX, 50f, chosenSpawn.Z + offsetZ);
        }

        // Misma logica que en Player para verificar si esta colisionando con las paredes
        private bool IsColliding(Vector3 pos)
        {
            BoundingSphere sphere = new BoundingSphere(pos, 15f);
            foreach (var box in LevelGeneratorHelper.WallColliders)
            {
                if (sphere.Intersects(box)) return true;
            }
            return false;
        }
    }
}