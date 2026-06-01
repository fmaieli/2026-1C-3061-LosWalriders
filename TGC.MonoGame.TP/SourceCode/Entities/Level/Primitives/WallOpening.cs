using TGC.MonoGame.TP.SourceCode.Enums;

public class WallOpening
{
    public WallType Type;
    public float Width;
    public float Height;
    public float Offset;    // Para poder desplazar las puertas y ventanas,
                            // de esta forma poder juntarlos con los que los pasillos crean

    public static WallOpening Solid() => new WallOpening { Type = WallType.Solid };
    
    public static WallOpening Empty() => new WallOpening { Type = WallType.Empty };

    public static WallOpening Door(float width, float height, float offset = 0f) => new WallOpening
    {
        Type = WallType.Door,
        Width = width,
        Height = height,
        Offset = offset
    };

    public static WallOpening Window(float width, float height, float offset = 0f) => new WallOpening
    {
        Type = WallType.Window,
        Width = width,
        Height = height,
        Offset = offset
    };
}