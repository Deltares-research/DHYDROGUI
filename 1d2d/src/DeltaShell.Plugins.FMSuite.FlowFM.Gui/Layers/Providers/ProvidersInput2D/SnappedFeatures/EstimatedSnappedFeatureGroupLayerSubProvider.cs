using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures
{
    /// <summary>
    /// <see cref="EstimatedSnappedFeatureGroupLayerSubProvider"/> is responsible for creating
    /// the estimated snapped feature group layers out of <see cref="EstimatedSnappedFeatureGroupData"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal sealed class EstimatedSnappedFeatureGroupLayerSubProvider : ILayerSubProvider
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="BoundariesLayerSubProvider"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public EstimatedSnappedFeatureGroupLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is EstimatedSnappedFeatureGroupData;

        public ILayer CreateLayer(object sourceData, object parentData) =>
            CanCreateLayerFor(sourceData, parentData) 
                ? instanceCreator.CreateEstimatedSnappedFeatureGroupLayer() 
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data) =>
            data is EstimatedSnappedFeatureGroupData layerData
                ? FeatureTypes.Select(t => new EstimatedSnappedFeatureData(layerData.Model, t))
                : Enumerable.Empty<object>();

        private static IEnumerable<EstimatedSnappedFeatureType> FeatureTypes =>
            (EstimatedSnappedFeatureType[])System.Enum.GetValues(typeof(EstimatedSnappedFeatureType));
    }
}