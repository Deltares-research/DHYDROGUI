using System.Collections.Generic;
using DeltaShell.NGHS.Common;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="WaveMapLayerProvider"/> implements the
    /// <see cref="IWaveLayerSubProvider"/> for data of type <see cref="WaveModel"/>.
    /// </summary>
    /// <seealso cref="IWaveLayerSubProvider" />
    public class WaveModelLayerSubProvider : IWaveLayerSubProvider
    {
        private readonly IWaveLayerFactory factory;

        /// <summary>
        /// Creates a new <see cref="WaveModelLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public WaveModelLayerSubProvider(IWaveLayerFactory factory)
        {
            Ensure.NotNull(factory, nameof(factory));

            this.factory = factory;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is WaveModel;
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        { 
            return sourceData is WaveModel waveModel ? factory.CreateModelGroupLayer(waveModel) 
                                                     : null;
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            yield break;
        }
    }
}