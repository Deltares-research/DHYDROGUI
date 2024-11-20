using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests
{
    /// <summary>
    /// Provides an equality comparer for shape geometry.
    /// </summary>
    public sealed class ShapeGeometryComparer : IEqualityComparer<ShapeBase>
    {
        public bool Equals(ShapeBase x, ShapeBase y)
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

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.X.Equals(y.X) &&
                   x.Y.Equals(y.Y) &&
                   x.Width.Equals(y.Width) &&
                   x.Height.Equals(y.Height);
        }

        public int GetHashCode(ShapeBase obj)
        {
            unchecked
            {
                int hashCode = obj.X.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Y.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Width.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Height.GetHashCode();
                return hashCode;
            }
        }
    }
}