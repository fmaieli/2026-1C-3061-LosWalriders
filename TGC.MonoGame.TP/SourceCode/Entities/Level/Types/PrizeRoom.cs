using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Helpers;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Types
{
    public class PrizeRoom : IRoomAssets
    {
        public RoomType Type => RoomType.Prize;

        // Placeholder - uso texturas de living como default
        public string WallTexturePath => "Textures/rooms/wall/wall-living";
        public string FloorTexturePath => "Textures/rooms/floor/floor_living";

        public List<string> Assets { get; } = new List<string>
        {
            "Items/PSX_Item_Shotgun"
        };

        public List<(string ModelPath, Vector3 Position, float RotationY)> SpawnedModels { get; private set; } = new();

        public void Generate(float width, float depth, float cellSize, int seed)
        {
            SpawnedModels = ModelPlacementOnRoomHelper.GeneratePlacements(this, width, depth, cellSize, seed);
        }
    }
}