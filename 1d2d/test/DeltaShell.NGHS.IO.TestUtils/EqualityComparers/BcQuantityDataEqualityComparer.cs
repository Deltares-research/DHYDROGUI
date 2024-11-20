using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.TestUtils.EqualityComparers
{
    /// <summary>
    /// An equality comparer for <see cref="IBcQuantityData"/>.
    /// </summary>
    public class BcQuantityDataEqualityComparer : IEqualityComparer<IBcQuantityData>
    {
        /// <summary>
        /// Determines whether the specified Delft BC quantity data objects are equal.
        /// </summary>
        /// <param name="x"> The first Delft BC quantity data object to compare. </param>
        /// <param name="y"> The second Delft BC quantity data object to compare. </param>
        /// <returns>
        /// <see langword="true"/> if the specified Delft BC quantity data objects are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(IBcQuantityData x, IBcQuantityData y)
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

            if (!Equals(x.Quantity, y.Quantity))
            {
                return false;
            }

            if (!Equals(x.Unit, y.Unit))
            {
                return false;
            }

            if (x.Values.Count != y.Values.Count)
            {
                return false;
            }

            IEnumerable<bool> valueEqualities = x.Values.Zip(y.Values, (vx, vy) => vx == vy);
            if (valueEqualities.Any(equal => equal == false))
            {
                return false;
            }

            return x.LineNumber == y.LineNumber;
        }

        /// <inheritdoc/>
        public int GetHashCode(IBcQuantityData obj)
        {
            unchecked
            {
                int hashCode = obj.Quantity != null ? obj.Quantity.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (obj.Unit != null ? obj.Unit.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.LineNumber;
                hashCode = (hashCode * 397) ^ (obj.Values != null ? obj.Values.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}