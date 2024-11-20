using System;

namespace DelftTools.Hydro
{
    public static class ExtensionMethods
    {
        public static bool IsEqualTo(this double first, double second, double tolerance = 1e-6)
        {
            return Math.Abs(first - second) < tolerance;
        }
    }
}
