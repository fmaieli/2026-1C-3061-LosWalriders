using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.SourceCode.Components
{
    internal class NokiaLight : LightSource
    {
        public NokiaLight()
        {
            MaxDurability = 100f;
            Durability = 100f;
            DecayRate = 2.5f;     // Se pierden 2.5 unidades por segundo
            LightIntensity = 1f;  // Ilumina al 100% - los fosforos podrian ser 0.3/0.4 para crear una diferencia
            IsActive = false;
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
                    fx.Parameters["UseVertexColor"]?.SetValue(false);
                    fx.Parameters["DiffuseColor"]?.SetValue(Color.White.ToVector3());
                }

                mesh.Draw();
            }
        }
    }
}