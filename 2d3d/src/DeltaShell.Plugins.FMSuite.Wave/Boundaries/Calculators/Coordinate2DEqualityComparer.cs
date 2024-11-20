using System.Collections.Generic;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators
{
    /// <summary>
    /// <see cref="Coordinate2DEqualityComparer"/> provides an <see cref="IEqualityComparer{T}"/>.
    /// It leverages the internal <see cref="Coordinate.Equals2D(Coordinate)"/> to obtain equality.
    /// </summary>
    /// <seealso cref="IEqualityComparer{Coordinate}"/>
    public class Coordinate2DEqualityComparer : IEqualityComparer<Coordinate>
    {
        public bool Equals(Coordinate x, Coordinate y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Equals2D(y);
        }

        public int GetHashCode(Coordinate obj)
        {
            int hCode = obj.X.GetHashCode() ^ obj.Y.GetHashCode();
            return hCode.GetHashCode();
        }
    }
}