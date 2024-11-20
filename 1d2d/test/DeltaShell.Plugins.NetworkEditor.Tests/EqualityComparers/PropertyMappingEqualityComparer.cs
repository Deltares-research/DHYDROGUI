using System.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Import;

namespace DeltaShell.Plugins.NetworkEditor.Tests.EqualityComparers
{
    /// <summary>
    /// An equality comparer for <see cref="PropertyMapping"/>.
    /// </summary>
    public class PropertyMappingEqualityComparer : IEqualityComparer<PropertyMapping>
    {
        /// <summary>
        /// Determines whether the specified <see cref="PropertyMapping"/> are equal.
        /// </summary>
        /// <param name="x"> The first property mapping to compare. </param>
        /// <param name="y"> The second property mapping to compare. </param>
        /// <returns>
        /// <see langword="true"/> if the specified properties are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(PropertyMapping x, PropertyMapping y)
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

            return x.PropertyName == y.PropertyName
                   && x.PropertyUnit == y.PropertyUnit
                   && x.IsUnique == y.IsUnique
                   && x.IsRequired == y.IsRequired;
        }

        public int GetHashCode(PropertyMapping obj)
        {
            unchecked
            {
                int hashCode = obj.PropertyName != null ? obj.PropertyName.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (obj.PropertyUnit != null ? obj.PropertyUnit.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.IsUnique.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.IsRequired.GetHashCode();
                hashCode = (hashCode * 397) ^ (obj.MappingColumn != null ? obj.MappingColumn.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}