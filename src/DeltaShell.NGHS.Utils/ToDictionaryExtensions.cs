using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using log4net;
using log4net.Core;

namespace DeltaShell.NGHS.Utils
{
    public static class ToDictionaryExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ToDictionaryExtensions));

        /// <summary>
        /// Performs a ToDictionary, but if there are non-unique keys it throws an <see cref="ArgumentException"/>
        /// with the keys that are not unique and their index.
        /// </summary>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <typeparam name="TSource">Type of the source</typeparam>
        /// <param name="source">Source to query</param>
        /// <param name="context">The context of the operation. For example a file path or other information useful for the user to fix the problem.</param>
        /// <param name="keySelector">Function for selecting a key</param>
        /// <param name="valueSelector">Function for selecting value</param>
        /// <returns>Dictionary of <typeparamref name="TKey"/> and <typeparamref name="TValue"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when there are duplicates found with the <paramref name="keySelector"/>.</exception>
        public static Dictionary<TKey, TValue> ToDictionaryWithErrorDetails<TKey, TValue, TSource>(
            this IEnumerable<TSource> source,
            string context,
            Func<TSource, TKey> keySelector,
            Func<TSource, TValue> valueSelector)
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(context, nameof(context));
            Ensure.NotNull(keySelector, nameof(keySelector));
            Ensure.NotNull(valueSelector, nameof(valueSelector));

            try
            {
                return source.ToDictionary(keySelector, valueSelector);
            }
            catch (ArgumentException)
            {
                IEnumerable<string> nonUniqueDescriptions = GetNonUniqueDescriptors(source, keySelector);
                string message = GenerateErrorMessage(context, nonUniqueDescriptions);

                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Performs a ToDictionary, but if there are non-unique keys it throws an <see cref="ArgumentException"/>
        /// with the keys that are not unique and there index
        /// </summary>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TSource">Type of the source</typeparam>
        /// <param name="source">Source to query</param>
        /// <param name="context">The context of the operation. For example a file path or other information useful for the user to fix the problem.</param>
        /// <param name="keySelector">Function for selecting a key</param>
        /// <returns>Dictionary of <typeparamref name="TKey"/> and <typeparamref name="TSource"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when there are duplicates found with the <paramref name="keySelector"/>.</exception>
        public static Dictionary<TKey, TSource> ToDictionaryWithErrorDetails<TKey, TSource>(this IEnumerable<TSource> source, string context, Func<TSource, TKey> keySelector)
        {
            return ToDictionaryWithErrorDetails(source, context, keySelector, x => x);
        }

        private static IEnumerable<string> GetNonUniqueDescriptors<TKey, TSource>(
            IEnumerable<TSource> source, 
            Func<TSource, TKey> keySelector)
        {
            return source
                   .Select((item, index) => new
                   {
                       item,
                       index
                   })
                   .GroupBy(i => keySelector(i.item))
                   .Where(g => g.Count() > 1)
                   .Select(g => $"{g.Key} at indices ({string.Join(", ", g.Select(i => i.index))})");
        }
        
        private static string GenerateErrorMessage(string context, IEnumerable<string> nonUniqueDescriptions)
        {
            return $@"The following entries were not unique in {context}: {Environment.NewLine}" +
                   $"{string.Join(Environment.NewLine, nonUniqueDescriptions)}";
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
        /// <param name="comparer"></param>
        /// <returns>Created dictionary without duplicates</returns>
        public static Dictionary<TKey, TValue> ToDictionaryWithDuplicateLogging<TKey, TValue, T>(this IEnumerable<T> source, string context, Func<T, TKey> keySelector, Func<T, TValue> valueSelector, Level logLevel = null, IEqualityComparer<TKey> comparer = null)
        {
            var dictionary = comparer != null ? new Dictionary<TKey, TValue>(comparer) : new Dictionary<TKey, TValue>();
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
    }
}