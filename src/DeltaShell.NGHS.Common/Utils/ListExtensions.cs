using System;
using System.Collections.Generic;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// Extension methods for <see cref="IList{T}"/>.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate,
        /// and returns the zero-based index of the first occurrence within the entire <paramref name="source"/>.
        /// </summary>
        /// <param name="source"> The list of elements. </param>
        /// <param name="predicate"> The delegate that defines the conditions of the element to search for.</param>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <returns>
        /// The zero-based index of the first occurrence within the entire <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.
        /// </exception>
        public static int FindIndex<T>(this IList<T> source, Func<T, bool> predicate)
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(predicate, nameof(predicate));

            for (var i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}