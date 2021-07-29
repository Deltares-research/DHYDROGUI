using System;
using DelftTools.Hydro.SewerFeatures;

namespace DelftTools.Hydro.Structures
{
    public static class SewerConnectionExtensions
    {
        public static double Slope(this ISewerConnection sewerConnection)
        {
            if (sewerConnection == null) return double.NaN;
            var length = sewerConnection.Length;
            var dy = sewerConnection.LevelTarget - sewerConnection.LevelSource;

            var angle = Math.Asin(dy / length);

            return RadToDeg(angle);
        }

        private static double RadToDeg(double rad)
        {
            return 180.0 / Math.PI * rad;
        }
    }
}