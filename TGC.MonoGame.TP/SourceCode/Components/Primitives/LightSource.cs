using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.SourceCode.Components
{
    internal abstract class LightSource
    {
        // Propiedades de la luz
        public float MaxDurability { get; protected set; }
        public float Durability { get; protected set; }
        public float DecayRate { get; protected set; }      // Durabilidad que va perdiendo por segundo
        public float LightIntensity { get; protected set; } // Intensidad de la luz

        public bool IsActive { get; protected set; }

        protected Model Model;
        protected Effect Effect;

        public virtual void Toggle()
        {
            // Solo se puede prender si le queda bateria
            if (Durability > 0)
            {
                IsActive = !IsActive;
            }
            else
            {
                IsActive = false;
            }
        }

        public virtual void Update(float deltaTime)
        {
            if (IsActive)
            {
                // Resto durabilidad con el paso del tiempo
                Durability -= DecayRate * deltaTime;

                if (Durability <= 0)
                {
                    Durability = 0;
                    IsActive = false; // Se apaga sola cuando se agota
                }
            }
        }

        // Los modelos deben de implementar estos metodos para poder funcionar
        public abstract void LoadContent(ContentManager content, Effect baseEffect);
        public abstract void Draw(Matrix view, Matrix projection, Matrix cameraWorld);
    }
}