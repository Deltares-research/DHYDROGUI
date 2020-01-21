using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers;

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
            var factory = new WaveLayerFactory();

            yield return new BoundaryMapFeaturesContainerLayerSubProvider(factory);
            yield return new DiscreteGridPointCoverageLayerSubProvider(factory, getWaveModelsFunc);
            yield return new ObservationCrossSectionLayerSubProvider(factory);
            yield return new ObservationPointLayerSubProvider(factory);
            yield return new ObstacleLayerSubProvider(factory);
            yield return new Sp2BoundaryLayerSubProvider(factory);
            yield return new WaveBoundaryConditionLayerSubProvider();
            yield return new WaveBoundaryLayerSubProvider(factory);
            yield return new WaveDomainDataLayerSubProvider(factory);
            yield return new WaveModelLayerSubProvider(factory);
            yield return new WaveSnappedFeaturesGroupLayerDataLayerSubProvider(factory);
            yield return new WavmFileFunctionStoreLayerSubProvider(factory, getWaveModelsFunc);
        }
    }
}