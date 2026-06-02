using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.SourceCode.Components
{
    internal class MatchLight : LightSource
    {
        public MatchLight()
        {
            MaxDurability = 15f;   // Dura solo 15 segundos
            Durability = 15f;
            DecayRate = 1f;        // Pierde 1 punto por segundo
            LightIntensity = 0.4f; // Ilumina menos que la linterna
            IsActive = false;
        }

        public override void LoadContent(ContentManager content, Effect baseEffect)
        {
            Model = content.Load<Model>("Models/Items/PSX_Item_Match");
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

            Vector3 matchOffset = new Vector3(16f, -3f, -43f);

            Matrix matchWorld = Matrix.CreateScale(0.06f) * Matrix.CreateRotationX(rotacionX) *
                                Matrix.CreateRotationY(rotacionY) *
                                Matrix.CreateRotationZ(rotacionZ) *
                                Matrix.CreateTranslation(matchOffset) *
                                cameraWorld;

            foreach (var mesh in Model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    var fx = (Effect)part.Effect;
                    fx.Parameters["World"]?.SetValue(matchWorld);
                    fx.Parameters["View"]?.SetValue(view);
                    fx.Parameters["Projection"]?.SetValue(projection);
                    fx.Parameters["UseVertexColor"]?.SetValue(false);
                    // Color naranja/rojizo simula a fuego (ponele)
                    fx.Parameters["DiffuseColor"]?.SetValue(Color.Orange.ToVector3());
                }

                mesh.Draw();
            }
        }
    }
}