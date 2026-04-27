using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level
{
    public class Wall
    {
        public VertexPositionColor[] CreateWalls(float width, float height, float depth)
        {
            var wallsVertices = new VertexPositionColor[16];

            CreateRightWall(width, height, depth).CopyTo(wallsVertices, 0);
            CreateLeftWall(width, height, depth).CopyTo(wallsVertices, 4);
            CreateBackWall(width, height, depth).CopyTo(wallsVertices, 8);
            CreateFrontWall(width, height, depth).CopyTo(wallsVertices, 12);

            return wallsVertices;
        }

        private VertexPositionColor[] CreateFrontWall(float width, float height, float depth)
        {
            var frontWallVertices = new VertexPositionColor[4];

            frontWallVertices[0] = new VertexPositionColor(new Vector3(-width, 0, depth), Color.Blue);
            frontWallVertices[1] = new VertexPositionColor(new Vector3(width, 0, depth), Color.Blue);
            frontWallVertices[2] = new VertexPositionColor(new Vector3(width, height, depth), Color.Blue);
            frontWallVertices[3] = new VertexPositionColor(new Vector3(-width, height, depth), Color.Blue);

            return frontWallVertices;
        }

        private VertexPositionColor[] CreateBackWall(float width, float height, float depth)
        {
            var backWallVertices = new VertexPositionColor[4];

            backWallVertices[0] = new VertexPositionColor(new Vector3(-width, 0, -depth), Color.Blue);
            backWallVertices[1] = new VertexPositionColor(new Vector3(width, 0, -depth), Color.Blue);
            backWallVertices[2] = new VertexPositionColor(new Vector3(width, height, -depth), Color.Blue);
            backWallVertices[3] = new VertexPositionColor(new Vector3(-width, height, -depth), Color.Blue);

            return backWallVertices;
        }

        private VertexPositionColor[] CreateLeftWall(float width, float height, float depth)
        {
            var leftWallVertices = new VertexPositionColor[4];

            leftWallVertices[0] = new VertexPositionColor(new Vector3(-width, 0, -depth), Color.LightBlue);
            leftWallVertices[1] = new VertexPositionColor(new Vector3(-width, 0, depth), Color.LightBlue);
            leftWallVertices[2] = new VertexPositionColor(new Vector3(-width, height, depth), Color.LightBlue);
            leftWallVertices[3] = new VertexPositionColor(new Vector3(-width, height, -depth), Color.LightBlue);

            return leftWallVertices;
        }

        private VertexPositionColor[] CreateRightWall(float width, float height, float depth)
        {
            var rightWallVertices = new VertexPositionColor[4];

            rightWallVertices[0] = new VertexPositionColor(new Vector3(width, 0, -depth), Color.LightBlue);
            rightWallVertices[1] = new VertexPositionColor(new Vector3(width, 0, depth), Color.LightBlue);
            rightWallVertices[2] = new VertexPositionColor(new Vector3(width, height, depth), Color.LightBlue);
            rightWallVertices[3] = new VertexPositionColor(new Vector3(width, height, -depth), Color.LightBlue);

            return rightWallVertices;
        }
    }
}
