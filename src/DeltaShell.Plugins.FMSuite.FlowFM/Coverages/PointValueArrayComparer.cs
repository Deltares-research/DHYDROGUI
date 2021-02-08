using System.Collections.Generic;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Coverages
{
    /// <summary>
    /// This <see cref="PointValueArrayComparer"/> compares <see cref="IPointValue"/> arrays.
    /// </summary>
    public class PointValueArrayComparer : IEqualityComparer<IPointValue[]>
    {
        public bool Equals(IPointValue[] x, IPointValue[] y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.Length != y.Length)
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                IPointValue pointValueX = x[i];
                IPointValue pointValueY = y[i];

                if (!pointValueX.Value.Equals(pointValueY.Value))
                {
                    return false;
                }

                if (!pointValueX.X.Equals(pointValueY.X))
                {
                    return false;
                }

                if (!pointValueX.Y.Equals(pointValueY.Y))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(IPointValue[] obj)
        {
            unchecked
            {
                {
                    int hashCode = obj.Length.GetHashCode();

                    foreach (IPointValue pointValue in obj)
                    {
                        hashCode = (hashCode * 397) ^ GetHashCode(pointValue);
                    }

                    return hashCode;
                }
            }
        }

        private static int GetHashCode(IPointValue pointValue)
        {
            int hashCode = pointValue.X.GetHashCode();
            hashCode = (hashCode * 397) ^ pointValue.Y.GetHashCode();
            hashCode = (hashCode * 397) ^ pointValue.Value.GetHashCode();
            return hashCode;
        }
    }
}