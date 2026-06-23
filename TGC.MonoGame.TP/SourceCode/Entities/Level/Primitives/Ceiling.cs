using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.SourceCode.Geometries;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Primitives
{
    public class Ceiling
    {
        public VertexPositionNormalColorTexture[] CreateCeiling(float width, float height, float depth, Color color)
        {
            // Calculado para las texturas y se repitan correctamente
            float u = (width * 2) / 50f;
            float v = (depth * 2) / 50f;
            Vector3 normal = Vector3.Down;

            return new VertexPositionNormalColorTexture[] {
                new(new Vector3(-width, height, -depth), normal, color, new Vector2(0, 0)),
                new(new Vector3(width, height, -depth), normal, color, new Vector2(u, 0)),
                new(new Vector3(width, height, depth), normal, color, new Vector2(u, v)),
                new(new Vector3(-width, height, depth), normal, color, new Vector2(0, v))
            };
        }
    }
}
