using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Enums;
using TGC.MonoGame.TP.SourceCode.Interfaces;

namespace TGC.MonoGame.TP.SourceCode.Entities.Level.Types
{
    public class PrizeRoom : IRoomAssets
    {
        public RoomType Type => RoomType.Prize;

        public List<string> Assets { get; } = new List<string>
        {
            "Items/PSX_Item_Shotgun"
        };
    }
}