using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection.Metadata.Ecma335;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Primitives
{  
    public class Room
    {
        public readonly Floor _floor;
        public Ceiling _ceiling;
        public Wall _wall;

        public Room()
        {
            _floor = new Floor();
            _ceiling = new Ceiling();
            _wall = new Wall();
        }

        public MeshData CreateRoom(float width, float height, float depth, 
            Color floorColor, Color ceilingColor, Color frontWallColor, Color backWallColor, Color leftWallColor, Color rightWallColor)
        {
            var roomVertices = new VertexPositionColor[24];

            _floor.CreateFloor(width, depth, floorColor).CopyTo(roomVertices, 0);
            _ceiling.CreateCeiling(width, height, depth, ceilingColor).CopyTo(roomVertices, 4);
            _wall.CreateWalls(width, height, depth, 
                              frontWallColor, backWallColor, leftWallColor, rightWallColor).CopyTo(roomVertices, 8);

            var roomIndices = new ushort[36];
            for (int face = 0; face < 6; face++)
            {
                int v = face * 4;
                int i = face * 6;
                roomIndices[i + 0] = (ushort)(v + 0);
                roomIndices[i + 1] = (ushort)(v + 1);
                roomIndices[i + 2] = (ushort)(v + 2);
                roomIndices[i + 3] = (ushort)(v + 0);
                roomIndices[i + 4] = (ushort)(v + 2);
                roomIndices[i + 5] = (ushort)(v + 3);
            }

            return new MeshData(roomVertices, roomIndices);
        }
    }
}
