using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.Common;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D
{
    /// <summary>
    /// <see cref="Model1DProperty"/> defines the 1D <see cref="IModelProperty"/> definitions
    /// </summary>
    internal static class Model1DProperty
    {
        /// <summary>
        /// <see cref="BoundaryConditions1D"/> implements the <see cref="IModelProperty"/>
        /// for the <see cref="IWaterFlowFMModel.BoundaryConditions1D"/>.
        /// </summary>
        internal sealed class BoundaryConditions1D : IModelProperty
        {
            public object Retrieve(IWaterFlowFMModel model) => 
                model.BoundaryConditions1D;

            public ILayer CreateLayer(IFlowFMLayerInstanceCreator creator, IWaterFlowFMModel model) =>
                creator.CreateBoundaryNodeDataLayer(model);
        }

        /// <summary>
        /// <see cref="LateralSources"/> implements the <see cref="IModelProperty"/>
        /// for the <see cref="IWaterFlowFMModel.LateralSourcesData"/>.
        /// </summary>
        internal sealed class LateralSources : IModelProperty
        {
            public object Retrieve(IWaterFlowFMModel model) =>
                model.LateralSourcesData;

            public ILayer CreateLayer(IFlowFMLayerInstanceCreator creator, IWaterFlowFMModel model) =>
                creator.CreateLateralDataLayer(model);
        }
    }
}