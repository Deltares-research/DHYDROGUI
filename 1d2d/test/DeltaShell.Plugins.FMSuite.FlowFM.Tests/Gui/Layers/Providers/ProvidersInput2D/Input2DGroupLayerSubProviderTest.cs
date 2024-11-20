using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers.Providers.ProvidersInput2D
{
    [TestFixture]
    internal class Input2DGroupLayerSubProviderTest : LayerSubProviderBaseFixture<
        Input2DGroupLayerSubProvider,
        Input2DGroupLayerSubProviderTest.CanCreateLayerForParams,
        Input2DGroupLayerSubProviderTest.CreateLayerParams,
        Input2DGroupLayerSubProviderTest.GenerateChildLayerObjectsParams
    >
    {
        public class CanCreateLayerForParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            { 
                var model = Substitute.For<IWaterFlowFMModel>();
                var otherModel = Substitute.For<IWaterFlowFMModel>();

                var inputLayerData1D = new InputLayerData(model, LayerDataDimension.Data1D);
                var inputLayerData2D = new InputLayerData(model, LayerDataDimension.Data2D);

                yield return new TestCaseData(new object(), null, false);
                yield return new TestCaseData(null, null, false);

                yield return new TestCaseData(inputLayerData1D, null, false);
                yield return new TestCaseData(inputLayerData1D, new object(), false);
                yield return new TestCaseData(inputLayerData1D, model, false);
                yield return new TestCaseData(inputLayerData1D, otherModel, false);
                
                yield return new TestCaseData(inputLayerData2D, null, false);
                yield return new TestCaseData(inputLayerData2D, new object(), false);
                yield return new TestCaseData(inputLayerData2D, otherModel, false);

                yield return new TestCaseData(inputLayerData2D, model, true);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class CreateLayerParams : IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                var model = Substitute.For<IWaterFlowFMModel>();
                var otherModel = Substitute.For<IWaterFlowFMModel>();
                var inputLayerData1D = new InputLayerData(model, LayerDataDimension.Data1D);
                var inputLayerData2D = new InputLayerData(model, LayerDataDimension.Data2D);

                var layer = Substitute.For<ILayer>();

                void ConfigureMock(IFlowFMLayerInstanceCreator instanceCreator) =>
                    instanceCreator.Create2DGroupLayer().Returns(layer);

                Action<IFlowFMLayerInstanceCreator, ILayer> assertValid =
                    CommonAsserts.CreatedLayer(layer, creator => creator.Create2DGroupLayer());

                yield return new TestCaseData(inputLayerData2D,
                                              model,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              assertValid);

                yield return new TestCaseData(null,
                                              null,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(new object(),
                                              new object(),
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(inputLayerData2D,
                                              new object(),
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(inputLayerData2D,
                                              otherModel,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
                yield return new TestCaseData(inputLayerData1D,
                                              model,
                                              (Action<IFlowFMLayerInstanceCreator>)ConfigureMock,
                                              CommonAsserts.NoLayerCreated());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        
        public class GenerateChildLayerObjectsParams : IEnumerable<TestCaseData>
        {
            public GenerateChildLayerObjectsParams()
            {
                Bathymetry = Substitute.ForPartsOf<UnstructuredGridCoverage>(); 
                InitialWaterLevel = new UnstructuredGridCellCoverage(Grid, false);
                Roughness = new UnstructuredGridFlowLinkCoverage(Grid, false);
                Viscosity= new UnstructuredGridFlowLinkCoverage(Grid, false);
                Diffusivity= new UnstructuredGridFlowLinkCoverage(Grid, false);
                Infiltration = new UnstructuredGridCellCoverage(Grid, false);
                InitialTemperature = new UnstructuredGridCellCoverage(Grid, false);

                Tracers = new EventedList<UnstructuredGridCellCoverage>(
                    Enumerable.Range(0, 10).Select(_ => new UnstructuredGridCellCoverage(Grid, false)));
                InitialFractions = new EventedList<UnstructuredGridCellCoverage>(
                    Enumerable.Range(0, 10).Select(_ => new UnstructuredGridCellCoverage(Grid, false)));
            }

            private HydroArea Area { get; } = 
                new HydroArea();
            private UnstructuredGrid Grid { get; } = 
                new UnstructuredGrid();
            private UnstructuredGridCoverage Bathymetry { get; }
            private UnstructuredGridCellCoverage InitialWaterLevel { get; }
            private IEventedList<BoundaryConditionSet> BoundaryConditionSets { get; } = 
                new EventedList<BoundaryConditionSet>();
            private IEventedList<Feature2D> Boundaries { get; } = 
                new EventedList<Feature2D>();
            private UnstructuredGridFlowLinkCoverage Roughness { get; }
            private UnstructuredGridFlowLinkCoverage Viscosity { get; } 
            private UnstructuredGridFlowLinkCoverage Diffusivity { get; } 
            private UnstructuredGridCellCoverage Infiltration { get; }
            private UnstructuredGridCellCoverage InitialTemperature { get; }
            private IEventedList<ICoverage> InitialSalinityCoverages { get; } =
                new EventedList<ICoverage>(Enumerable.Range(0, 10).Select(_ => Substitute.For<ICoverage>()));
            private IEventedList<UnstructuredGridCellCoverage> Tracers { get; }
            private IEventedList<UnstructuredGridCellCoverage> InitialFractions { get; }
            private IEventedList<Feature2D> Pipes { get; } =
                new EventedList<Feature2D>();

            private IEventedList<ILink1D2D> Links { get; } =
                new EventedList<ILink1D2D>();


            private IWaterFlowFMModel CreateMockedModel()
            {
                var model = Substitute.For<IWaterFlowFMModel>();

                model.Area = Area;
                model.Grid.Returns(Grid);

                // Set default due to problems with Returns.
                model.ReturnsForAll<UnstructuredGridCoverage>(Bathymetry);
                model.ReturnsForAll<UnstructuredGridCellCoverage>(InitialWaterLevel);
                model.ReturnsForAll<UnstructuredGridFlowLinkCoverage>(Roughness);

                model.Bathymetry.Returns(Bathymetry);
                model.InitialWaterLevel.Returns(InitialWaterLevel);

                model.BoundaryConditionSets.Returns(BoundaryConditionSets);
                model.Boundaries.Returns(Boundaries);
                model.Roughness.Returns(Roughness);
                model.Viscosity.Returns(Viscosity);
                model.Diffusivity.Returns(Diffusivity);
                model.Infiltration.Returns(Infiltration);
                model.InitialTemperature.Returns(InitialTemperature);

                var depthLayersList = new CoverageDepthLayersList(s => Substitute.For<ICoverage>());
                depthLayersList.Coverages.AddRange(InitialSalinityCoverages);
                model.InitialSalinity.Returns(depthLayersList);

                model.InitialTracers.Returns(Tracers);
                model.InitialFractions.Returns(InitialFractions);

                model.Pipes.Returns(Pipes);
                model.Links.Returns(Links);

                return model;
            }

            private (IWaterFlowFMModel model, IList<object>) GetValidInput(
                bool withInfiltration=false,
                bool withHeatFlux=false,
                bool withSalinity=false,
                bool withMorSed=false)
            {
                IWaterFlowFMModel model = CreateMockedModel();
                model.Owner.Returns(Substitute.For<IModel>());

                model.UseInfiltration.Returns(withInfiltration);
                model.HeatFluxModelType.Returns(withHeatFlux 
                                                    ? HeatFluxModelType.TransportOnly 
                                                    : HeatFluxModelType.None);
                model.UseSalinity.Returns(withSalinity);
                model.UseMorSed.Returns(withMorSed);

                var expectedChildren = new List<object>
                {
                    Area,
                    Grid,
                    Bathymetry,
                    InitialWaterLevel,
                    BoundaryConditionSets,
                    Boundaries,
                    Roughness,
                    Viscosity,
                    Diffusivity,
                    withInfiltration ? Infiltration : null,
                    withHeatFlux ? InitialTemperature : null,
                };

                if (withSalinity)
                {
                    expectedChildren.AddRange(InitialSalinityCoverages);
                }

                expectedChildren.AddRange(Tracers);

                if (withMorSed)
                {
                    expectedChildren.AddRange(InitialFractions);
                }

                expectedChildren.Add(Pipes);
                expectedChildren.Add(Links);
                expectedChildren.Add(new EstimatedSnappedFeatureGroupData(model));

                return (model, expectedChildren.Where(x => !(x is null)).ToList());
            }

            public IEnumerator<TestCaseData> GetEnumerator()
            {
                TestCaseData ToTestCase(ValueTuple<IWaterFlowFMModel, IList<object>> data) =>
                    new TestCaseData(new InputLayerData(data.Item1, LayerDataDimension.Data2D),
                                     CommonAsserts.ChildrenEqualTo(data.Item2));


                yield return ToTestCase(GetValidInput());
                yield return ToTestCase(GetValidInput(withInfiltration: true));
                yield return ToTestCase(GetValidInput(withHeatFlux: true));
                yield return ToTestCase(GetValidInput(withSalinity: true));
                yield return ToTestCase(GetValidInput(withMorSed: true));
                yield return ToTestCase(GetValidInput(
                                            withInfiltration: true,
                                            withHeatFlux: true,
                                            withSalinity:true,
                                            withMorSed:true));

                yield return new TestCaseData(null, CommonAsserts.NoChildren());
                yield return new TestCaseData(new object(), CommonAsserts.NoChildren());

                (IWaterFlowFMModel modelWithoutArea, IList<object> _) = GetValidInput();
                yield return new TestCaseData(
                    new InputLayerData(modelWithoutArea, LayerDataDimension.Data1D), 
                    CommonAsserts.NoChildren());
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        protected override Input2DGroupLayerSubProvider CreateDefault(IFlowFMLayerInstanceCreator instanceCreator) =>
            new Input2DGroupLayerSubProvider(instanceCreator);

        [Test]
        [Category(TestCategory.Integration)]
        public void AreaWithInvalidEnclosure_GenerateWarningMessage()
        {
            Input2DGroupLayerSubProvider provider = CreateDefault(instanceCreatorMock);
            using (var model = new WaterFlowFMModel())
            {
                const string featureName = "Enclosure01";
                GroupableFeature2DPolygon enclosureFeature = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                    featureName,
                    FlowFMTestHelper.GetInvalidGeometryForEnclosureExample());

                model.Area.Enclosures.Add(enclosureFeature);

                IEnumerable<HydroArea> areaChildren = Enumerable.Empty<HydroArea>(); 

                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => areaChildren = provider.GenerateChildLayerObjects(new InputLayerData(model, LayerDataDimension.Data2D)).OfType<HydroArea>().ToList(),
                    $"The drawn polygon {featureName} is not valid to create an enclosure. Verify it is not self-intersected.");

                Assert.That(areaChildren.ToList(), Has.Count.EqualTo(1));
            }
        }
    }
}