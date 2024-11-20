using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.Common;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersInput1D
{
    [TestFixture]
    internal class Model1DPropertyTest
    { 
        [TestFixture]
        internal class BoundaryConditions1DTest : 
            ModelPropertyBaseTestFixture<Model1DProperty.BoundaryConditions1D,
                                         IEventedList<Model1DBoundaryNodeData>>
        {
            protected override IEventedList<Model1DBoundaryNodeData> Data { get; } =
                new EventedList<Model1DBoundaryNodeData>();

            protected override IEventedList<Model1DBoundaryNodeData> GetValue(IWaterFlowFMModel model) =>
                model.BoundaryConditions1D;

            protected override ILayer GetLayer(IFlowFMLayerInstanceCreator creator, 
                                               IWaterFlowFMModel model) => 
                creator.CreateBoundaryNodeDataLayer(model);
        }

        [TestFixture]
        internal class LateralSourcesTest : 
            ModelPropertyBaseTestFixture<Model1DProperty.LateralSources,
                                         IEventedList<Model1DLateralSourceData>>
        {
            protected override IEventedList<Model1DLateralSourceData> Data { get; } =
                new EventedList<Model1DLateralSourceData>();

            protected override IEventedList<Model1DLateralSourceData> GetValue(IWaterFlowFMModel model) =>
                model.LateralSourcesData;

            protected override ILayer GetLayer(IFlowFMLayerInstanceCreator creator, 
                                               IWaterFlowFMModel model) => 
                creator.CreateLateralDataLayer(model);
        }
    }
}