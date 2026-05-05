using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Helpers;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Types
{
    internal class LivingRoom : IRoomAssets
    {
        public RoomType Type { get; } = RoomType.Living;

        public List<string> Assets { get; } = new()
        {
            "Level/Living/PSX_Armchair",
            "Level/Living/PSX_Old_TV",
            "Level/Living/PSX_PlayStation1",
            "Level/Living/PSX_TV_Stand",
            "Level/Living/PSX_Wooden_Chair",
            "Level/Living/PSX_Wooden_Chair1",
            "Level/Living/PSX_Wooden_Chair2",
            "Level/Living/PSX_Wooden_Table"
        };

        public List<(string ModelPath, Vector3 Position)> SpawnedModels { get; private set; } = new();

        public void Generate(float width, float depth, float cellSize, int seed)
        {
            SpawnedModels = ModelPlacementOnRoomHelper.GeneratePlacements(this, width, depth, cellSize, seed);
        }
    }
}