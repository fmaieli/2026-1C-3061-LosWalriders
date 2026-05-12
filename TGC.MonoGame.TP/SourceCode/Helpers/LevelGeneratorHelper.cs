using Microsoft.Xna.Framework;
using TGC.MonoGame.TP.SourceCode.Enums;

namespace TGC.MonoGame.TP.SourceCode.Helpers
{
    public static class LevelGeneratorHelper
    {
        public static WallOpening DetermineOpening(char currentCell, char neighborCell)
        {
            // TODO: REVISAR LOGICA PARA DETERMINAR SI ES UN PASILLO PARA NO DIBUJAR LAS PAREDES,
            // NO ESTA FUNCIONANDO CORRECTAMENTE
            bool currentIsHallway = (currentCell == 'H' || currentCell == 'V');
            bool neighborIsHallway = (neighborCell == 'H' || neighborCell == 'V');

            if (currentIsHallway && neighborIsHallway)
                return WallOpening.Empty();

            if (currentCell == 'E' && (neighborCell == ' ' || neighborCell == '\0'))
                return WallOpening.Door(40f, 80f);

            if (neighborCell == ' ' || neighborCell == '\0')
                return WallOpening.Solid();

            return WallOpening.Door(40f, 80f);
        }

        // Se pintan las paredes de las habitacion con Front-Back y Left-Right
        public static (RoomType Type, Color FrontBack, Color LeftRight)? GetRoomData(char cell)
        {
            return cell switch
            {
                'E' => (RoomType.Entrance, Color.SkyBlue, Color.LightBlue),          // Entrance
                'Z' => (RoomType.Outdoor, Color.Red, Color.DarkRed),                 // Exit (ahora es Z porque se creo la referencia de Entrance)
                'H' => (RoomType.Hallway, Color.Yellow, Color.LightGoldenrodYellow),
                'V' => (RoomType.Hallway, Color.DarkGoldenrod, Color.Goldenrod),     // Vents
                'C' => (RoomType.Computer, Color.Gray, Color.DarkGray),
                'O' => (RoomType.Outdoor, Color.DarkGreen, Color.ForestGreen),
                'B' => (RoomType.Bed, Color.DarkOrange, Color.Orange),
                'L' => (RoomType.Living, Color.SaddleBrown, Color.Peru),
                'K' => (RoomType.Kitchen, Color.LightSalmon, Color.PeachPuff),
                'A' => (RoomType.Bath, Color.MediumBlue, Color.CornflowerBlue),      // Baños (se usa A porque la B la usa la habitacion Bed)
                _ => null
            };
        }
    }
}