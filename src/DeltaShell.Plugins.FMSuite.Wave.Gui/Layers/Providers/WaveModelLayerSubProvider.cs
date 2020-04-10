using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.Layers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="WaveModelLayerSubProvider"/> implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="WaveModel"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider" />
    public class WaveModelLayerSubProvider : ILayerSubProvider
    {
        private readonly IWaveLayerFactory factory;

        /// <summary>
        /// Creates a new <see cref="WaveModelLayerSubProvider"/>.
        /// </summary>
        /// <param name="factory">The factory to build the layers with.</param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="factory"/> is <c>null</c>.
        /// </exception>
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
            if (!(data is WaveModel model))
            {
                yield break;
            }

            yield return new WaveSnappedFeaturesGroupLayerData(model);

            yield return BoundaryMapFeaturesContainerFactory.ConstructEditableBoundaryMapFeaturesContainer(
                model.BoundaryContainer, 
                model.CoordinateSystem);

            yield return model.BoundaryConditions;
            yield return model.Boundaries;
            yield return model.Sp2Boundaries;
            yield return model.Obstacles;
            yield return model.ObservationPoints;
            yield return model.ObservationCrossSections;

            foreach (WaveDomainData waveDomain in WaveDomainHelper.GetAllDomains(model.OuterDomain))
            {
                yield return waveDomain;
            }

            IEnumerable<WavmFileFunctionStore> relevantFunctionStores =
                model.WavmFunctionStores.Where(fs => fs.Functions.Any() && 
                                                     !string.IsNullOrEmpty(fs.Path));
            foreach (WavmFileFunctionStore wavmFunctionStore in relevantFunctionStores)
            {
                yield return wavmFunctionStore;
            }
        }
    }
}