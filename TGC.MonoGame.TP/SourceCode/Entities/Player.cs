using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.SourceCode.Entities
{
    internal class Player
    {
        public Vector3 Position { get; private set; } = new Vector3(0, 50, 150);
        public float Rotation { get; private set; } = 0f;

        // Variables de camara Free (para debuguear y ver rapidamente el nivel generado)
        private float _cameraPitch = 0f;
        private bool _freeCameraMode = false;
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;

        public Matrix View { get; private set; }

        public void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            if (keyboardState.IsKeyDown(Keys.LeftControl) &&
                keyboardState.IsKeyDown(Keys.LeftShift) &&
                keyboardState.IsKeyDown(Keys.F) &&
                _previousKeyboardState.IsKeyUp(Keys.F))
            {
                _freeCameraMode = !_freeCameraMode;
            }

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

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

            // Rotacion de la camara para modo normal
            if (keyboardState.IsKeyDown(Keys.Left)) Rotation += turnSpeed * elapsedTime;
            if (keyboardState.IsKeyDown(Keys.Right)) Rotation -= turnSpeed * elapsedTime;

            // Rotacion de la camara arriba y abajo
            if (_freeCameraMode)
            {
                if (keyboardState.IsKeyDown(Keys.Up)) _cameraPitch += turnSpeed * elapsedTime;
                if (keyboardState.IsKeyDown(Keys.Down)) _cameraPitch -= turnSpeed * elapsedTime;

                // Limitamos el pitch para no dar una vuelta completa
                _cameraPitch = MathHelper.Clamp(_cameraPitch, -MathHelper.PiOver2 + 0.01f, MathHelper.PiOver2 - 0.01f);
            }

            // Movimiento vertical
            if (_freeCameraMode)
            {
                // Subir y bajar en el eje Y
                if (keyboardState.IsKeyDown(Keys.E)) Position += Vector3.Up * moveSpeed * elapsedTime;
                if (keyboardState.IsKeyDown(Keys.Q)) Position -= Vector3.Up * moveSpeed * elapsedTime;
            }
            else
            {
                // En modo normal, el jugador se queda al ras del suelo, se reinicia la posicion
                Position = new Vector3(Position.X, 50f, Position.Z);
            }

            Matrix cameraRotation = Matrix.CreateFromYawPitchRoll(Rotation, _cameraPitch, 0f);
            Vector3 forward = Vector3.Transform(Vector3.Forward, cameraRotation);
            Vector3 right = Vector3.Transform(Vector3.Right, cameraRotation);

            // Mirar hacia arriba y hacia abajo pero manteniendo la misma altura
            forward.Y = 0f;
            right.Y = 0f;
            // Normalizo los vectores para que no se note la diferencia de velocidad al mirar hacia arriba o abajo
            forward.Normalize();
            right.Normalize();

            if (keyboardState.IsKeyDown(Keys.W)) Position += forward * moveSpeed * elapsedTime;
            if (keyboardState.IsKeyDown(Keys.S)) Position -= forward * moveSpeed * elapsedTime;
            if (keyboardState.IsKeyDown(Keys.A)) Position -= right * moveSpeed * elapsedTime;
            if (keyboardState.IsKeyDown(Keys.D)) Position += right * moveSpeed * elapsedTime;

            // Anteriormente UpdateViewMatrix
            View = Matrix.CreateLookAt(Position, Position + Vector3.Transform(Vector3.Forward, cameraRotation), Vector3.Up);

            // Guardado del estado del teclado para proximo frame - toggle de teclas
            _previousKeyboardState = keyboardState;
        }
    }
}