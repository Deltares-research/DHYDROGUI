using System.Collections.Generic;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Layers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="WaveSnappedFeaturesGroupLayerDataLayerSubProvider"/> implements the
    /// <see cref="IWaveLayerSubProvider"/> for data of type <see cref="WaveSnappedFeaturesGroupLayerData"/>.
    /// </summary>
    /// <seealso cref="IWaveLayerSubProvider" />
    public class WaveSnappedFeaturesGroupLayerDataLayerSubProvider : IWaveLayerSubProvider
    {
        private readonly IWaveLayerFactory factory;

        /// <summary>
        /// Creates a new <see cref="WaveSnappedFeaturesGroupLayerDataLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory">The factory to build the layers with.</param>
        public WaveSnappedFeaturesGroupLayerDataLayerSubProvider(IWaveLayerFactory factory)
        {
            Ensure.NotNull(factory, nameof(factory));
            this.factory = factory;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is WaveSnappedFeaturesGroupLayerData;
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            return sourceData is WaveSnappedFeaturesGroupLayerData data
                       ? factory.CreateSnappedFeaturesLayer(data)
                       : null;
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            yield break;
        }
    }
}