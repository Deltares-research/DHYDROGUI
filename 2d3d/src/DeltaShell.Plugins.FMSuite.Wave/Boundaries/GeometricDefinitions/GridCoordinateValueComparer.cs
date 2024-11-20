using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="GridCoordinateValueComparer"/> is used to compare grid coordinates
    /// on equality of their X and Y members.
    /// </summary>
    /// <seealso cref="EqualityComparer{T}"/>
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

        public override int GetHashCode(GridCoordinate obj)
        {
            if (obj == null)
            {
                return 0;
            }

            int hCode = (ShiftAndWrap(obj.X.GetHashCode(), 2) ^
                         obj.Y.GetHashCode()) + 1;
            return hCode.GetHashCode();
        }

        private static int ShiftAndWrap(int value, int positions)
        {
            positions &= 0x1F;

            var number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
            uint wrapped = number >> (32 - positions);
            uint bitShiftedValue = (number << positions) | wrapped;

            return BitConverter.ToInt32(BitConverter.GetBytes(bitShiftedValue), 0);
        }
    }
}