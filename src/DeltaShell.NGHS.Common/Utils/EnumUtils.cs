using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Utils.Extensions;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// Provides utility methods for <see cref="Enum"/>.
    /// </summary>
    public static class EnumUtils
    {
        /// <summary>
        /// Gets the enum value with the specified <paramref name="displayName"/>.
        /// </summary>
        /// <typeparam name="T"> The enum type. </typeparam>
        /// <param name="displayName"> The description. </param>
        /// <param name="enumType"></param>
        /// <returns>
        /// If found, the enum value with the specified <paramref name="displayName"/>;
        /// otherwise the default <paramref name="enumType"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="enumType"/> is not an <see cref="Enum"/> type.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="displayName"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// The comparison of the display name is case-insensitive.
        /// </remarks>
        public static object GetEnumValueFromDisplayName(string displayName, Type enumType)
        {
            Ensure.NotNull(displayName, nameof(displayName));

            if (!enumType.IsEnum)
            {
                throw new ArgumentException($"Type {enumType} is not an Enum.");
            }

            IEnumerable<Enum> enumValues = Enum.GetValues(enumType).Cast<Enum>();
            return enumValues.FirstOrDefault(v => HasDisplayName(displayName, v));
        }

        private static bool HasDisplayName(string displayName, Enum v)
        {
            return v.GetDisplayName().EqualsCaseInsensitive(displayName);
        }
    }
}