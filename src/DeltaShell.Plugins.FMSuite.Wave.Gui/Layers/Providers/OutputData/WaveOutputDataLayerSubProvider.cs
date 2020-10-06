using System;
using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData
{
    /// <summary>
    /// <see cref="WaveOutputDataLayerSubProvider"/> implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="IWaveOutputData"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider" />
    public class WaveOutputDataLayerSubProvider : ILayerSubProvider
    {
        private readonly IWaveLayerFactory factory;

        /// <summary>
        /// Creates a new <see cref="WaveOutputDataLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory">The factory to build the layers with.</param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="factory"/> is <c>null</c>.
        /// </exception>
        public WaveOutputDataLayerSubProvider(IWaveLayerFactory factory)
        {
            Ensure.NotNull(factory, nameof(factory));

            this.factory = factory;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is IWaveOutputData outputData && outputData.IsConnected;

        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is IWaveOutputData outputData && outputData.IsConnected
                ? factory.CreateWaveOutputDataLayer(outputData)
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            yield break;
        }
    }
}