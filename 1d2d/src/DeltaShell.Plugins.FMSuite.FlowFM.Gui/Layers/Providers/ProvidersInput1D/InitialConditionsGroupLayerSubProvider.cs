using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D
{
    /// <summary>
    /// <see cref="InitialConditionsGroupLayerSubProvider"/> provides the group layer
    /// for the different initial conditions sub layers.as well as the different
    /// child layer objects that should be part of this group layer.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal sealed class InitialConditionsGroupLayerSubProvider : ILayerSubProvider
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="InitialConditionsGroupLayerSubProvider"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public InitialConditionsGroupLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is InputFeatureGroupLayerData layerData &&
            layerData.FeatureGroupType == FeatureType.InitialConditions;

        public ILayer CreateLayer(object sourceData, object parentData) =>
            CanCreateLayerFor(sourceData, parentData) 
                ? instanceCreator.CreateInitialConditionsGroupLayer() 
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is InputFeatureGroupLayerData layerData &&
                  layerData.FeatureGroupType == FeatureType.InitialConditions))
            {
                yield break;
            }

            yield return layerData.Model.ChannelInitialConditionDefinitions;
        }
    }
}