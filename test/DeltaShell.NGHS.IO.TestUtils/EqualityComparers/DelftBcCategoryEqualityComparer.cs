using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.TestUtils.EqualityComparers
{
    /// <summary>
    /// An equality comparer for <see cref="IDelftBcCategory"/>.
    /// </summary>
    public class DelftBcCategoryEqualityComparer : IEqualityComparer<IDelftBcCategory>
    {
        private static readonly DelftIniCategoryEqualityComparer categoryComparer = new DelftIniCategoryEqualityComparer();
        private static readonly DelftBcQuantityDataEqualityComparer quantityDataEqualityComparer = new DelftBcQuantityDataEqualityComparer();

        /// <summary>
        /// Determines whether the specified Delft BC categories are equal.
        /// </summary>
        /// <param name="x"> The first Delft BC category to compare. </param>
        /// <param name="y"> The second Delft BC category to compare. </param>
        /// <returns>
        /// <see langword="true"/> if the specified categories are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(IDelftBcCategory x, IDelftBcCategory y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (!categoryComparer.Equals(x, y))
            {
                return false;
            }

            if (x.Table.Count != y.Table.Count)
            {
                return false;
            }

            IEnumerable<bool> tableEqualities = x.Table.Zip(y.Table, (tx, ty) => quantityDataEqualityComparer.Equals(tx, ty));
            return tableEqualities.All(equal => equal);
        }

        /// <inheritdoc/>
        public int GetHashCode(IDelftBcCategory obj)
        {
            int hashCode = obj.Name != null ? obj.Name.GetHashCode() : 0;
            hashCode = (hashCode * 397) ^ (obj.Properties != null ? obj.Properties.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ obj.LineNumber;
            hashCode = (hashCode * 397) ^ (obj.Table != null ? obj.Table.GetHashCode() : 0);
            return hashCode;
        }
    }
}