using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures
{
    /// <summary>
    /// <see cref="EstimatedSnappedFeatureLayerSubProvider"/> is responsible for creating
    /// estimated snapped feature layers out of <see cref="EstimatedSnappedFeatureData"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal sealed class EstimatedSnappedFeatureLayerSubProvider : ILayerSubProvider
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
        public EstimatedSnappedFeatureLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is EstimatedSnappedFeatureData;

        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is EstimatedSnappedFeatureData data
                ? instanceCreator.CreateEstimatedSnappedFeatureLayer(data.Model, data.FeatureType)
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data) =>
            Enumerable.Empty<object>();
    }
}