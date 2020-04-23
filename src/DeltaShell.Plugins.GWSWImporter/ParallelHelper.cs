using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using log4net;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    internal static class ParallelHelper
    {
        private static ILog Log = LogManager.GetLogger(typeof(ParallelHelper));

        public static void RunActionInParallel<T>(IFileImporter importer, T[] elementUsedInAction, Action<T> action, string importTo)
        {
            var nrOfElements = elementUsedInAction.Length;
            var stepSize = nrOfElements / 20;
            var listOfErrors = new ConcurrentQueue<string>();
            var current = 0;

            CancellationTokenSource cts = new CancellationTokenSource();

            // Use ParallelOptions instance to store the CancellationToken
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cts.Token;
            po.MaxDegreeOfParallelism = 1;//Environment.ProcessorCount * 2;
            OrderablePartitioner<T> partitioner =
                Partitioner.Create(elementUsedInAction, EnumerablePartitionerOptions.NoBuffering);
            Task t = new Task(() => Parallel.ForEach(partitioner, po, (e, s, l) =>
            {
                try
                {
                    if (po.CancellationToken.IsCancellationRequested)
                        s.Break();
                    action?.Invoke(e);
                }
                catch (Exception exception)
                {
                    listOfErrors.Enqueue(exception.Message + Environment.NewLine);
                }
                finally
                {
                    Interlocked.Increment(ref current);
                }
            }));
            var bubblingEnabled = EventSettings.BubblingEnabled;
            try
            {
                t.Start();
                int step = 0;
                while (!t.IsCompleted)
                {
                    if (stepSize != 0 && current / stepSize > step)
                    {
                        step = current / stepSize;
                        EventSettings.BubblingEnabled = true;
                        importer.ProgressChanged?.Invoke($"{importTo} ({((double) ((double) current / (double) nrOfElements)):P0})", current, nrOfElements);
                        EventSettings.BubblingEnabled = false;
                    }

                    Thread.Sleep(200);
                    if (importer.ShouldCancel)
                        cts.Cancel();
                }
                EventSettings.BubblingEnabled = true;
                importer.ProgressChanged?.Invoke($"{importTo} ({((double)((double)nrOfElements / (double)nrOfElements)):P0})", nrOfElements, nrOfElements);
                EventSettings.BubblingEnabled = false;
                if (listOfErrors.Any())
                    Log.ErrorFormat($"While {importTo} we encountered the following errors: {Environment.NewLine}{String.Join(Environment.NewLine, listOfErrors)}");
            }
            finally
            {
                EventSettings.BubblingEnabled = bubblingEnabled;
            }
        }
    }
}