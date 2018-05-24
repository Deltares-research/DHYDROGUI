using System;

namespace DelftTools.Hydro.Structures
{
    public static class PipeExtensions
    {
        public static double Slope(this IPipe pipe)
        {
            if (pipe == null) return double.NaN;
            var length = pipe.Length;
            var dy = pipe.LevelTarget - pipe.LevelSource;

            var angle = Math.Asin(dy / length);

            return RadToDeg(angle);
        }

        private static double RadToDeg(double rad)
        {
            return 180.0 / Math.PI * rad;
        }
    }
}