using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Helpers;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Types
{
    internal class BedRoom : IRoomAssets
    {
        public RoomType Type { get; } = RoomType.Bed;

        public List<string> Assets { get; } = new()
        {
            "Level/Bedroom/PSX_Bed",
            "Level/Bedroom/PSX_Lamp",
            "Level/Bedroom/PSX_Wooden_Closet",
            "Level/Bedroom/PSX_Wooden_Drawers"
        };

        public List<(string ModelPath, Vector3 Position, float RotationY)> SpawnedModels { get; private set; } = new();

        public void Generate(float width, float depth, float cellSize, int seed)
        {
            SpawnedModels = ModelPlacementOnRoomHelper.GeneratePlacements(this, width, depth, cellSize, seed);
        }
    }
}