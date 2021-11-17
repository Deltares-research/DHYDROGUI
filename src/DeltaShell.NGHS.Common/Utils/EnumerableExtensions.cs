using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// Contains <see cref="IEnumerable{T}"/> extensions.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Performs an action for each pair of items in the two specified sequences.
        /// </summary>
        /// <param name="sources"> The value tuple containing the first and second sequence. </param>
        /// <param name="action"> The action to perform. </param>
        /// <typeparam name="T1">The type of the elements of the first input sequence.</typeparam>
        /// <typeparam name="T2">The type of the elements of the second input sequence.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown when a collection in <paramref name="sources"/>  or <paramref name="action"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// The collection are expected to be of equal size.
        /// The action is performed as long as both collections contain an element at the iteration.
        /// </remarks>
        public static void ForEach<T1, T2>(this (IEnumerable<T1>, IEnumerable<T2>) sources, Action<T1, T2> action)
        {
            Ensure.NotNull(action, nameof(action));
            Ensure.NotNull(sources.Item1, $"{nameof(sources)}.{nameof(sources.Item1)}");
            Ensure.NotNull(sources.Item2, $"{nameof(sources)}.{nameof(sources.Item2)}");

            using (IEnumerator<T1> first = sources.Item1.GetEnumerator())
            using (IEnumerator<T2> second = sources.Item2.GetEnumerator())
            {
                while (first.MoveNext() && second.MoveNext())
                {
                    action(first.Current, second.Current);
                }
            }
        }

        /// <summary>
        /// Determines whether all elements in the collection are equal, using the default <see cref="IEqualityComparer{T}"/>>.
        /// </summary>
        /// <param name="source"> The source collection of which to compare the elements. </param>
        /// <typeparam name="T"> The type of elements in the collection. </typeparam>
        /// <returns>
        /// True, if all elements in the collection are equal or if the collection is empty;
        /// false, if not all elements in the collection are empty.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static bool AllEqual<T>(this IEnumerable<T> source)
        {
            Ensure.NotNull(source, nameof(source));

            T[] array = source as T[] ?? source.ToArray();
            return array.AllEqualTo(array.FirstOrDefault());
        }

        /// <summary>
        /// Determines whether all elements in the collection are unique, using the default <see cref="IEqualityComparer{T}"/>>.
        /// </summary>
        /// <param name="source"> The source collection of which to determine if all elements are unique. </param>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <returns>
        /// True, if all elements in the collection are unique or if the collection is empty;
        /// false, if the collection contains duplicate elements.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static bool AllUnique<T>(this IEnumerable<T> source)
        {
            Ensure.NotNull(source, nameof(source));
            
            var hashset = new HashSet<T>();
            
            foreach(T element in source)
            {
                if (!hashset.Add(element))
                {
                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// Invokes the <paramref name="selector"/> on each item of the <paramref name="source"/>, and returns the duplicates of
        /// this result, using the default <see cref="IEqualityComparer{T}"/>>.
        /// </summary>
        /// <param name="source"> The source collection. </param>
        /// <param name="selector"> The selector function to invoke for each item. </param>
        /// <typeparam name="TSource">The type of elements in the source collection.</typeparam>
        /// <typeparam name="TResult"> The type of element in the resulting collection.</typeparam>
        /// <returns>
        /// The duplicate values in the resulting collection.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="selector"/> is <c>null</c>.
        /// </exception>
        public static IEnumerable<TResult> Duplicates<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(selector, nameof(selector));

            return source.Select(selector).Duplicates();
        }

        /// <summary>
        /// Gets the duplicate values from a collection, using the default <see cref="IEqualityComparer{T}"/>>.
        /// </summary>
        /// <param name="source"> The collection to retrieve the duplicates from. </param>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <returns> The duplicate values in the collection. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> source)
        {
            Ensure.NotNull(source, nameof(source));

            var dictionary = new Dictionary<T, bool>();

            foreach (T item in source)
            {
                if (dictionary.TryGetValue(item, out bool isDuplicate))
                {
                    if (!isDuplicate)
                    {
                        dictionary[item] = true;
                        yield return item;
                    }
                }
                else
                {
                    dictionary[item] = false;
                }
            }
        }

        private static bool AllEqualTo<T>(this IEnumerable<T> source, T value)
        {
            return source.All(element => Equals(element, value));
        }
    }
}