using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.SourceCode.Geometries;

namespace TGC.MonoGame.TP.SourceCode.Helpers
{
    public static class GroundBuilderHelper
    {
        public static (VertexBuffer GroundVertexBuffer, IndexBuffer GroundIndexBuffer, int PrimitiveCount) Create(
            GraphicsDevice graphicsDevice, float halfWidth, float halfDepth, Color color)
        {
            var vertices = new VertexPositionNormalColorTexture[4];

            // La normal del suelo siempre apunta hacia arriba
            Vector3 normal = Vector3.Up;

            // Calculado para las texturas y se repitan correctamente
            float u = (halfWidth * 2) / 50f;
            float v = (halfDepth * 2) / 50f;

            vertices[0] = new VertexPositionNormalColorTexture(new Vector3(-halfWidth, 0f, -halfDepth), normal, color, new Vector2(0, 0));
            vertices[1] = new VertexPositionNormalColorTexture(new Vector3(halfWidth, 0f, -halfDepth), normal, color, new Vector2(u, 0));
            vertices[2] = new VertexPositionNormalColorTexture(new Vector3(halfWidth, 0f, halfDepth), normal, color, new Vector2(u, v));
            vertices[3] = new VertexPositionNormalColorTexture(new Vector3(-halfWidth, 0f, halfDepth), normal, color, new Vector2(0, v));

            var indices = new ushort[] { 0, 1, 2, 0, 2, 3 };

            var groundVertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalColorTexture), vertices.Length, BufferUsage.WriteOnly);
            groundVertexBuffer.SetData(vertices);

            var groundIndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            groundIndexBuffer.SetData(indices);

            int primitiveCount = indices.Length / 3;

            return (groundVertexBuffer, groundIndexBuffer, primitiveCount);
        }
    }
}