using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP.SourceCode.Components
{
    internal class MatchLight : LightSource
    {
        public MatchLight()
        {
            MaxDurability = 18f;   // Dura solo 18 segundos
            Durability = 18f;
            DecayRate = 1f;        // Pierde 1 punto por segundo
            LightIntensity = 0.4f; // Ilumina menos que la linterna
            IsActive = false;

            LightColor = new Vector3(1.0f, 0.6f, 0.2f); // Naranja calido
            LightRadius = 150f;
            IsSpotLight = false;
        }

        public override void LoadContent(ContentManager content, Effect baseEffect)
        {
            Model = content.Load<Model>("Models/Items/PSX_Item_Match");

            foreach (var mesh in Model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    Texture2D modelTexture = null;
                    var originalEffect = part.Effect;
                    var textureParam = originalEffect?.Parameters["Texture"];
                    if (textureParam != null && textureParam.ParameterType == EffectParameterType.Texture2D)
                        modelTexture = textureParam.GetValueTexture2D();

                    var fx = baseEffect.Clone();

                    if (modelTexture != null)
                        fx.Parameters["MainTexture"]?.SetValue(modelTexture);

                    part.Effect = fx;
                }
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            if (IsActive)
            {
                // Efecto de fuego: varia la intensidad usando el seno, utilizo TickCount para no tener que crear un cronometro propio y calcular a partir de ElapsedGameTime
                // Intensidad base de la luz + seno(TickCount) * 0.1f => 0.5 < X < 0.7
                LightIntensity = 0.6f + (float)Math.Sin(Environment.TickCount * 0.01f) * 0.1f;
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
                    fx.Parameters["UseVertexColor"]?.SetValue(0.0f);
                    // Sin luz ambiental para que el fosforo sea brillante
                    fx.Parameters["IsLightActive"]?.SetValue(0.0f);
                    // Multiplico x10 por culpa de la oscuridad del shader base
                    fx.Parameters["DiffuseColor"]?.SetValue(Color.Orange.ToVector3() * 10f);
                }

                mesh.Draw();
            }
        }
    }
}