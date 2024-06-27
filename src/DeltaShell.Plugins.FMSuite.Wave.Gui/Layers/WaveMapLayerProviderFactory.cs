using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers.OutputData;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers
{
    /// <summary>
    /// <see cref="WaveMapLayerProviderFactory"/> provides the methods to
    /// construct a configured <see cref="IMapLayerProvider"/> for the
    /// Wave plugin.
    /// </summary>
    public static class WaveMapLayerProviderFactory
    {
        /// <summary>
        /// Constructs the <see cref="IMapLayerProvider"/> for the Waves plugin.
        /// </summary>
        /// <param name="getWaveModelsFunc">Function to obtain all the Wave models within the application.</param>
        /// <returns>A configured <see cref="IMapLayerProvider"/> for the Waves plugin.</returns>
        public static IMapLayerProvider ConstructMapLayerProvider(Func<IEnumerable<WaveModel>> getWaveModelsFunc)
        {
            ILayerSubProvider[] subProviders = GetSubProviders(getWaveModelsFunc).ToArray();
            var provider = new MapLayerProvider();

            provider.RegisterSubProviders(subProviders);

            return provider;
        }

        /// <summary>
        /// Gets the <see cref="ILayerSubProvider"/> required for the Waves plugin.
        /// </summary>
        /// <param name="getWaveModelsFunc">Function to obtain all the Wave models within the application.</param>
        /// <returns>The enumerable of <see cref="ILayerSubProvider"/> required for the Waves plugin.</returns>
        internal static IEnumerable<ILayerSubProvider> GetSubProviders(Func<IEnumerable<WaveModel>> getWaveModelsFunc)
        {
            Ensure.NotNull(getWaveModelsFunc, nameof(getWaveModelsFunc));
            var instanceCreator = new WaveLayerInstanceCreator();

            yield return new BoundaryMapFeaturesContainerLayerSubProvider(instanceCreator);
            yield return new DiscreteGridPointCoverageLayerSubProvider(instanceCreator, getWaveModelsFunc);
            yield return new ObservationCrossSectionLayerSubProvider(instanceCreator);
            yield return new ObservationPointLayerSubProvider(instanceCreator);
            yield return new ObstacleLayerSubProvider(instanceCreator);
            yield return new WaveDomainDataLayerSubProvider(instanceCreator);
            yield return new WaveModelLayerSubProvider(instanceCreator);
            yield return new WaveOutputDataLayerSubProvider(instanceCreator);
            yield return new WavmFileFunctionStoreGroupLayerSubProvider(instanceCreator);
            yield return new WavmFileFunctionStoreLayerSubProvider(instanceCreator, getWaveModelsFunc);
            yield return new WavhFileFunctionStoreGroupLayerSubProvider(instanceCreator);
            yield return new WavhFileFunctionStoreLayerSubProvider(instanceCreator, getWaveModelsFunc);
        }
    }
}