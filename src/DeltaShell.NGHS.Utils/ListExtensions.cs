using System.Collections.Generic;

namespace DeltaShell.NGHS.Utils
{
    public static class ListExtensions
    {
        /// <summary>
        /// Creates a dictionary to lookup the index for an item
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="list">List of items</param>
        /// <param name="skipDuplicates">Skip duplicates items in dictionary (otherwise an error is thrown)</param>
        public static IDictionary<T, int> ToIndexDictionary<T>(this IList<T> list, bool skipDuplicates = false)
        {
            var dictionary = new Dictionary<T, int>();

            for (int i = 0; i < list.Count; i++)
            {
                if (!skipDuplicates)
                {
                    dictionary.Add(list[i], i);
                }
                else if (!dictionary.ContainsKey(list[i]))
                {
                    dictionary.Add(list[i], i);
                }
            }

            return dictionary;
        }
    }
}