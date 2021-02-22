using System.Collections.Generic;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Coverages
{
    /// <summary>
    /// This <see cref="PointValueEqualityComparer"/> compares <see cref="IPointValue"/> on equality.
    /// </summary>
    public class PointValueEqualityComparer : IEqualityComparer<IPointValue>
    {
        public bool Equals(IPointValue x, IPointValue y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            if (!x.Value.Equals(y.Value))
            {
                return false;
            }

            if (!x.X.Equals(y.X))
            {
                return false;
            }

            if (!x.Y.Equals(y.Y))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(IPointValue obj)
        {
            unchecked
            {
                int hashCode = obj.X.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Y.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Value.GetHashCode();
                return hashCode;
            }
        }
    }
}