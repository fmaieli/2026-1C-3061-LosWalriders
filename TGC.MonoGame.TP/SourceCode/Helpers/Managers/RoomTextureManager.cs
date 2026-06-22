using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Factories;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Helpers
{
    public static class RoomTextureManager
    {
        public static Dictionary<RoomType, (Texture2D Wall, Texture2D Floor)> RoomTextures { get; } = new();

        public static void LoadTextures(ContentManager content)
        {
            RoomTextures.Clear();

            foreach (RoomType roomType in (RoomType[])Enum.GetValues(typeof(RoomType)))
            {
                IRoomAssets roomAsset = RoomFactory.Create(roomType);

                string wallPath = roomAsset?.WallTexturePath;
                string floorPath = roomAsset?.FloorTexturePath;

                if (string.IsNullOrEmpty(wallPath))
                {
                    wallPath = roomType switch
                    {
                        RoomType.Entrance => "Textures/rooms/wall/wall-living",
                        RoomType.Computer => "Textures/rooms/wall/wall-living",
                        RoomType.Kitchen => "Textures/rooms/wall/wall-kitchen",
                        RoomType.Prize => "Textures/rooms/wall/wall-bedroom",
                        RoomType.Bath => "Textures/rooms/wall/wall-kitchen",
                        RoomType.Hallway => "Textures/rooms/wall/wall-living", 
                        _ => "Textures/rooms/wall/wall-bedroom"                 // Default
                    };
                }

                if (string.IsNullOrEmpty(floorPath))
                {
                    floorPath = roomType switch
                    {
                        RoomType.Entrance => "Textures/rooms/floor/floor_living",
                        RoomType.Computer => "Textures/rooms/floor/floor_living",
                        RoomType.Kitchen => "Textures/rooms/floor/floor_kitchen",
                        RoomType.Prize => "Textures/rooms/floor/floor_bedroom",
                        RoomType.Bath => "Textures/rooms/floor/floor_kitchen",
                        RoomType.Hallway => "Textures/rooms/floor/floor_living",
                        _ => "Textures/rooms/floor/floor_bedroom"               // Default
                    };
                }

                try
                {
                    Texture2D wallTex = content.Load<Texture2D>(wallPath);
                    Texture2D floorTex = content.Load<Texture2D>(floorPath);
                    RoomTextures[roomType] = (wallTex, floorTex);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"No se encontro la imagen para {roomType}: {ex.Message}");
                }
            }
        }
    }
}