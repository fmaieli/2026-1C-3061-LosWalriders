using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level
{
    public class Ceiling
    {
        public VertexPositionColor[] CreateCeiling(float width, float height, float depth)
        {
            var ceilingVertices = new VertexPositionColor[4];

            ceilingVertices[0] = new VertexPositionColor(new Vector3(-width, height, -depth), Color.Yellow);
            ceilingVertices[1] = new VertexPositionColor(new Vector3(width, height, -depth), Color.Yellow);
            ceilingVertices[2] = new VertexPositionColor(new Vector3(width, height, depth), Color.Yellow);
            ceilingVertices[3] = new VertexPositionColor(new Vector3(-width, height, depth), Color.Yellow);

            return ceilingVertices;
        }
    }
}
