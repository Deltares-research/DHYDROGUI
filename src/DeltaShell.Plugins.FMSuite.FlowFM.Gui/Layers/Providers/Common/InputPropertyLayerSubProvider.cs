using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.Common
{
    /// <summary>
    /// <see cref="InputPropertyLayerSubProvider{TModelProperty}"/> implements the
    /// <see cref="ILayerSubProvider"/> for model properties of the <see cref="IWaterFlowFMModel"/>
    /// with the help of a <typeparamref name="TModelProperty"/>.
    /// </summary>
    /// <typeparam name="TModelProperty">The type of <see cref="IModelProperty"/> used.</typeparam>
    internal sealed class InputPropertyLayerSubProvider<TModelProperty> : ILayerSubProvider
        where TModelProperty: IModelProperty, new()
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;
        private readonly TModelProperty property;

        /// <summary>
        /// Creates a new <see cref="InputPropertyLayerSubProvider{TPropertyRetriever}"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <param name="property">The <typeparamref name="TModelProperty"/>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        internal InputPropertyLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator,
                                               TModelProperty property)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
            this.property = property;
        }

        /// <summary>
        /// Creates a new <see cref="InputPropertyLayerSubProvider{TPropertyRetriever}"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public InputPropertyLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator) : 
            this(instanceCreator, new TModelProperty()) {}

        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            if (!(parentData is InputLayerData layerData)) return false;
            object propertyData = property.Retrieve(layerData.Model);

            return propertyData != null && propertyData.Equals(sourceData);
        }

        public ILayer CreateLayer(object sourceData, object parentData) =>
            parentData is InputLayerData layerData 
                ? property.CreateLayer(instanceCreator, layerData.Model) 
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data) =>
            Enumerable.Empty<object>();
    }
}