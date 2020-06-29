using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using log4net;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class ToDictionaryExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ToDictionaryExtensions));

        /// <summary>
        /// Performs a ToDictionary, but catches non-unique key exceptions and prints the keys that are not unique
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="context">The context of the operation. For example a file path or other information useful for the user to fix the problem.</param>
        /// <param name="keySelector"></param>
        /// <param name="valueSelector"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> ToDictionaryWithErrorDetails<TKey, TValue, T>(
            this IEnumerable<T> source,
            string context,
            Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector)
        {
            try
            {
                return source.ToDictionary(keySelector, valueSelector);
            }
            catch (ArgumentException)
            {
                var sourceKeys = source.Select(keySelector).ToList();
                var nonUniqueKeys = sourceKeys.ToList();
                var distinctKeys = nonUniqueKeys.Distinct().ToList();
                distinctKeys.ForEach(id => nonUniqueKeys.Remove(id));

                var numNonUnique = nonUniqueKeys.Count;
                nonUniqueKeys = nonUniqueKeys.Distinct().ToList();
                var nonUniqueEntries = string.Join(", ", nonUniqueKeys);

                var indexOfFirstNonUniqueEntry = sourceKeys.IndexOf(nonUniqueKeys.First()) + 1; //make it 1-based

                var message =
                    string.Format("The following entries were not unique: '{0}', first encountered at the {1} entry (total non-unique: {2}), in: {3}.",
                                  nonUniqueEntries,
                                  ToOrdinalSuffixString(indexOfFirstNonUniqueEntry),
                                  numNonUnique,
                                  context);
                throw new ArgumentException(message);
            }
        }
        
        private static string ToOrdinalSuffixString(int number)
        {
            string suffix;
            switch (number)
            {
                case 1:
                    suffix = "st";
                    break;
                case 2:
                    suffix = "nd";
                    break;
                case 3:
                    suffix = "rd";
                    break;
                default:
                    suffix = "th";
                    break;
            }

            return string.Format("{0}{1}", number, suffix);
        }

        public static Dictionary<TKey, T> ToDictionaryWithErrorDetails<TKey, T>(this IEnumerable<T> source,
                                                                                 string context,
                                                                                 Func<T, TKey> keySelector)
        {
            return ToDictionaryWithErrorDetails(source, context, keySelector, x => x);
        }

        public static Dictionary<TKey, TValue> ToDictionaryWithDuplicateWarnings<TKey, TValue, T>(
            this IEnumerable<T> source,
            string context,
            Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector)
        {

            var dictionary = new Dictionary<TKey, TValue>();
            var duplicateKeys = new Collection<TKey>();
            foreach (var item in source)
            {
                var key = keySelector(item);
                if (dictionary.ContainsKey(key))
                {
                    duplicateKeys.Add(key);
                    continue;
                }

                var value = valueSelector(item);
                dictionary[key] = value;
            }

            if (duplicateKeys.Any())
            {
                var message =
                    string.Format("The following entries were not unique: '{0}'. Total non-unique: {1}, in: {2}.",
                        string.Join(", ", duplicateKeys),
                        duplicateKeys.Count,
                        context);
                Log.Warn(message);
            }

            return dictionary;
        }

        public static Dictionary<TKey, T> ToDictionaryWithDuplicateWarnings<TKey, T>(
            this IEnumerable<T> source,
            string context,
            Func<T, TKey> keySelector)
        {

            return ToDictionaryWithDuplicateWarnings(source, context, keySelector, x => x);
        }
    }
}