using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Geometries;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Primitives
{
    public class Room
    {
        public readonly Floor _floor;
        public Ceiling _ceiling;
        public Wall _wall;

        // Centros de agujeros (ventanas/puertas) => para poder centrar modelos de ventanas o puertas mas tarde
        public List<Vector3> OpeningCenters { get; } = new();

        public Room()
        {
            _floor = new Floor();
            _ceiling = new Ceiling();
            _wall = new Wall();
        }

        public MeshData CreateRoom(
            float width, float height, float depth,
            Color floorColor, Color ceilingColor, Color frontWallColor,
            Color backWallColor, Color leftWallColor, Color rightWallColor,
            WallOpening frontOpening, WallOpening backOpening,
            WallOpening leftOpening, WallOpening rightOpening,
            bool hasCeiling = true)
        {
            var vertices = new List<VertexPositionColor>();
            var indices = new List<ushort>();

            void Append(VertexPositionColor[] v, ushort[] i)
            {
                ushort offset = (ushort)vertices.Count; // Desde que valor debe de arrancar para crear los triangulos
                vertices.AddRange(v);
                // Se calcula el valor de los indices nuevo ya que para este punto Floor y Ceiling ya tienen sus indices en la lista
                for (int k = 0; k < i.Length; k++)
                    indices.Add((ushort)(i[k] + offset));
            }

            // Floor
            vertices.AddRange(_floor.CreateFloor(width, depth, floorColor));
            // Ceiling
            if (hasCeiling)
            {
                vertices.AddRange(_ceiling.CreateCeiling(width, height, depth, ceilingColor));
            }

            // Indices para Floor y Ceiling
            ushort[] quad = { 0, 1, 2, 0, 2, 3 };
            for (int face = 0; face < 2; face++)
            {
                ushort baseV = (ushort)(face * 4);
                indices.Add((ushort)(baseV + quad[0]));
                indices.Add((ushort)(baseV + quad[1]));
                indices.Add((ushort)(baseV + quad[2]));
                indices.Add((ushort)(baseV + quad[3]));
                indices.Add((ushort)(baseV + quad[4]));
                indices.Add((ushort)(baseV + quad[5]));
            }

            // FrontWall
            {
                var mesh = _wall.CreateFrontWall(width, height, depth, frontWallColor, frontOpening);
                Append(mesh.Vertices, mesh.Indices);
                OpeningCenters.AddRange(mesh.OpeningCenters);
            }

            // BackWall
            {
                var mesh = _wall.CreateBackWall(width, height, depth, backWallColor, backOpening);
                Append(mesh.Vertices, mesh.Indices);
                OpeningCenters.AddRange(mesh.OpeningCenters);
            }

            // LeftWall
            {
                var mesh = _wall.CreateLeftWall(width, height, depth, leftWallColor, leftOpening);
                Append(mesh.Vertices, mesh.Indices);
                OpeningCenters.AddRange(mesh.OpeningCenters);
            }

            // RightWall
            {
                var mesh = _wall.CreateRightWall(width, height, depth, rightWallColor, rightOpening);
                Append(mesh.Vertices, mesh.Indices);
                OpeningCenters.AddRange(mesh.OpeningCenters);
            }

            return new MeshData(vertices.ToArray(), indices.ToArray());
        }
    }
}