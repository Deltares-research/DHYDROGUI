using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// Contains a set of extension methods for a collection
    /// of <see cref="INameable"/> objects.
    /// </summary>
    public static class NameableCollectionExtensions
    {
        /// <summary>
        /// Gets the first or default <see cref="INameable"/> object by name from a collection.
        /// </summary>
        /// <typeparam name="T">The type of the searched object.</typeparam>
        /// <param name="objects">The objects.</param>
        /// <param name="name">The name of the searched object.</param>
        /// <param name="comparisonType"> Optional parameter; the type of comparison used to compare the strings. </param>
        /// <returns>
        /// The first object with the same name; default if the object was not found.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="objects"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="comparisonType"/> is not defined.
        /// </exception>
        public static T GetByName<T>(this IEnumerable<T> objects, string name, StringComparison comparisonType = StringComparison.Ordinal)
            where T : INameable
        {
            Ensure.NotNull(objects, nameof(objects));
            Ensure.IsDefined(comparisonType, nameof(comparisonType));

            return objects.FirstOrDefault(o => string.Equals(o.Name, name, comparisonType));
        }

        /// <summary>
        /// Gets all elements in an <see cref="INameable"/> sequence with the specified <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The type of the searched objects.</typeparam>
        /// <param name="objects">The objects.</param>
        /// <param name="name">The name of the searched objects.</param>
        /// <returns>
        /// A collection containing the objects with the same name.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="objects"/> is <c>null</c>.
        /// </exception>
        public static IEnumerable<T> GetAllByName<T>(this IEnumerable<T> objects, string name) where T : INameable
        {
            Ensure.NotNull(objects, nameof(objects));
            return objects.Where(o => o.Name == name);
        }
    }
}