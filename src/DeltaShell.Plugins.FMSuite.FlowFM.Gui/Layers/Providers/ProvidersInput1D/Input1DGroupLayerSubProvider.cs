using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D
{
    /// <summary>
    /// <see cref="Input1DGroupLayerSubProvider"/> provides the input group layer
    /// containing the 1D components as well as the child layer objects that should be
    /// part of it.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal sealed class Input1DGroupLayerSubProvider : ILayerSubProvider
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="Input1DGroupLayerSubProvider"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public Input1DGroupLayerSubProvider (IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is InputLayerData inputData &&
            inputData.Dimension == LayerDataDimension.Data1D &&
            inputData.Model.Equals(parentData);

        public ILayer CreateLayer(object sourceData, object parentData) =>
            CanCreateLayerFor(sourceData, parentData) 
                ? instanceCreator.Create1DGroupLayer() 
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is InputLayerData layerData && 
                  layerData.Dimension == LayerDataDimension.Data1D))
            {
                yield break;
            }

            IWaterFlowFMModel model = layerData.Model;

            if (model.GetDataItemByTag(WaterFlowFMModelDataSet.NetworkTag).LinkedTo == null)
            {
                yield return model.Network;
            }

            yield return model.NetworkDiscretization;

            yield return model.BoundaryConditions1D;
            yield return model.LateralSourcesData;

            yield return new InputFeatureGroupLayerData(model, FeatureType.InitialConditions);
            yield return new InputFeatureGroupLayerData(model, FeatureType.Friction);
        }
    }
}