using Microsoft.Xna.Framework.Graphics;

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

        public VertexPositionColor[] CreateRoom(float width, float height, float depth)
        {
            var roomVertices = new VertexPositionColor[24];

            _floor.CreateFloor(width, depth).CopyTo(roomVertices, 0);
            _ceiling.CreateCeiling(width, height, depth).CopyTo(roomVertices, 4);
            _wall.CreateWalls(width, height, depth).CopyTo(roomVertices, 8);

            return roomVertices;
        }
    }
}
