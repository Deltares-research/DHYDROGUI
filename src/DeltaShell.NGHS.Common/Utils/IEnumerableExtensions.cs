using System;
using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate,
        /// and returns the zero-based index of the first occurrence within the entire <paramref name="source"/>.
        /// </summary>
        /// <param name="source"> The collection of elements. </param>
        /// <param name="predicate"> The delegate that defines the conditions of the element to search for.</param>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <returns>
        /// When found, the zero-based index of the first occurrence within the entire <paramref name="source"/>
        /// that satisfies the <paramref name="predicate"/>; otherwise, -1.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.
        /// </exception>
        public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(predicate, nameof(predicate));

            var i = 0;
            foreach (T item in source)
            {
                if (predicate(item))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        /// <summary>
        /// Produces a sequence equal to the <paramref name="source"/> excluding the <paramref name="item"/>,
        /// using the default equality comparer to compare values.
        /// </summary>
        /// <param name="source">The source <see cref="IEnumerable{T}"/> from which to exclude the <paramref name="item"/> from.</param>
        /// <param name="item"> The item the will be excluded from the <paramref name="source"/>.</param>
        /// <typeparam name="T"> The type of the elements of the source sequence.</typeparam>
        /// <returns>
        /// A sequence equal to the <paramref name="source"/> excluding the <paramref name="item"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static IEnumerable<T> Except<T>(this IEnumerable<T> source, T item)
        {
            Ensure.NotNull(source, nameof(source));

            foreach (T sourceItem in source)
            {
                if (!Equals(sourceItem, item))
                {
                    yield return sourceItem;
                }
            }
        }
    }
}