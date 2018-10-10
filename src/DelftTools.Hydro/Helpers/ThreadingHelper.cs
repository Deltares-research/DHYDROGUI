using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DelftTools.Hydro.Helpers
{
    public static class ThreadingHelper
    {
        /// <summary>
        /// Converts items on separate threads
        /// </summary>
        /// <typeparam name="TL">Type of the original list items</typeparam>
        /// <typeparam name="TR">Type of the result list items</typeparam>
        /// <param name="originalItems">List of items to convert</param>
        /// <param name="action">Action that converts TL to TR</param>
        /// <param name="numberOfThreads">Number of threads to use (default = -1 (Auto determine (between 1 and 10)))</param>
        /// <returns>Converted list of TR items</returns>
        public static IList<TR> ConvertMultiThreaded<TL, TR>(this IList<TL> originalItems, Func<TL, TR> action, int numberOfThreads = -1)
        {
            numberOfThreads = numberOfThreads == -1 ? Math.Min(10, Math.Max(1, originalItems.Count / 500)) : numberOfThreads;

            if (numberOfThreads == 1)
            {
                // no multi threading required
                return originalItems.Select(action).ToList();
            }

            var numberOfElementsPerThread = (int)Math.Round((double)originalItems.Count / numberOfThreads);

            var tasks = new List<Task>();
            var total = new IList<TR>[numberOfThreads];

            for (int i = 0; i < numberOfThreads; i++)
            {
                var numberOfElementsToTake = i == numberOfThreads - 1
                    ? originalItems.Count - (i * numberOfElementsPerThread)
                    : numberOfElementsPerThread;

                var itemsForThread = originalItems.Skip(i * numberOfElementsPerThread).Take(numberOfElementsToTake).ToList();
                var i1 = i;

                tasks.Add(Task.Factory.StartNew(() => total[i1] = itemsForThread.Select(action).ToList()));
            }

            Task.WaitAll(tasks.ToArray());

            return total.SelectMany(l => l).ToList();
        }
    }
}