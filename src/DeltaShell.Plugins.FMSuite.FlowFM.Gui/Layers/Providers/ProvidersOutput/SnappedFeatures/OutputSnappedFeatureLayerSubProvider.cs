using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures
{
    /// <summary>
    /// <see cref="OutputSnappedFeatureLayerSubProvider"/> is responsible for creating
    /// output snapped feature layers out of <see cref="OutputSnappedFeatureData"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal class OutputSnappedFeatureLayerSubProvider : ILayerSubProvider
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="OutputSnappedFeatureLayerSubProvider"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public OutputSnappedFeatureLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) => 
            sourceData is OutputSnappedFeatureData;

        public ILayer CreateLayer(object sourceData, object parentData) => 
            sourceData is OutputSnappedFeatureData featureData 
                ? instanceCreator.CreateOutputSnappedFeatureLayer(
                    featureData.LayerName,
                    featureData.SnappedFeatureDataPath,
                    featureData.Model) 
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data) =>
            Enumerable.Empty<object>();
    }
}