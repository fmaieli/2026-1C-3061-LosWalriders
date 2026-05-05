using TGC.MonoGame.TP.SourceCode.Entities.Level.Types;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Factories
{
    public static class RoomFactory
    {
        public static IRoomAssets Create(RoomType type) => type switch
        {
            RoomType.Bath => new BathRoom(),
            RoomType.Bed => new BedRoom(),
            RoomType.Computer => new ComputerRoom(),
            RoomType.Kitchen => new KitchenRoom(),
            RoomType.Living => new LivingRoom(),
            RoomType.Outdoor => new OutdoorRoom(),
            RoomType.Hallway => new HallwayRoom(),
            _ => null
        };
    }
}
