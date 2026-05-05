using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Helpers;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Types
{
    internal class OutdoorRoom : IRoomAssets
    {
        public RoomType Type { get; } = RoomType.Outdoor;

        public List<string> Assets { get; } = new()
        {
            "Level/Outdoor/Grass",
            "Level/Outdoor/LowPoly_Grass",
            "Level/Outdoor/LowPoly_Tree",
            "Level/Outdoor/PSX_Bush",
            "Level/Outdoor/PSX_Bush2",
            "Level/Outdoor/PSX_Bush3",
            "Level/Outdoor/PSX_Fence_White_Gate_Poles",
            "Level/Outdoor/PSX_Fence_White_Gate",
            "Level/Outdoor/PSX_Fence_White_Left_Closed",
            "Level/Outdoor/PSX_Fence_White_Left_Open",
            "Level/Outdoor/PSX_Fence_White_Right_Closed",
            "Level/Outdoor/PSX_Fence_White_Right_Open",
            "Level/Outdoor/PSX_Fence_White_Right_Pole"
        };

        public List<(string ModelPath, Vector3 Position)> SpawnedModels { get; private set; } = new();

        public void Generate(float width, float depth, float cellSize, int seed)
        {
            SpawnedModels = ModelPlacementOnRoomHelper.GeneratePlacements(this, width, depth, cellSize, seed);
        }
    }
}