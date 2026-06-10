using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TGC.MonoGame.TP.SourceCode.Geometries
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

    public class MeshDataWithOpenings
    {
        public VertexPositionColor[] Vertices;
        public ushort[] Indices;
        public List<Vector3> OpeningCenters;

        public MeshDataWithOpenings(VertexPositionColor[] vertices, ushort[] indices, List<Vector3> centers)
        {
            Vertices = vertices;
            Indices = indices;
            OpeningCenters = centers;
        }
    }
}
