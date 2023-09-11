using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.Utils;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    internal static class ParallelHelper
    {
        public static void RunActionInParallel<T>(GwswFileImporter importer, T[] elementUsedInAction, Action<T> action, string importTo)
        {
            var nrOfElements = elementUsedInAction.Length;
            var current = 0;
            
            CancellationTokenSource cts = new CancellationTokenSource();

            // Use ParallelOptions instance to store the CancellationToken
            ParallelOptions po = new ParallelOptions();
            
            po.CancellationToken = cts.Token;
            po.MaxDegreeOfParallelism = 1;// this needs to be determined and optimized... Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 2.0));
            OrderablePartitioner<T> partitioner =
                Partitioner.Create(elementUsedInAction, EnumerablePartitionerOptions.None);
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
                    importer.LogHandler?.ReportError(exception.Message);
                }
                finally
                {
                    Interlocked.Increment(ref current);
                }
            }));
            EventingHelper.DoWithoutEvents(() =>
            {
                ProcessAction(importer, importTo, t, ref current, nrOfElements, cts);                
                
            });
        }
        private static void ProcessAction(IFileImporter importer, string importTo, Task actionTaskToBeExecuted, ref int current, int nrOfElements, CancellationTokenSource cts)
        {
            var stepSize = nrOfElements < 500 ?
                               nrOfElements / 20 :
                                   nrOfElements < 1000 ?
                                    nrOfElements / 10 : 
                                        nrOfElements / 5;
            actionTaskToBeExecuted.Start();
            int step = 0;
            
            while (!actionTaskToBeExecuted.IsCompleted)
            {
                if (stepSize != 0 && current / stepSize > step)
                {
                    step = current / stepSize;
                    ReportProgress(importer, importTo, current, nrOfElements);
                }

                Thread.Sleep(20);
                if (importer.ShouldCancel)
                    cts.Cancel();
            }

            ReportProgress(importer, importTo, nrOfElements, nrOfElements);
        }

        private static void ReportProgress(IFileImporter importer, string importTo, int current, int nrOfElements)
        {
            EventingHelper.DoWithEvents(() => { importer.ProgressChanged?.Invoke($"{importTo} ({(double) current / nrOfElements:P0})", current, nrOfElements); });
        }
    }
}