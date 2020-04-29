using System.Collections.Generic;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.Wave.Layers
{
    public class WaveSnappedFeaturesGroupLayerData
    {
        private readonly WaveModel model;

        public WaveSnappedFeaturesGroupLayerData(WaveModel model)
        {
            this.model = model;
        }

        public IEnumerable<FeatureCollection> ChildData
        {
            get
            {
                yield break; // TODO place new boundaries?
            }
        }
    }
}