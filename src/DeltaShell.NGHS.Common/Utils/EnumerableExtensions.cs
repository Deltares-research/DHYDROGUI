using System;
using System.Collections.Generic;
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
        /// Thrown when a collection in <paramref name="sources"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// The collection are expected to be of equal size.
        /// The action is performed as long as both collections contain an element at the iteration.
        /// </remarks>
        public static void ForEach<T1, T2>(this (IEnumerable<T1>, IEnumerable<T2>) sources, Action<T1, T2> action)
        {
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
    }
}