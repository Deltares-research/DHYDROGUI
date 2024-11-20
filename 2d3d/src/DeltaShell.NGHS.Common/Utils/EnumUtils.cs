using System;
using System.Linq;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Guards;

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
        /// If found, the enum value with the specified <paramref name="description"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="description"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when there is no enum value for the given <paramref name="description"/>.
        /// </exception>
        /// <remarks>
        /// The comparison of the description is case-insensitive.
        /// </remarks>
        public static T GetEnumValueByDescription<T>(string description)
        {
            Ensure.NotNull(description, nameof(description));

            T[] enumsFromDescription = Enum.GetValues(typeof(T)).OfType<T>().Where(v => description.Equals((v as Enum).GetDescription(), StringComparison.OrdinalIgnoreCase)).ToArray();

            if (!enumsFromDescription.Any())
            {
                throw new NotSupportedException($"No enum value exists described with {description} for enum type {typeof(T)}.");
            }

            return enumsFromDescription.First();
        }
    }
}