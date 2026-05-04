using Microsoft.Xna.Framework;
using System;

namespace TGC.MonoGame.TP.SourceCode.Helpers
{
    public static class ModelTintHelper
    {
        public static Color GetTint(string modelPath)
        {
            var path = modelPath.Replace('\\', '/');

            if (path.StartsWith("Level/Bathroom", StringComparison.OrdinalIgnoreCase))
                return Color.SkyBlue;
            if (path.StartsWith("Level/Bedroom", StringComparison.OrdinalIgnoreCase))
                return Color.Yellow;
            if (path.StartsWith("Level/Computer", StringComparison.OrdinalIgnoreCase))
                return Color.Red;
            if (path.StartsWith("Level/Kitchen", StringComparison.OrdinalIgnoreCase))
                return Color.Orange;
            if (path.StartsWith("Level/Living", StringComparison.OrdinalIgnoreCase))
                return Color.Brown;
            if (path.StartsWith("Level/Outdoor", StringComparison.OrdinalIgnoreCase))
                return Color.Green;
            if (path.StartsWith("Miscellaneous", StringComparison.OrdinalIgnoreCase))
                return Color.Violet;
            if (path.StartsWith("Items", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("Player", StringComparison.OrdinalIgnoreCase))
                return Color.Pink;

            return Color.Black; 
        }
    }
}
