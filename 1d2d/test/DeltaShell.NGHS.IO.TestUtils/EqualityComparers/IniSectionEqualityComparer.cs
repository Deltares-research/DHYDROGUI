using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.IO.Ini;

namespace DeltaShell.NGHS.IO.TestUtils.EqualityComparers
{
    /// <summary>
    /// An equality comparer for <see cref="IniSection"/>.
    /// </summary>
    public class IniSectionEqualityComparer : IEqualityComparer<IniSection>
    {
        private static readonly IniPropertyEqualityComparer propertyComparer = new IniPropertyEqualityComparer();

        /// <summary>
        /// Determines whether the specified Delft INI sections are equal.
        /// </summary>
        /// <param name="x"> The first Delft INI section to compare. </param>
        /// <param name="y"> The second Delft INI section to compare. </param>
        /// <returns>
        /// <see langword="true"/> if the specified INI sections are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(IniSection x, IniSection y)
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

            if (x.Properties.Count() != y.Properties.Count())
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
        public int GetHashCode(IniSection obj)
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