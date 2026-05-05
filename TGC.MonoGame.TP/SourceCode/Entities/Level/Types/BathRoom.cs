using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Helpers;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Types
{
    internal class BathRoom : IRoomAssets
    {
        public RoomType Type { get; } = RoomType.Bath;

        public List<string> Assets { get; } = new()
        {
            "Level/Bathroom/PSX_Toilet_Paper",
            "Level/Bathroom/PSX_Toilet"
        };

        public List<(string ModelPath, Vector3 Position)> SpawnedModels { get; private set; } = new();

        public void Generate(float width, float depth, float cellSize, int seed)
        {
            SpawnedModels = ModelPlacementOnRoomHelper.GeneratePlacements(this, width, depth, cellSize, seed);
        }
    }
}
