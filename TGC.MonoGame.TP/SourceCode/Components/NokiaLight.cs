using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP.SourceCode.Components
{
    internal class NokiaLight : LightSource
    {
        public NokiaLight()
        {
            MaxDurability = 180f;   // Aumento el maximo de durabilidad
            Durability = 180f;      // Durabilidad inicial = a maximo
            DecayRate = 2f;         // Se pierden 2 unidades por segundo
            LightIntensity = 1f;    // Ilumina al 100% - los fosforos podrian ser 0.3/0.4 para crear una diferencia
            IsActive = false;

            LightColor = new Vector3(0.9f, 0.9f, 1.0f); // Blanco con algo de azul
            LightIntensity = 1.2f;
            LightRadius = 400f;
            IsSpotLight = true;
            SpotAngle = (float)Math.Cos(MathHelper.ToRadians(25f)); // Calculo del coseno para utilizarlo directamente en el shader
        }

        public override void LoadContent(ContentManager content, Effect baseEffect)
        {
            Model = content.Load<Model>("Models/Items/PSX_Nokia");
            Effect = baseEffect;

            foreach (var mesh in Model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                    part.Effect = Effect.Clone();
            }
        }

        public override void Draw(Matrix view, Matrix projection, Matrix cameraWorld)
        {
            if (!IsActive || Model == null) return;

            float rotacionX = MathHelper.ToRadians(-90f);
            float rotacionY = MathHelper.ToRadians(0f);
            float rotacionZ = MathHelper.ToRadians(0f);

            Vector3 nokiaOffset = new Vector3(12f, -1.75f, -50f);

            Matrix nokiaWorld = Matrix.CreateScale(0.05f) * Matrix.CreateRotationX(rotacionX) *
                                Matrix.CreateRotationY(rotacionY) *
                                Matrix.CreateRotationZ(rotacionZ) *
                                Matrix.CreateTranslation(nokiaOffset) *
                                cameraWorld;

            foreach (var mesh in Model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    var fx = (Effect)part.Effect;
                    fx.Parameters["World"]?.SetValue(nokiaWorld);
                    fx.Parameters["View"]?.SetValue(view);
                    fx.Parameters["Projection"]?.SetValue(projection);
                    fx.Parameters["UseVertexColor"]?.SetValue(0.0f);
                    // Apagamos la iluminación ambiental para el celular
                    fx.Parameters["IsLightActive"]?.SetValue(0.0f);
                    fx.Parameters["DiffuseColor"]?.SetValue(Color.White.ToVector3() * 10f);
                }

                mesh.Draw();
            }
        }
    }
}