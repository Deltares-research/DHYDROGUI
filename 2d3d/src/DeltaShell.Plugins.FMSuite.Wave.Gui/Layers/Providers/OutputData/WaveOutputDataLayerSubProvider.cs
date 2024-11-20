using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
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
        private readonly IWaveLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="WaveOutputDataLayerSubProvider"/>.
        /// </summary>
        /// <param name="instanceCreator">The factory to build the layers with.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Throw when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public WaveOutputDataLayerSubProvider(IWaveLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));

            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is IWaveOutputData outputData && outputData.IsConnected;

        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is IWaveOutputData outputData && outputData.IsConnected
                ? instanceCreator.CreateWaveOutputDataLayer(outputData)
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is IWaveOutputData outputData))
            {
                yield break;
            }

            if (outputData.WavmFileFunctionStores.Any(x => x.Functions.Any()))
            {
                yield return outputData.WavmFileFunctionStores;
            }

            if (outputData.WavhFileFunctionStores.Any(x => x.Functions.Any()))
            {
                yield return outputData.WavhFileFunctionStores;
            }
        }
    }
}