using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FMHisFileFunctionStoreTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void OpenHisFileCheckFunctions()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));
            Assert.AreEqual(10, store.Functions.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFMOutputHisFileWithPumpAndWeirAndStationsAndGateAndGeneralStructureWhenCreatingStoreThenFunctionsCorrectlyInitialized()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\D3DFMIQ-2084.nc"));
            Assert.AreEqual(74, store.Functions.Count);
            var pumpFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "pump_structure_discharge");
            Assert.That(pumpFunction, Is.Not.Null);
            Assert.AreEqual(289, pumpFunction.GetValues().Count);
            Assert.AreEqual(289, pumpFunction.Time.Values.Count);
            Assert.AreEqual(1, pumpFunction.Arguments[1].Values.Count);

            var weirFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "weirgen_discharge");
            Assert.That(weirFunction, Is.Not.Null);
            Assert.AreEqual(289, weirFunction.GetValues().Count);
            Assert.AreEqual(289, weirFunction.Time.Values.Count);
            Assert.AreEqual(1, weirFunction.Arguments[1].Values.Count);

            var stationFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");
            Assert.That(stationFunction, Is.Not.Null);
            Assert.AreEqual(289 * 2, stationFunction.GetValues().Count);
            Assert.AreEqual(289, stationFunction.Time.Values.Count);
            Assert.AreEqual(2, stationFunction.Arguments[1].Values.Count);

            var gateFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "gategen_discharge");
            Assert.That(gateFunction, Is.Not.Null);
            Assert.AreEqual(289, gateFunction.GetValues().Count);
            Assert.AreEqual(289, gateFunction.Time.Values.Count);
            Assert.AreEqual(1, gateFunction.Arguments[1].Values.Count);

            var generalStructureFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "general_structure_discharge");
            Assert.That(generalStructureFunction, Is.Not.Null);
            Assert.AreEqual(289, generalStructureFunction.GetValues().Count);
            Assert.AreEqual(289, generalStructureFunction.Time.Values.Count);
            Assert.AreEqual(1, generalStructureFunction.Arguments[1].Values.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenNewCreatedFMModelWith2dGridAndPumpAndWeirAndStationsAndGateAndGeneralStructureAndCrossSection2DWhenModelRanThenFunctionsCorrectlyInitialized()
        {
            using (var model = new WaterFlowFMModel())
            {
                InitializeModelWith10By10Grid(model);

                InitializeModelWithStructuresUsedWithHisOutput(model);

                AddFlowBoundaryConditionsForValues(model);

                ActivityRunner.RunActivity(model);
                Assert.That(model.Status, Is.EqualTo(ActivityStatus.Cleaned), "Model should have run correctly.");

                IFMHisFileFunctionStore store = model.OutputHisFileStore;

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(89, store.Functions.Count);
                    var pumpFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "pump_s1up");
                    Assert.That(pumpFunction, Is.Not.Null);
                    Assert.AreEqual(5, pumpFunction.GetValues().Count);
                    Assert.AreEqual(5, pumpFunction.Time.Values.Count);
                    Assert.AreEqual(1, pumpFunction.Arguments[1].Values.Count);
                    Assert.That(pumpFunction[DateTime.Today.AddHours(6)], Is.EqualTo(-1.91634125682817).Within(0.000001));

                    var weirFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "weirgen_discharge");
                    Assert.That(weirFunction, Is.Not.Null);
                    Assert.AreEqual(5, weirFunction.GetValues().Count);
                    Assert.AreEqual(5, weirFunction.Time.Values.Count);
                    Assert.AreEqual(1, weirFunction.Arguments[1].Values.Count);
                    Assert.That(weirFunction[DateTime.Today.AddHours(12)], Is.EqualTo(0.0897687181942).Within(0.000001));

                    var stationFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");
                    Assert.That(stationFunction, Is.Not.Null);
                    Assert.AreEqual(5 * 2, stationFunction.GetValues().Count);
                    Assert.AreEqual(5, stationFunction.Time.Values.Count);
                    Assert.AreEqual(2, stationFunction.Arguments[1].Values.Count);
                    Assert.That(stationFunction[DateTime.Today.AddHours(6), model.Area.ObservationPoints.First()], Is.EqualTo(-1.91634124982479).Within(0.000001));
                    Assert.That(stationFunction[DateTime.Today.AddHours(12), model.Area.ObservationPoints.Last()], Is.EqualTo(219.75641079320).Within(0.000001));

                    var gateFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "gategen_s1up");
                    Assert.That(gateFunction, Is.Not.Null);
                    Assert.AreEqual(5, gateFunction.GetValues().Count);
                    Assert.AreEqual(5, gateFunction.Time.Values.Count);
                    Assert.AreEqual(1, gateFunction.Arguments[1].Values.Count);
                    Assert.That(gateFunction[DateTime.Today.AddHours(6)], Is.EqualTo(-1.9163412511480).Within(0.000001));

                    var generalStructureFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "general_structure_s1up");
                    Assert.That(generalStructureFunction, Is.Not.Null);
                    Assert.AreEqual(5, generalStructureFunction.GetValues().Count);
                    Assert.AreEqual(5, generalStructureFunction.Time.Values.Count);
                    Assert.AreEqual(1, generalStructureFunction.Arguments[1].Values.Count);
                    Assert.That(gateFunction[DateTime.Today.AddHours(6)], Is.EqualTo(-1.91634125114807).Within(0.000001));

                    var crossSection2DFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "cross_section_discharge");
                    Assert.That(crossSection2DFunction, Is.Not.Null);
                    Assert.AreEqual(5, crossSection2DFunction.GetValues().Count);
                    Assert.AreEqual(5, crossSection2DFunction.Time.Values.Count);
                    Assert.AreEqual(1, crossSection2DFunction.Arguments[1].Values.Count);
                    Assert.That(crossSection2DFunction[DateTime.Today.AddHours(12)], Is.EqualTo(0.09792273884199).Within(0.000001));
                });
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void OpenStationsWaterLevelTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));

            var waterLevelFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");

            Assert.IsNotNull(waterLevelFunction);
            Assert.AreEqual(37248, waterLevelFunction.GetValues().Count);
            Assert.AreEqual(388, waterLevelFunction.Time.Values.Count);
            Assert.AreEqual(96, waterLevelFunction.Arguments[1].Values.Count);
            Assert.AreEqual(new DateTime(1999, 12, 16), waterLevelFunction.Time.Values.First());
            Assert.AreEqual("(POR)", waterLevelFunction.Arguments[1].Values.OfType<Feature2D>().First().Name);
            Assert.AreEqual("(POR)", waterLevelFunction.Features.OfType<Feature2D>().First().Name);
            Assert.AreEqual(1.5, (double) waterLevelFunction.Components[0].Values[0], 0.001);
            Assert.AreEqual("m", waterLevelFunction.Components[0].Unit.Symbol);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void OpenStationsWaterLevelTimeSeriesCheckWithTimeFilter()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));
            var waterLevelFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");

            var timeFiltered = (IFeatureCoverage) waterLevelFunction.FilterTime(waterLevelFunction.Time.Values.First());
            Assert.AreEqual(96, timeFiltered.FeatureVariable.Values.Cast<IFeature>().ToArray().Length);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ShowWaterBalanceTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\har_fm_his.nc"));
            IFunction waterbalancetimeseries =
                store.Functions.First(f => f.Components[0].Name == "WaterBalance_total_volume");

            var expectedSeries = new[]
            {
                0.0,
                216117380.39221892,
                213569886.88264033,
                211512224.48981249,
                209755740.84053218,
                208179872.92569879,
                206707387.47398412,
                205320172.47705263,
                204001444.64842579,
                202735815.96192247,
                201511154.33547282
            };

            CollectionAssert.AreEquivalent(expectedSeries, waterbalancetimeseries.GetValues<double>().ToArray());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void OpenStationsWaterLevelTimeSeriesCheckWithStationFilter()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));
            var waterLevelFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");

            IFunction timeSeriesForPoint = waterLevelFunction.GetTimeSeries(waterLevelFunction.Features.Skip(1).First());
            Assert.AreEqual(388, timeSeriesForPoint.GetValues().Count);
            Assert.AreEqual(0.1957, (double) timeSeriesForPoint.GetValues()[50], 0.001);
            Assert.AreEqual(new DateTime(1999, 12, 16), waterLevelFunction.Time.Values.First());
            Assert.AreEqual("(POR)", waterLevelFunction.Arguments[1].Values.OfType<Feature2D>().First().Name);
            Assert.AreEqual("(POR)", waterLevelFunction.Features.OfType<Feature2D>().First().Name);
            Assert.AreEqual(1.5d, (double) waterLevelFunction.Components[0].Values[0], 0.001);
            Assert.AreEqual("m", waterLevelFunction.Components[0].Unit.Symbol);
            Assert.AreEqual(new Point(new Coordinate(502049, 4205261)), waterLevelFunction.Features.OfType<Feature2D>().First().Geometry);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void OpenCrossSectionDischargeTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));

            var dischargeFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "cross_section_discharge");

            Assert.IsNotNull(dischargeFunction);
            Assert.AreEqual(16296, dischargeFunction.GetValues().Count);
            Assert.AreEqual(388, dischargeFunction.Time.Values.Count);
            Assert.AreEqual(new DateTime(1999, 12, 16), dischargeFunction.Time.Values.First());
            Assert.AreEqual("L1", dischargeFunction.Arguments[1].Values.OfType<Feature2D>().First().Name);
            Assert.AreEqual("L1", dischargeFunction.Features.OfType<Feature2D>().First().Name);
            Assert.AreEqual(0.0d, (double) dischargeFunction.Components[0].Values[0], 0.001);
            Assert.AreEqual("m^3/s", dischargeFunction.Components[0].Unit.Symbol);
            Assert.AreEqual(
                new LineString(new[]
                {
                    new Coordinate(544991.375, 4186662.5),
                    new Coordinate(546229.875, 4184738.25)
                }),
                dischargeFunction.Features.OfType<Feature2D>().First().Geometry);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void OpenGeneralStructureTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\generalStructure_his.nc"));

            /* We use any of the components of general structure, just to check it has been created. */
            var generalStructureFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "general_structure_discharge");

            Assert.IsNotNull(generalStructureFunction);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void OpenHisFileInModelContextAndExpectFeaturesToBeSameInstance()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            string localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localMduFilePath);

            ActivityRunner.RunActivity(model);

            var waterLevelFunction = (FeatureCoverage) model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");
            Assert.IsNotNull(waterLevelFunction);
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.Arguments[1].Values.OfType<IFeature>().First(), "feature waterlevel1");
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.Features.First(), "feature waterlevel2");

            var dischargeFunction = (FeatureCoverage) model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "cross_section_discharge");
            Assert.IsNotNull(dischargeFunction);
            Assert.AreSame(model.Area.ObservationCrossSections.First(),
                           dischargeFunction.Arguments[1].Values.OfType<IFeature>().First(), "feat discharge1");
            Assert.AreSame(model.Area.ObservationCrossSections.First(),
                           dischargeFunction.Features.First(), "feat discharge2");
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void RunModelDeleteObservationPointsRunAgain()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            string localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localMduFilePath);

            ActivityRunner.RunActivity(model);

            var waterLevelFunction = (FeatureCoverage) model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");
            Assert.IsNotNull(waterLevelFunction);
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.Arguments[1].Values.OfType<IFeature>().First(), "feature waterlevel1");
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.Features.First(), "feature waterlevel2");

            for (var i = 0; i < 100; ++i)
            {
                model.Area.ObservationPoints.RemoveAt(0);
            }

            ActivityRunner.RunActivity(model);

            waterLevelFunction =
                (FeatureCoverage)
                model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");
            Assert.IsNotNull(waterLevelFunction);
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.FeatureVariable.Values.OfType<IFeature>().First(), "feature waterlevel1");
            Assert.AreSame(model.Area.ObservationPoints.First(), waterLevelFunction.Features.First());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void OpenHisFile()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            string localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localMduFilePath);

            ActivityRunner.RunActivity(model);

            GroupableFeature2DPoint observationPoint = model.Area.ObservationPoints[0];

            int numEventsBefore = TestReferenceHelper.FindEventSubscriptions(observationPoint, true);

            var waterLevelFunction = model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel") as FeatureCoverage;
            Assert.IsNotNull(waterLevelFunction);

            int numEventsAfter = TestReferenceHelper.FindEventSubscriptions(observationPoint, true);

            Assert.IsTrue(numEventsAfter <= numEventsBefore + 2);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void RunFMModelWithStructuresReadHisFile()
        {
            string mduPath = TestHelper.GetTestFilePath(@"roughness\bendprof.mdu");
            string localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel())
            {
                model.ImportFromMdu(localMduFilePath);

                var weir = new Structure()
                {
                    Name = "weir1",
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(51.0, -180.0),
                        new Coordinate(150.0, -180.0)
                    }),
                    CrestWidth = 42.0,
                    UseCrestLevelTimeSeries = true
                };
                weir.CrestLevelTimeSeries[model.StartTime] = 10.0;
                weir.CrestLevelTimeSeries[model.StartTime.AddHours(1)] = 7.5;
                weir.CrestLevelTimeSeries[model.StartTime.AddHours(2)] = 2.5;
                weir.CrestLevelTimeSeries[model.StopTime.AddSeconds(1)] = 5.5;
                model.Area.Structures.Add(weir);

                var gate = new Structure()
                {
                    Name = "weir2",
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(-149.1, -180.0),
                        new Coordinate(-50.1, -180.0)
                    }),
                    Formula = new SimpleGateFormula(true)
                    {
                        UseHorizontalGateOpeningWidthTimeSeries = true,
                        UseGateLowerEdgeLevelTimeSeries = true
                    },
                    CrestLevel = 102.0,
                    CrestWidth = 42.0
                };

                var gatedWeirFormula = gate.Formula as SimpleGateFormula;

                Assert.NotNull(gatedWeirFormula);

                gatedWeirFormula.HorizontalGateOpeningWidthTimeSeries[model.StartTime] = 0.0;
                gatedWeirFormula.HorizontalGateOpeningWidthTimeSeries[model.StartTime.AddHours(1)] = 0.0;
                gatedWeirFormula.HorizontalGateOpeningWidthTimeSeries[model.StartTime.AddHours(2)] = 25.0;
                gatedWeirFormula.HorizontalGateOpeningWidthTimeSeries[model.StopTime.AddSeconds(1)] = 25.0;

                gatedWeirFormula.GateLowerEdgeLevelTimeSeries[model.StartTime] = 8.5;
                gatedWeirFormula.GateLowerEdgeLevelTimeSeries[model.StartTime.AddHours(1)] = 6.5;
                gatedWeirFormula.GateLowerEdgeLevelTimeSeries[model.StartTime.AddHours(2)] = 0.0;
                gatedWeirFormula.GateLowerEdgeLevelTimeSeries[model.StopTime.AddSeconds(1)] = -10.0;
                model.Area.Structures.Add(gate);

                var pump = new Pump()
                {
                    Name = "pump",
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0.0, 51.5),
                        new Coordinate(0.0, 81.2)
                    }),
                    UseCapacityTimeSeries = true
                };
                pump.CapacityTimeSeries[model.StartTime] = 5.0;
                pump.CapacityTimeSeries[model.StartTime.AddHours(1)] = 20.0;
                pump.CapacityTimeSeries[model.StartTime.AddHours(2)] = 10.4;
                pump.CapacityTimeSeries[model.StopTime.AddSeconds(1)] = 0.0;
                model.Area.Pumps.Add(pump);

                ActivityRunner.RunActivity(model);

                Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                var dischargeFunction =
                    model.OutputHisFileStore.Functions.FirstOrDefault(
                            f => f.Components[0].Name == "cross_section_discharge") as
                        FeatureCoverage;
                Assert.IsNotNull(dischargeFunction);
                Assert.AreEqual(2, dischargeFunction.Arguments[1].Values.Count);
            }
        }

        [Test]
        [TestCase("general_structures")]
        [TestCase("weirgens")]
        [TestCase("gategens")]
        [TestCase("pumps")]
        [TestCase("cross_section")]
        [TestCase("stations")]
        [Category(TestCategory.Integration)]
        public void FMHisFileFunctionStore_Imports_Coverages_For_Feature(string featureName)
        {
            // 1. Set up test model.
            const string fileName = "output_hisfiles\\D3DFMIQ933.nc";
            FMHisFileFunctionStore functionStore = null;

            // 2. Set initial expectations
            Action<string> testAction = filePath => functionStore = new FMHisFileFunctionStore(filePath);

            // 3. Create function store.
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string mduFile = temporaryDirectory.CopyTestDataFileToTempDirectory(fileName);
                Assert.That(File.Exists(mduFile), Is.True, $"MduFile {fileName} could not be found.");
                Assert.DoesNotThrow(() => testAction.Invoke(mduFile));
            }

            // 4. Verify final expectations.
            Assert.That(functionStore, Is.Not.Null, "No Store Function was created with the FMHisFileFunctionStore constructor.");
            var errorMssgNoCoveragesFound = $"No FileBasedFeatureCoverage was created from file {fileName}.";
            List<FileBasedFeatureCoverage> functions = functionStore.Functions.OfType<FileBasedFeatureCoverage>().ToList();
            Assert.That(functions, Is.Not.Null, errorMssgNoCoveragesFound);
            Assert.That(functions.Any(), Is.True, errorMssgNoCoveragesFound);

            FileBasedFeatureCoverage featureFunction = functions.FirstOrDefault(f => f.FeatureVariable.Name.Equals(featureName));
            var errorMssgNoFeaturesForCoverage = $"Features for {featureName} were not loaded from his file {fileName}";
            Assert.That(featureFunction, Is.Not.Null, errorMssgNoFeaturesForCoverage);
            Assert.That(featureFunction.Features, Is.Not.Null, errorMssgNoFeaturesForCoverage);
            Assert.That(featureFunction.Features, Is.Not.Empty, errorMssgNoFeaturesForCoverage);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetVariablesCore_HisFileWithUnknownDimension_DoesNotThrowException()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                const string fileName = "output_hisfiles\\WithSourceSinkDimension_his.nc";
                string ncFilePath = tempDir.CopyTestDataFileToTempDirectory(fileName);

                var functionStore = new FMHisFileFunctionStore(ncFilePath);
                functionStore.DisableCaching = true;

                var function = Substitute.For<IVariable>();
                function.Attributes["ncName"] = "source_sink";
                function.Attributes["hasVariable"] = "false";
                var filters = new IVariableFilter[] { };

                // Call 
                IMultiDimensionalArray<IFeature> result = null;

                void Call()
                {
                    result = functionStore.GetVariableValues<IFeature>(function, filters);
                    IMultiDimensionalArrayView<IFeature> _ = result.Select(0, 0, 0); // Trigger LazyMultiDimensionalArrayBehaviour.
                }

                Assert.DoesNotThrow(Call);

                // Assert
                Assert.That(result, Has.Count.EqualTo(1));
                Assert.That(result.Rank, Is.EqualTo(1));
                Assert.That(result.Shape[0], Is.EqualTo(0));
            }
        }

        private static void AddFlowBoundaryConditionsForValues(WaterFlowFMModel model)
        {
            var feature = new Feature2D
            {
                Name = "Boundary1",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(model.GridExtent.MinX, model.GridExtent.MaxY),
                        new Coordinate(model.GridExtent.MaxX, model.GridExtent.MaxY)
                    })
            };

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge,
                                                                  BoundaryConditionDataType.TimeSeries) {Feature = feature};

            flowBoundaryCondition.AddPoint(0);
            flowBoundaryCondition.PointData[0].Arguments[0].SetValues(new[]
            {
                model.StartTime,
                model.StopTime
            });
            flowBoundaryCondition.PointData[0][model.StartTime] = -2d;
            flowBoundaryCondition.PointData[0][model.StopTime] = 6d;

            var set = new BoundaryConditionSet {Feature = feature};
            set.BoundaryConditions.Add(flowBoundaryCondition);
            model.BoundaryConditionSets.Add(set);
        }

        private static void InitializeModelWithStructuresUsedWithHisOutput(WaterFlowFMModel model)
        {
            model.Area.ObservationPoints.Add(new GroupableFeature2DPoint
            {
                Name = "ObservationPoint01",
                Geometry = new Point(model.GridExtent.MinX + 1, model.GridExtent.MinY + 5)
            });
            model.Area.ObservationPoints.Add(new GroupableFeature2DPoint
            {
                Name = "ObservationPoint02",
                Geometry = new Point(model.GridExtent.MaxX - 1, model.GridExtent.MinY + 5)
            });
            model.Area.Structures.Add(new Structure()
            {
                Name = "structure01",
                CrestWidth = 1.0d,
                Geometry = new LineString(new[]
                {
                    new Coordinate(model.GridExtent.MinX + 1, model.GridExtent.MinY + 4),
                    new Coordinate(model.GridExtent.MinX + 2, model.GridExtent.MinY + 4)
                })
            });
            model.Area.Structures.Add(new Structure()
            {
                Name = "structure02",
                CrestWidth = 2.0d,
                Geometry = new LineString(new[]
                {
                    new Coordinate(model.GridExtent.MinX + 3, model.GridExtent.MinY + 4),
                    new Coordinate(model.GridExtent.MinX + 4, model.GridExtent.MinY + 4)
                }),
                Formula = new SimpleGateFormula()
            });
            model.Area.Structures.Add(new Structure()
            {
                Name = "structure03",
                Geometry = new LineString(new[]
                {
                    new Coordinate(model.GridExtent.MinX + 5, model.GridExtent.MinY + 4),
                    new Coordinate(model.GridExtent.MinX + 6, model.GridExtent.MinY + 4)
                }),
                Formula = new GeneralStructureFormula
                {
                    GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric,
                    CrestWidth = 0.5d,
                    Upstream2Width = 0.2,
                    Downstream1Width = 1.0,
                    Upstream1Width = 1.0,
                    Downstream2Width = 1.0,
                }
            });
            model.Area.Pumps.Add(new Pump()
            {
                Name = "pump01",
                Geometry = new LineString(new[]
                {
                    new Coordinate(model.GridExtent.MinX + 6, model.GridExtent.MinY + 5),
                    new Coordinate(model.GridExtent.MinX + 7, model.GridExtent.MinY + 5)
                })
            });
            model.Area.ObservationCrossSections.Add(new ObservationCrossSection2D
            {
                Name = "ObservationCrossSection2D_1",
                Geometry = new LineString(new[]
                {
                    new Coordinate(model.GridExtent.MinX + 8, model.GridExtent.MinY + 4),
                    new Coordinate(model.GridExtent.MinX + 9, model.GridExtent.MinY + 4)
                })
            });
        }

        private static void InitializeModelWith10By10Grid(WaterFlowFMModel model)
        {
            var dtUserTimeSpan = new TimeSpan(0, 6, 0, 0, 0);
            model.TimeStep = dtUserTimeSpan;
            model.ModelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value = false;
            model.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value = new TimeSpan(0, 6, 0, 0);
            model.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value = new TimeSpan(0, 6, 0, 0);
            model.ModelDefinition.GetModelProperty(KnownProperties.DtMax).Value = 60 * 60 * 6 * 1d;

            model.ReferenceTime = DateTime.Today;
            model.StartTime = DateTime.Today;
            model.StopTime = DateTime.Today.AddDays(1);

            string tempMduFilePath = Path.Combine(FileUtils.CreateTempDirectory(), model.Name + ".mdu");
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(10, 10, 1, 1);
            grid.Vertices.ForEach(v => v.Z = -2);
            model.Grid = grid;
            model.ExportTo(tempMduFilePath);
            model.ReloadGrid();
        }
    }
}