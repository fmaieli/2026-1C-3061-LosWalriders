using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Primitives
{
    public class Wall
    {
        public VertexPositionColor[] CreateWalls(float width, float height, float depth, Color frontColor, Color backColor, Color leftColor, Color rightColor)
        {
            var wallsVertices = new VertexPositionColor[16];

            CreateRightWall(width, height, depth, rightColor).CopyTo(wallsVertices, 0);
            CreateLeftWall(width, height, depth, leftColor).CopyTo(wallsVertices, 4);
            CreateBackWall(width, height, depth, backColor).CopyTo(wallsVertices, 8);
            CreateFrontWall(width, height, depth, frontColor).CopyTo(wallsVertices, 12);

            return wallsVertices;
        }

        private VertexPositionColor[] CreateFrontWall(float width, float height, float depth, Color color)
        {
            var frontWallVertices = new VertexPositionColor[4];

            frontWallVertices[0] = new VertexPositionColor(new Vector3(-width, 0, depth), color);
            frontWallVertices[1] = new VertexPositionColor(new Vector3(width, 0, depth), color);
            frontWallVertices[2] = new VertexPositionColor(new Vector3(width, height, depth), color);
            frontWallVertices[3] = new VertexPositionColor(new Vector3(-width, height, depth), color);

            return frontWallVertices;
        }

        private VertexPositionColor[] CreateBackWall(float width, float height, float depth, Color color)
        {
            var backWallVertices = new VertexPositionColor[4];

            backWallVertices[0] = new VertexPositionColor(new Vector3(-width, 0, -depth), color);
            backWallVertices[1] = new VertexPositionColor(new Vector3(width, 0, -depth), color);
            backWallVertices[2] = new VertexPositionColor(new Vector3(width, height, -depth), color);
            backWallVertices[3] = new VertexPositionColor(new Vector3(-width, height, -depth), color);

            return backWallVertices;
        }

        private VertexPositionColor[] CreateLeftWall(float width, float height, float depth, Color color)
        {
            var leftWallVertices = new VertexPositionColor[4];

            leftWallVertices[0] = new VertexPositionColor(new Vector3(-width, 0, -depth), color);
            leftWallVertices[1] = new VertexPositionColor(new Vector3(-width, 0, depth), color);
            leftWallVertices[2] = new VertexPositionColor(new Vector3(-width, height, depth), color);
            leftWallVertices[3] = new VertexPositionColor(new Vector3(-width, height, -depth), color);

            return leftWallVertices;
        }

        private VertexPositionColor[] CreateRightWall(float width, float height, float depth, Color color)
        {
            var rightWallVertices = new VertexPositionColor[4];

            rightWallVertices[0] = new VertexPositionColor(new Vector3(width, 0, -depth), color);
            rightWallVertices[1] = new VertexPositionColor(new Vector3(width, 0, depth), color);
            rightWallVertices[2] = new VertexPositionColor(new Vector3(width, height, depth), color);
            rightWallVertices[3] = new VertexPositionColor(new Vector3(width, height, -depth), color);

            return rightWallVertices;
        }
    }
}
