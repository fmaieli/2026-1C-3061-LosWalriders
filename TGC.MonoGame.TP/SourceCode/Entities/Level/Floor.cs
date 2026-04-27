using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level
{
    public class Floor
    {
        public VertexPositionColor[] CreateFloor(float width, float depth) 
        {
            var floorVertices = new VertexPositionColor[4];

            floorVertices[0] = new VertexPositionColor(new Vector3(-width, 0, -depth), Color.Yellow);
            floorVertices[1] = new VertexPositionColor(new Vector3(width, 0, -depth), Color.Yellow);
            floorVertices[2] = new VertexPositionColor(new Vector3(width, 0, depth), Color.Yellow);
            floorVertices[3] = new VertexPositionColor(new Vector3(-width, 0, depth), Color.Yellow);

            return floorVertices;
        }
    }
}
