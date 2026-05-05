using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Helpers;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Types
{
    internal class KitchenRoom : IRoomAssets
    {
        public RoomType Type { get; } = RoomType.Kitchen;

        public List<string> Assets { get; } = new()
        {
            "Level/Kitchen/PSX_Empty_Cup",
            "Level/Kitchen/PSX_Microwave",
            "Level/Kitchen/PSX_Plate",
            "Level/Kitchen/PSX_Plate1",
            "Level/Kitchen/PSX_Stockpot",
            "Level/Kitchen/PSX_Wooden_Table1"
        };

        public List<(string ModelPath, Vector3 Position)> SpawnedModels { get; private set; } = new();

        public void Generate(float width, float depth, float cellSize, int seed)
        {
            SpawnedModels = ModelPlacementOnRoomHelper.GeneratePlacements(this, width, depth, cellSize, seed);
        }
    }
}