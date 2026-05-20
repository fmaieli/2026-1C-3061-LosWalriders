using TGC.MonoGame.TP.SourceCode.Enums;

public class WallOpening
{
    public WallType Type;
    public float Width;
    public float Height;

    public static WallOpening Solid() => new WallOpening { Type = WallType.Solid };

    public static WallOpening Door(float width, float height) => new WallOpening
    {
        Type = WallType.Door,
        Width = width,
        Height = height
    };

    public static WallOpening Window(float width, float height) => new WallOpening
    {
        Type = WallType.Window,
        Width = width,
        Height = height
    };

    public static WallOpening Empty() => new WallOpening { Type = WallType.Empty };
}