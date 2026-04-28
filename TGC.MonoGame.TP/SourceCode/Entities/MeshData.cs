using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP.SourceCode.Entities
{
    public class MeshData
    {
        public VertexPositionColor[] Vertices { get; }
        public ushort[] Indices { get; }

        public MeshData(VertexPositionColor[] vertices, ushort[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}
