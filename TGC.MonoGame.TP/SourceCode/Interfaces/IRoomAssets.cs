using System.Collections.Generic;
using TGC.MonoGame.TP.SourceCode.Enums;

namespace TGC.MonoGame.TP.SourceCode.Interfaces
{
    public interface IRoomAssets
    {
        RoomType Type { get; }
        List<string> Assets { get; }
    }
}
