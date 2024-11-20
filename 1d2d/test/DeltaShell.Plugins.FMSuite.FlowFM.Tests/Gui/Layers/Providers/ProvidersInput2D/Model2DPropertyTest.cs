using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.Common;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersInput2D
{
    [TestFixture]
    internal class Model2DPropertyTest
    {
        internal class BoundariesTest :
            ModelPropertyBaseTestFixture<Model2DProperty.Boundaries, 
                                         IEventedList<Feature2D>>
        {
            protected override IEventedList<Feature2D> Data { get; } =
                new EventedList<Feature2D>();

            protected override IEventedList<Feature2D> GetValue(IWaterFlowFMModel model) =>
                model.Boundaries;

            protected override ILayer GetLayer(IFlowFMLayerInstanceCreator creator, IWaterFlowFMModel model) =>
                creator.CreateBoundariesLayer(model);
        }

        internal class BoundaryConditionSetsTest :
            ModelPropertyBaseTestFixture<Model2DProperty.BoundaryConditionSets,
                                         IEventedList<BoundaryConditionSet>>
        {
            protected override IEventedList<BoundaryConditionSet> Data { get; } =
                new EventedList<BoundaryConditionSet>();

            protected override IEventedList<BoundaryConditionSet> GetValue(IWaterFlowFMModel model) =>
                model.BoundaryConditionSets;

            protected override ILayer GetLayer(IFlowFMLayerInstanceCreator creator, IWaterFlowFMModel model) =>
                creator.CreateBoundaryConditionSetsLayer(model);
        }

        internal class Links1D2DTest : 
            ModelPropertyBaseTestFixture<Model2DProperty.Links1D2D, 
                                         IEventedList<ILink1D2D>>
        {
            protected override IEventedList<ILink1D2D> Data { get; } =
                new EventedList<ILink1D2D>();

            protected override IEventedList<ILink1D2D> GetValue(IWaterFlowFMModel model) =>
                model.Links;

            protected override ILayer GetLayer(IFlowFMLayerInstanceCreator creator, IWaterFlowFMModel model) =>
                creator.CreateLinks1D2DLayer(model);
        }

        internal class PipesTest :  
            ModelPropertyBaseTestFixture<Model2DProperty.Pipes, 
                                         IEventedList<Feature2D>>
        {
            protected override IEventedList<Feature2D> Data { get; } =
                new EventedList<Feature2D>();

            protected override IEventedList<Feature2D> GetValue(IWaterFlowFMModel model) =>
                model.Pipes;

            protected override ILayer GetLayer(IFlowFMLayerInstanceCreator creator, IWaterFlowFMModel model) =>
                creator.CreatePipesLayer(model);
        }
    }
}