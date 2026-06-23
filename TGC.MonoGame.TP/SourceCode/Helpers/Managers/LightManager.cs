using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.SourceCode.Helpers.Managers
{
    public static class LightManager
    {
        // Propiedades de iluminacion en la escena
        public static bool IsLightActive { get; set; }
        public static Vector3 LightPosition { get; set; }
        public static Vector3 LightDirection { get; set; }
        public static Vector3 LightColor { get; set; }
        public static float LightIntensity { get; set; }
        public static float LightRadius { get; set; }       // Hasta donde llega la luz

        // Spotlight (linterna) - PointLight (fosforo)
        public static bool IsSpotLight { get; set; }
        public static float SpotAngle { get; set; }         // Angulo del haz de luz de la linterna

        public static void ApplyLightingToShader(Effect effect)
        {
            // 1f = true - 0f false
            effect.Parameters["IsLightActive"]?.SetValue(IsLightActive ? 1f : 0f);

            if (!IsLightActive) return;

            effect.Parameters["LightPosition"]?.SetValue(LightPosition);
            effect.Parameters["LightDirection"]?.SetValue(LightDirection);
            effect.Parameters["LightColor"]?.SetValue(LightColor);
            effect.Parameters["LightIntensity"]?.SetValue(LightIntensity);
            effect.Parameters["LightRadius"]?.SetValue(LightRadius);

            // 1f = true - 0f false
            effect.Parameters["IsSpotLight"]?.SetValue(IsSpotLight ? 1f : 0f);
            effect.Parameters["SpotAngle"]?.SetValue(SpotAngle);
        }
    }
}