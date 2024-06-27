using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="WaveModelLayerSubProvider"/> implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="WaveModel"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    public class WaveModelLayerSubProvider : ILayerSubProvider
    {
        private readonly IWaveLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="WaveModelLayerSubProvider"/>.
        /// </summary>
        /// <param name="instanceCreator">The factory to build the layers with.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Throw when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public WaveModelLayerSubProvider(IWaveLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));

            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is WaveModel;
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            return sourceData is WaveModel waveModel
                       ? instanceCreator.CreateModelGroupLayer(waveModel)
                       : null;
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is WaveModel model))
            {
                yield break;
            }

            yield return BoundaryMapFeaturesContainerFactory.ConstructEditableBoundaryMapFeaturesContainer(
                model.BoundaryContainer,
                model.CoordinateSystem);

            yield return model.FeatureContainer.Obstacles;
            yield return model.FeatureContainer.ObservationPoints;
            yield return model.FeatureContainer.ObservationCrossSections;

            foreach (WaveDomainData waveDomain in WaveDomainHelper.GetAllDomains(model.OuterDomain))
            {
                yield return waveDomain;
            }

            if (model.WaveOutputData.IsConnected)
            {
                yield return model.WaveOutputData;
            }
        }
    }
}