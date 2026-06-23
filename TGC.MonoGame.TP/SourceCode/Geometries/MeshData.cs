using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TGC.MonoGame.TP.SourceCode.Geometries
{
    public class MeshData
    {
        public VertexPositionNormalColorTexture[] Vertices { get; set; }
        public ushort[] Indices { get; }

        public MeshData(VertexPositionNormalColorTexture[] vertices, ushort[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }

    public class MeshDataWithOpenings
    {
        public VertexPositionNormalColorTexture[] Vertices { get; set; }
        public ushort[] Indices;
        public List<Vector3> OpeningCenters;

        public MeshDataWithOpenings(VertexPositionNormalColorTexture[] vertices, ushort[] indices, List<Vector3> centers)
        {
            Vertices = vertices;
            Indices = indices;
            OpeningCenters = centers;
        }
    }
}
