using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Enums;

namespace TGC.MonoGame.TP.SourceCode.Interfaces
{
    public interface IRoomAssets
    {
        RoomType Type { get; }        
        List<string> Assets { get; }
        string WallTexturePath { get; }
        string FloorTexturePath { get; }
        List<(string ModelPath, Vector3 Position, float RotationY)> SpawnedModels { get; }
        void Generate(float width, float depth, float cellSize, int seed);
    }
}
