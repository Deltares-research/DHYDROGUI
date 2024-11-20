using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="FlowFMModelLayerSubProvider"/> is responsible for creating the
    /// <see cref="WaterFlowFMModel"/> group layer, as well as generating the
    /// appropriate child objects used to generate the child layers.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal sealed class FlowFMModelLayerSubProvider : ILayerSubProvider
    {
        private readonly IFlowFMLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="FlowFMModelLayerSubProvider"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public FlowFMModelLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is IWaterFlowFMModel;

        public ILayer CreateLayer(object sourceData, object parentData) =>
            sourceData is IWaterFlowFMModel model ? instanceCreator.CreateModelGroupLayer(model)  : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is IWaterFlowFMModel model))
            {
                yield break;
            }

            yield return new InputLayerData(model, LayerDataDimension.Data1D);
            yield return new InputLayerData(model, LayerDataDimension.Data2D);

            if (!model.OutputIsEmpty)
            {
                yield return new OutputLayerData(model);
            }
        }
    }
}