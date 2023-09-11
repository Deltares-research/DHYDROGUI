using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.Utils;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class GwswElementGenerationActivity<T> : Activity
    {
        private readonly SewerFeatureType elementType;
        private readonly GwswElement[] gwswElements;
        private readonly GwswFileImporter gwswFileImporter;
        private IGwswFeatureGenerator<T> generator;
        private int nrOfGwswFeatures;
        public ConcurrentQueue<T> Features { get; } = new ConcurrentQueue<T>();
        public ConcurrentQueue<string> GenerationExceptions { get; } = new ConcurrentQueue<string>();


        public GwswElementGenerationActivity(SewerFeatureType elementKey, GwswElement[] element, GwswFileImporter gwswFileImporter)
        {
            elementType = elementKey;
            gwswElements = element;
            this.gwswFileImporter = gwswFileImporter;
            Name = $"Generating gwsw element of type {elementKey.ToString()}";
        }

        protected override void OnInitialize()
        {
            nrOfGwswFeatures = gwswElements.Count();
        }

        protected override void OnExecute()
        {
            var stepSize = nrOfGwswFeatures / 20;
            var current = 0;
            CancellationTokenSource cts = new CancellationTokenSource();

            // Use ParallelOptions instance to store the CancellationToken
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cts.Token;
            po.MaxDegreeOfParallelism = Environment.ProcessorCount * 2;
            var importedFeatureElements = gwswElements.ToArray();
            var nrOfImportedFeatureElements = importedFeatureElements.Length;
            OrderablePartitioner<GwswElement> partitioner = Partitioner.Create(importedFeatureElements, EnumerablePartitionerOptions.NoBuffering);
            Task t = new Task(() => Parallel.ForEach(partitioner, po, (gwswElement, s, l) =>
            {
                try
                {
                    if (po.CancellationToken.IsCancellationRequested)
                        s.Break();
                    generator = SewerFeatureFactory.GetGwswFeatureGenerator<T>(elementType, gwswElement, gwswFileImporter.LogHandler);
                    if (generator != null)
                    {
                        var generatedFeature = generator.Generate(gwswElement);
                        if (generatedFeature != null)
                        {
                            Features.Enqueue(generatedFeature);
                        }
                    }

                }
                catch(Exception e)
                {
                    GenerationExceptions.Enqueue(e.Message);
                }
                finally
                {
                    Interlocked.Increment(ref current);
                }
                
            }));

            t.Start();
            int step = 0;
            while (!t.IsCompleted)
            {
                if (stepSize != 0 && current / stepSize > step)
                {
                    step = current / stepSize;

                    EventingHelper.DoWithEvents(() =>
                    {
                        SetProgressText($"Generating {elementType}  features {current} / {nrOfImportedFeatureElements}");
                    });
                }
                Thread.Sleep(100);
                if (gwswFileImporter != null && gwswFileImporter.ShouldCancel)
                    cts.Cancel();
            }
            Status = ActivityStatus.Done;
        }

        protected override void OnCancel()
        {
            
        }

        protected override void OnCleanUp()
        {
            
        }

        protected override void OnFinish()
        {
            if (elementType != SewerFeatureType.Structure) return;
            foreach (var structure1D in Features.OfType<IStructure1D>().Where(structure1D=> structure1D.Branch != null && structure1D.Branch.Source.Equals(structure1D.Branch.Target)))
            {
                    // is internal connection
                    structure1D.ParentPointFeature = (Manhole)structure1D.Branch.Source;
            }

        }
    }
}