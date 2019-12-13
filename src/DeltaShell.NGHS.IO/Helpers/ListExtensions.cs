using System.Collections.Generic;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class ListExtensions
    {
        public static IDictionary<T, int> ToIndexDictionary<T>(this IList<T> list)
        {
            var dictionary = new Dictionary<T, int>();

            for (int i = 0; i < list.Count; i++)
            {
                dictionary.Add(list[i], i);
            }

            return dictionary;
        }
    }
}