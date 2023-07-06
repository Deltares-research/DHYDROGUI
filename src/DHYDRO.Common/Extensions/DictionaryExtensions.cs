using System.Collections.Generic;
using DHYDRO.Common.Guards;

namespace DHYDRO.Common.Extensions
{
    /// <summary>
    /// Provides extensions methods for an <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Adds the specified <paramref name="value"/> to the list at corresponding <paramref name="key"/>.
        /// If no entry exists with the given <paramref name="key"/>, a new list is initialized with the <paramref name="value"/>.
        /// </summary>
        /// <param name="dictionary"> The dictionary to update. </param>
        /// <param name="key"> The key. </param>
        /// <param name="value"> The value to add to the list for the given key. </param>
        /// <typeparam name="TKey"> Type of key. </typeparam>
        /// <typeparam name="TValue"> Type of value. </typeparam>
        /// <remarks>
        /// The assumption is 
        /// </remarks>
        public static void AddToList<TKey, TValue>(this IDictionary<TKey, IList<TValue>> dictionary, TKey key, TValue value)
        {
            Ensure.NotNull(dictionary, nameof(dictionary));

            if (dictionary.ContainsKey(key))
            {
                dictionary[key].Add(value);
            }
            else
            {
                dictionary[key] = new List<TValue> { value };
            }
        }
    }
}