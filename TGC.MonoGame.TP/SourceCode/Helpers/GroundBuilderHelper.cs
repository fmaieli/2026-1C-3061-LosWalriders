using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.SourceCode.Helpers
{
    public static class GroundBuilderHelper
    {
        public static (VertexBuffer GroundVertexBuffer, IndexBuffer GroundIndexBuffer, int PrimitiveCount) Create(
            GraphicsDevice graphicsDevice, float halfWidth, float halfDepth, Color color)
        {
            var vertices = new VertexPositionColor[4];
            vertices[0] = new VertexPositionColor(new Vector3(-halfWidth, 0f, -halfDepth), color);
            vertices[1] = new VertexPositionColor(new Vector3(halfWidth, 0f, -halfDepth), color);
            vertices[2] = new VertexPositionColor(new Vector3(halfWidth, 0f, halfDepth), color);
            vertices[3] = new VertexPositionColor(new Vector3(-halfWidth, 0f, halfDepth), color);

            var indices = new ushort[] { 0, 1, 2, 0, 2, 3 };

            var groundVertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
            groundVertexBuffer.SetData(vertices);

            var groundIndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            groundIndexBuffer.SetData(indices);

            int primitiveCount = indices.Length / 3;

            return (groundVertexBuffer, groundIndexBuffer, primitiveCount);
        }
    }
}