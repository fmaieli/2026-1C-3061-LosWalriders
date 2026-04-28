using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Primitives
{
    public class Floor
    {
        public VertexPositionColor[] CreateFloor(float width, float depth, Color color) 
        {
            var floorVertices = new VertexPositionColor[4];

            floorVertices[0] = new VertexPositionColor(new Vector3(-width, 0, -depth), color);
            floorVertices[1] = new VertexPositionColor(new Vector3(width, 0, -depth), color);
            floorVertices[2] = new VertexPositionColor(new Vector3(width, 0, depth), color);
            floorVertices[3] = new VertexPositionColor(new Vector3(-width, 0, depth), color);

            return floorVertices;
        }
    }
}
