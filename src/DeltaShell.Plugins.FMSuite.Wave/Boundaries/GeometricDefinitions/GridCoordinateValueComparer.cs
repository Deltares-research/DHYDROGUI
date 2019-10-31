using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="GridCoordinateValueComparer"/> is used to compare grid coordinates
    /// on equality of their X and Y members.
    /// </summary>
    /// <seealso cref="EqualityComparer{GridCoordinate}" />
    public class GridCoordinateValueComparer : EqualityComparer<GridCoordinate>
    {
        public override bool Equals(GridCoordinate c1, GridCoordinate c2)
        {
            if (c1 == null && c2 == null)
            {
                return true;
            }

            if (c1 == null || c2 == null)
            {
                return false;
            }

            return c1.X == c2.X && c1.Y == c2.Y;
        }

        public override int GetHashCode(GridCoordinate coordinate)
        {
            if (coordinate == null)
            {
                return 0;
            }

            int hCode = coordinate.X ^ coordinate.Y;
            return hCode.GetHashCode();
        }
    }
}