using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.TestUtils.EqualityComparers
{
    /// <summary>
    /// An equality comparer for <see cref="DelftBcCategory"/>.
    /// </summary>
    public class DelftBcCategoryEqualityComparer : IEqualityComparer<DelftBcCategory>
    {
        private static readonly DelftIniCategoryEqualityComparer iniSectionComparer = new DelftIniCategoryEqualityComparer();
        private static readonly DelftBcQuantityDataEqualityComparer quantityDataEqualityComparer = new DelftBcQuantityDataEqualityComparer();

        /// <summary>
        /// Determines whether the specified Delft BC categories are equal.
        /// </summary>
        /// <param name="x"> The first Delft BC category to compare. </param>
        /// <param name="y"> The second Delft BC category to compare. </param>
        /// <returns>
        /// <see langword="true"/> if the specified categories are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(DelftBcCategory x, DelftBcCategory y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (!iniSectionComparer.Equals(x.Section, y.Section))
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
        public int GetHashCode(DelftBcCategory obj)
        {
            int hashCode = obj.Section.Name != null ? obj.Section.Name.GetHashCode() : 0;
            hashCode = (hashCode * 397) ^ (obj.Section.Properties != null ? obj.Section.Properties.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ obj.Section.LineNumber;
            hashCode = (hashCode * 397) ^ (obj.Table != null ? obj.Table.GetHashCode() : 0);
            return hashCode;
        }
    }
}