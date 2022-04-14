using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class FMHisFileFunctionStoreTest
    {
        [OneTimeSetUp]
        public void SetMapCoordinateSystemFactory()
        {
            if (Map.CoordinateSystemFactory == null)
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }

        [Test]
        public void OpenHisFileCheckFunctions()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));
            Assert.AreEqual(10, store.Functions.Count);
        }

        [Test]
        public void OpenStationsWaterLevelTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));

            var waterLevelFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");

            Assert.IsNotNull(waterLevelFunction);
            Assert.AreEqual(37248, waterLevelFunction.GetValues().Count);
            Assert.AreEqual(388, waterLevelFunction.Time.Values.Count);
            Assert.AreEqual(96, waterLevelFunction.Arguments[1].Values.Count);
            Assert.AreEqual(new DateTime(1999, 12, 16), waterLevelFunction.Time.Values.First());
            Assert.AreEqual("(POR)", waterLevelFunction.Arguments[1].Values.OfType<Feature2D>().First().Name);
            Assert.AreEqual("(POR)", waterLevelFunction.Features.OfType<Feature2D>().First().Name);
            Assert.AreEqual(1.5, (double)waterLevelFunction.Components[0].Values[0], 0.001);
            Assert.AreEqual("m", waterLevelFunction.Components[0].Unit.Symbol);

        }

        [Test]
        public void OpenStationsWaterLevelTimeSeriesCheckWithTimeFilter()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));
            var waterLevelFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");

            var timeFiltered = (IFeatureCoverage) waterLevelFunction.FilterTime(waterLevelFunction.Time.Values.First());
            Assert.AreEqual(96, timeFiltered.FeatureVariable.Values.Cast<IFeature>().ToArray().Length);
        }

        [Test]
        public void ShowWaterBalanceTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\har_1d2d_his.nc"));
            IFunction waterbalancetimeseries =
                store.Functions.First(f => f.Components[0].Name == "WaterBalance_total_volume");

            double[] expectedSeries = new[]
            {
                0.0, 216117380.39221892, 213569886.88264033, 211512224.48981249, 209755740.84053218, 208179872.92569879,
                206707387.47398412, 205320172.47705263, 204001444.64842579, 202735815.96192247, 201511154.33547282
            };

            CollectionAssert.AreEquivalent(expectedSeries, waterbalancetimeseries.GetValues<double>().ToArray());
        }

        [Test]
        public void OpenStationsWaterLevelTimeSeriesCheckWithStationFilter()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));
            var waterLevelFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");

            var timeSeriesForPoint = waterLevelFunction.GetTimeSeries(waterLevelFunction.Features.Skip(1).First());
            Assert.AreEqual(388, timeSeriesForPoint.GetValues().Count);
            Assert.AreEqual(0.1957, (double) timeSeriesForPoint.GetValues()[50], 0.001);
        }
        
        [Test]
        public void OpenCrossSectionDischargeTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));

            var dischargeFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "cross_section_discharge");

            Assert.IsNotNull(dischargeFunction);
            Assert.AreEqual(16296, dischargeFunction.GetValues().Count);
            Assert.AreEqual(388, dischargeFunction.Time.Values.Count);
            Assert.AreEqual(new DateTime(1999, 12, 16), dischargeFunction.Time.Values.First());
            Assert.AreEqual("L1", dischargeFunction.Arguments[1].Values.OfType<Feature2D>().First().Name);
            Assert.AreEqual("L1", dischargeFunction.Features.OfType<Feature2D>().First().Name);
            Assert.AreEqual(0.0d, (double) dischargeFunction.Components[0].Values[0], 0.001);
            Assert.AreEqual("m^3/s", dischargeFunction.Components[0].Unit.Symbol);
            Assert.AreEqual(
                new LineString(new []
                    {
                        new Coordinate(544991.375, 4186662.5),
                        new Coordinate(546229.875, 4184738.25)
                    }),
                dischargeFunction.Features.OfType<Feature2D>().First().Geometry);
        }


        [Test]
        public void OpenGeneralStructureTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\generalStructure_his.nc"));

            /* We use any of the components of general structure, just to check it has been created. */
            var generalStructureFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "general_structure_discharge");

            Assert.IsNotNull(generalStructureFunction);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void OpenHisFileInModelContextAndExpectFeaturesToBeSameInstance()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(localMduFilePath);

            ActivityRunner.RunActivity(model);

            var waterLevelFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");
            Assert.IsNotNull(waterLevelFunction);
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.Arguments[1].Values.OfType<IFeature>().First(), "feature waterlevel1");
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.Features.First(), "feature waterlevel2");

            var dischargeFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "cross_section_discharge");
            Assert.IsNotNull(dischargeFunction);
            Assert.AreSame(model.Area.ObservationCrossSections.First(),
                           dischargeFunction.Arguments[1].Values.OfType<IFeature>().First(),"feat discharge1");
            Assert.AreSame(model.Area.ObservationCrossSections.First(),
                           dischargeFunction.Features.First(), "feat discharge2");
        }


        [Test]
        public void OpenLeveeBreachTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\leveeBreach_his.nc"));

            /*
             waar staat dit!!! file moet corrupt zijn, coordinates attributes is niet gezet
            var result = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_discharge");
            Assert.IsNotNull(result, "dambreak_discharge");

            result = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_cumulative_discharge");
            Assert.IsNotNull(result, "dambreak_cumulative_discharge");
            */
            var result = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_s1up");
            Assert.IsNotNull(result, "dambreak_s1up");

            result = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_s1dn");
            Assert.IsNotNull(result, "dambreak_s1dn");

            result = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_breach_depth");
            Assert.IsNotNull(result, "dambreak_breach_depth");

            result = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_breach_width");
            Assert.IsNotNull(result, "dambreak_breach_width");
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void OpenLeveeBreachHisFileInModelContextAndExpectFeaturesToBeSameInstance()
        {
            var mduPath = TestHelper.GetTestFilePath(@"bommelerwaard\testcrop_breach_2.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(localMduFilePath);

            ActivityRunner.RunActivity(model);
            Assert.That(model.Status, Is.EqualTo(ActivityStatus.Cleaned));
            var leveeBrachDepthFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_breach_depth");
            Assert.IsNotNull(leveeBrachDepthFunction);
            Assert.AreSame(model.Area.LeveeBreaches.First(),
                           leveeBrachDepthFunction.Arguments[1].Values.OfType<IFeature>().First(), "dambreak_breach_depth");
            Assert.AreSame(model.Area.LeveeBreaches.First(),
                           leveeBrachDepthFunction.Features.First(), "dambreak_breach_depth");

         }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void RunModelDeleteObservationPointsRunAgain()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(localMduFilePath);

            ActivityRunner.RunActivity(model);

            var waterLevelFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");
            Assert.IsNotNull(waterLevelFunction);
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.Arguments[1].Values.OfType<IFeature>().First(), "feature waterlevel1");
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.Features.First(), "feature waterlevel2");

            for (int i = 0; i < 100; ++i)
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
            Assert.AreSame(model.Area.ObservationPoints.First(),waterLevelFunction.Features.First());
        }


        [Test]
        [Category(TestCategory.Slow)]
        public void OpenHisFile()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(localMduFilePath);

            ActivityRunner.RunActivity(model);

            var observationPoint = model.Area.ObservationPoints[0];

            var numEventsBefore = TestReferenceHelper.FindEventSubscriptions(observationPoint, true);

            var waterLevelFunction = model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel") as FeatureCoverage;
            Assert.IsNotNull(waterLevelFunction);

            for (var i = 0; i < 5; ++i)
            {
                var timeSeries = waterLevelFunction.Arguments[1].GetValues<IFeature>().ToList();
            }

            var numEventsAfter = TestReferenceHelper.FindEventSubscriptions(observationPoint, true);

            Assert.IsTrue(numEventsAfter <= numEventsBefore + 2);
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void RunFMModelWithStructuresReadHisFile()
        {
            var mduPath = TestHelper.GetTestFilePath(@"roughness\bendprof.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(localMduFilePath);

            var weir = new Weir2D("weir", true)
            {
                Geometry = new LineString(new[] {new Coordinate(51.0, -180.0), new Coordinate(150.0, -180.0)}),
                CrestWidth = 0.0,
                UseCrestLevelTimeSeries = true
            };
            weir.CrestLevelTimeSeries[model.StartTime] = 10.0;
            weir.CrestLevelTimeSeries[model.StartTime.AddHours(1)] = 7.5;
            weir.CrestLevelTimeSeries[model.StartTime.AddHours(2)] = 2.5;
            weir.CrestLevelTimeSeries[model.StopTime.AddSeconds(1)] = 5.5;
            model.Area.Weirs.Add(weir);

            var gate = new Gate2D("gate")
            {
                Geometry = new LineString(new[] {new Coordinate(-149.1, -180.0), new Coordinate(-50.1, -180.0)}),
                SillWidth = 102.0,
                UseOpeningWidthTimeSeries = true,
                UseLowerEdgeLevelTimeSeries = true
            };
            gate.OpeningWidthTimeSeries[model.StartTime] = 0.0;
            gate.OpeningWidthTimeSeries[model.StartTime.AddHours(1)] = 0.0;
            gate.OpeningWidthTimeSeries[model.StartTime.AddHours(2)] = 25.0;
            gate.OpeningWidthTimeSeries[model.StopTime.AddSeconds(1)] = 25.0;

            gate.LowerEdgeLevelTimeSeries[model.StartTime] = 8.5;
            gate.LowerEdgeLevelTimeSeries[model.StartTime.AddHours(1)] = 6.5;
            gate.LowerEdgeLevelTimeSeries[model.StartTime.AddHours(2)] = 0.0;
            gate.LowerEdgeLevelTimeSeries[model.StopTime.AddSeconds(1)] = -10.0;
            model.Area.Gates.Add(gate);

            var pump = new Pump2D("pump", true)
            {
                Geometry = new LineString(new[] {new Coordinate(0.0, 51.5), new Coordinate(0.0, 81.2)}),
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
                model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "cross_section_discharge") as
                    FeatureCoverage;
            Assert.IsNotNull(dischargeFunction);
            Assert.AreEqual(2, dischargeFunction.Arguments[1].Values.Count);

            // TODO: check structure output, once we support it.
        }
    }
}