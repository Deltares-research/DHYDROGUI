using System.Collections.Generic;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.TestUtils.EqualityComparers
{
    /// <summary>
    /// An equality comparer for <see cref="IniProperty"/>.
    /// </summary>
    public class DelftIniPropertyEqualityComparer : IEqualityComparer<IniProperty>
    {
        /// <summary>
        /// Determines whether the specified Delft INI properties are equal.
        /// </summary>
        /// <param name="x"> The first Delft INI property to compare. </param>
        /// <param name="y"> The second Delft INI property to compare. </param>
        /// <returns>
        /// <see langword="true"/> if the specified properties are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(IniProperty x, IniProperty y)
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

            return x.Key == y.Key && x.Value == y.Value && x.Comment == y.Comment && x.LineNumber == y.LineNumber;
        }

        /// <inheritdoc/>
        public int GetHashCode(IniProperty obj)
        {
            unchecked
            {
                int hashCode = obj.Key != null ? obj.Key.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (obj.Value != null ? obj.Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Comment != null ? obj.Comment.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.LineNumber;
                return hashCode;
            }
        }
    }
}