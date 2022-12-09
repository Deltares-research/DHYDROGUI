using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.TestUtils.EqualityComparers
{
    /// <summary>
    /// An equality comparer for <see cref="IDelftIniCategory"/>.
    /// </summary>
    public class DelftIniCategoryEqualityComparer : IEqualityComparer<IDelftIniCategory>
    {
        private static readonly DelftIniPropertyEqualityComparer propertyComparer = new DelftIniPropertyEqualityComparer();

        /// <summary>
        /// Determines whether the specified Delft INI categories are equal.
        /// </summary>
        /// <param name="x"> The first Delft INI category to compare. </param>
        /// <param name="y"> The second Delft INI category to compare. </param>
        /// <returns>
        /// <see langword="true"/> if the specified categories are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(IDelftIniCategory x, IDelftIniCategory y)
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

            if (x.Properties.Count != y.Properties.Count)
            {
                return false;
            }

            IEnumerable<bool> propertyEqualities = x.Properties.Zip(y.Properties, (px, py) => propertyComparer.Equals(px, py));
            if (propertyEqualities.Any(equal => equal == false))
            {
                return false;
            }

            return x.Name == y.Name && x.LineNumber == y.LineNumber;
        }

        /// <inheritdoc/>
        public int GetHashCode(IDelftIniCategory obj)
        {
            unchecked
            {
                int hashCode = obj.Name != null ? obj.Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (obj.Properties != null ? obj.Properties.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.LineNumber;
                return hashCode;
            }
        }
    }
}