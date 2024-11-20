using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.Common;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D
{
    /// <summary>
    /// <see cref="Model2DProperty"/> defines the 2D <see cref="IModelProperty"/> definitions
    /// </summary>
    internal static class Model2DProperty
    {
        /// <summary>
        /// <see cref="Boundaries"/> implements the <see cref="IModelProperty"/>
        /// for the <see cref="IWaterFlowFMModel.Boundaries"/>.
        /// </summary>
        internal sealed class Boundaries : IModelProperty
        {
            public object Retrieve(IWaterFlowFMModel model) =>
                model.Boundaries;

            public ILayer CreateLayer(IFlowFMLayerInstanceCreator creator, IWaterFlowFMModel model) =>
                creator.CreateBoundariesLayer(model);
        }

        /// <summary>
        /// <see cref="BoundaryConditionSets"/> implements the <see cref="IModelProperty"/>
        /// for the <see cref="IWaterFlowFMModel.BoundaryConditionSets"/>.
        /// </summary>
        internal sealed class BoundaryConditionSets : IModelProperty
        {
            public object Retrieve(IWaterFlowFMModel model) =>
                model.BoundaryConditionSets;

            public ILayer CreateLayer(IFlowFMLayerInstanceCreator creator, IWaterFlowFMModel model) =>
                creator.CreateBoundaryConditionSetsLayer(model);
        }

        /// <summary>
        /// <see cref="Links1D2D"/> implements the <see cref="IModelProperty"/>
        /// for the <see cref="IWaterFlowFMModel.Links"/>.
        /// </summary>
        internal sealed class Links1D2D : IModelProperty
        {
            public object Retrieve(IWaterFlowFMModel model) =>
                model.Links;

            public ILayer CreateLayer(IFlowFMLayerInstanceCreator creator, IWaterFlowFMModel model) =>
                creator.CreateLinks1D2DLayer(model);
        }

        /// <summary>
        /// <see cref="Pipes"/> implements the <see cref="IModelProperty"/>
        /// for the <see cref="IWaterFlowFMModel.Pipes"/>.
        /// </summary>
        internal sealed class Pipes : IModelProperty
        {
            public object Retrieve(IWaterFlowFMModel model) =>
                model.Pipes;

            public ILayer CreateLayer(IFlowFMLayerInstanceCreator creator, IWaterFlowFMModel model) =>
                creator.CreatePipesLayer(model);
        }
    }
}