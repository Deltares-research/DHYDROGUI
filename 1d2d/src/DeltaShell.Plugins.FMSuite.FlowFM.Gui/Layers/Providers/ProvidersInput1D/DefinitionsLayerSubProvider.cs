using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D
{
    /// <summary>
    /// <see cref="DefinitionsLayerSubProvider{TDefinition}"/> provides a
    /// specific definition layer given its configuration.
    /// </summary>
    /// <typeparam name="TDefinition">
    /// The type of definition for which layers are created
    /// </typeparam>
    /// <seealso cref="ILayerSubProvider"/>
    internal sealed class DefinitionsLayerSubProvider<TDefinition> :
        ILayerSubProvider 
        where TDefinition : IFeature
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;
        private readonly string layerName;
        private readonly FeatureType parentType;

        /// <summary>
        /// Creates a new <see cref="DefinitionsLayerSubProvider{TDefinition}"/> with the
        /// provided parameters.
        /// </summary>
        /// <param name="layerName">The name of the produced layers.</param>
        /// <param name="parentType">The type of features handled by this provider.</param>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="layerName"/> or <paramref name="instanceCreator"/>
        /// is <c>null</c>.
        /// </exception>
        public DefinitionsLayerSubProvider(
            string layerName,
            FeatureType parentType,
            IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            Ensure.NotNull(layerName, nameof(layerName));

            this.instanceCreator = instanceCreator;
            this.layerName = layerName;
            this.parentType = parentType;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is IEventedList<TDefinition> &&
            parentData is InputFeatureGroupLayerData parentGroupData &&
            parentGroupData.FeatureGroupType == parentType;

        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is IEventedList<TDefinition> definitions &&
            parentData is InputFeatureGroupLayerData groupLayerData &&
            groupLayerData.FeatureGroupType == parentType 
                ? instanceCreator.CreateDefinitionsLayer(
                    layerName,
                    definitions,
                    groupLayerData.Model.Network)
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data) =>
            Enumerable.Empty<object>();
    }
}