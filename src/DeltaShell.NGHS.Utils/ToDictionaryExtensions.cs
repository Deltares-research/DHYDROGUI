using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using log4net;
using log4net.Core;

namespace DeltaShell.NGHS.Utils
{
    public static class ToDictionaryExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ToDictionaryExtensions));

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

        public static Dictionary<TKey, T> ToDictionaryWithErrorDetails<TKey, T>(this IEnumerable<T> source, string context, Func<T, TKey> keySelector)
        {
            return ToDictionaryWithErrorDetails(source, context, keySelector, x => x);
        }

        /// <summary>
        /// Creates a dictionary with selected keys and values and logs duplicates with the provided log level (default warning)
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <typeparam name="T">The type of the original object</typeparam>
        /// <param name="source">Source enumeration of type <typeparamref name="T"/></param>
        /// <param name="context">Name of the enumeration</param>
        /// <param name="keySelector">Method to select keys from the <typeparamref name="T"/> object</param>
        /// <param name="valueSelector">Method to select values from the <typeparamref name="T"/> object</param>
        /// <param name="logLevel">Level of log message (default is warning)</param>
        /// <returns>Created dictionary without duplicates</returns>
        public static Dictionary<TKey, TValue> ToDictionaryWithDuplicateLogging<TKey, TValue, T>(this IEnumerable<T> source, string context, Func<T, TKey> keySelector, Func<T, TValue> valueSelector, Level logLevel = null)
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

                dictionary[key] = valueSelector(item);
            }

            if (duplicateKeys.Any())
            {
                var message = $"The following entries were not unique: '{string.Join(", ", duplicateKeys)}'. Total non-unique: {duplicateKeys.Count}, in: {context}.";
                logLevel = logLevel ?? Level.Warn;
                log.Logger.Log(typeof(ToDictionaryExtensions),logLevel, message,null);                
            }

            return dictionary;
        }

        /// <summary>
        /// Creates a dictionary with selected keys and the source elements as values and logs duplicates with the provided log level (default warning)
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="T">The type of the original object</typeparam>
        /// <param name="source">Source enumeration of type <typeparamref name="T"/></param>
        /// <param name="context">Name of the enumeration</param>
        /// <param name="keySelector">Method to select keys from the <typeparamref name="T"/> object</param>
        /// <param name="logLevel">Level of log message (default is warning)</param>
        /// <returns>Created dictionary without duplicates</returns>
        public static Dictionary<TKey, T> ToDictionaryWithDuplicateLogging<TKey, T>(this IEnumerable<T> source, string context, Func<T, TKey> keySelector, Level logLevel = null)
        {

            return ToDictionaryWithDuplicateLogging(source, context, keySelector, x => x, logLevel);
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

            return $"{number}{suffix}";
        }
    }
}