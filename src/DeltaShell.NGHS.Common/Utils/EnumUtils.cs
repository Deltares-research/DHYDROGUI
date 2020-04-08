using System;
using System.Linq;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Reflection;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// Provides helper methods for enums.
    /// </summary>
    public static class EnumUtils
    {
        /// <summary>
        /// Gets the enum value with the specified <paramref name="description"/>.
        /// </summary>
        /// <typeparam name="T"> The enum type. </typeparam>
        /// <param name="description"> The description. </param>
        /// <returns>
        /// If found, the enum value with the specified <paramref name="description"/>;
        /// otherwise, the <c> default </c> of <typeparam name="T"/>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="description"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// The comparison of the description is case-insensitive.
        /// </remarks>
        public static T GetEnumValueByDescription<T>(string description)
        {
            Ensure.NotNull(description, nameof(description));

            return Enum.GetValues(typeof(T)).OfType<T>()
                       .FirstOrDefault(v => description.Equals((v as Enum).GetDescription(),
                                                               StringComparison.OrdinalIgnoreCase));
        }
    }
}