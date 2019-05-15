using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class FMHisFileFunctionStoreTest
    {
        [Test]
        public void OpenHisFileCheckFunctions()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"), new WaterFlowFMModelDTO());
            Assert.AreEqual(10, store.Functions.Count);
        }

        [Test]
        public void GivenAHisFileStore_WhenOutputHisFileStoreContainsGeneralStructureFunction_ThenGeneralStructureFunctionTimeSeriesIsPresent()
        {
            //Given/When
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\generalStructure_his.nc"), new WaterFlowFMModelDTO());
            /* We use any of the components of general structure, just to check it has been created. */
            var generalStructureFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "general_structure_discharge");

            //Then
            Assert.IsNotNull(generalStructureFunction);
            Assert.AreEqual(73, generalStructureFunction.GetValues().Count);
            Assert.AreEqual(73, generalStructureFunction.Time.Values.Count);
        }

        [Test]
        public void GivenAHisFileStore_WhenOutputHisFileStoreContainsGatedWeirFunction_ThenGatedWeirFunctionTimeSeriesIsPresent()
        {
            //Given/When
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\FlowFM_his.nc"), new WaterFlowFMModelDTO());
            /* We use any of the components of gated weir, just to check it has been created. */
            var gatedWeirFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "gategen_discharge");

            //Then
            Assert.IsNotNull(gatedWeirFunction);
            Assert.AreEqual(289, gatedWeirFunction.GetValues().Count);
            Assert.AreEqual(289, gatedWeirFunction.Time.Values.Count);
        }

        [Test]
        public void GivenAHisFileStore_WhenOutputHisFileStoreContainsSimpleWeirFunction_ThenSimpleWeirFunctionTimeSeriesIsPresent3()
        {
            //Given/When
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\TestModel_his.nc"), new WaterFlowFMModelDTO());
            /* We use any of the components of simple weir, just to check it has been created. */
            var simpleWeirFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "weirgen_discharge");

            //Then
            Assert.IsNotNull(simpleWeirFunction);
            Assert.AreEqual(289, simpleWeirFunction.GetValues().Count);
            Assert.AreEqual(289, simpleWeirFunction.Time.Values.Count);
        }

        [Test]
        public void GivenAHisFileStore_WhenOutputHisFileStoreContainsPumpFunction_ThenPumpTimeSeriesIsPresent()
        {
            //Given/When
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\FlowFM_his.nc"), new WaterFlowFMModelDTO());
            /* We use any of the components of pumps, just to check it has been created. */
            var pumpFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "pump_discharge");

            //Then
            Assert.IsNotNull(pumpFunction);
            Assert.AreEqual(289, pumpFunction.GetValues().Count);
            Assert.AreEqual(289, pumpFunction.Time.Values.Count);
        }

        [Test]
        public void OpenStationsWaterLevelTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"), new WaterFlowFMModelDTO());

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
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"), new WaterFlowFMModelDTO());
            var waterLevelFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");

            var timeFiltered = (IFeatureCoverage) waterLevelFunction.FilterTime(waterLevelFunction.Time.Values.First());
            Assert.AreEqual(96, timeFiltered.FeatureVariable.Values.Cast<IFeature>().ToArray().Length);
        }

        [Test]
        public void ShowWaterBalanceTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\har_1d2d_his.nc"), new WaterFlowFMModelDTO());
            IFunction waterbalancetimeseries =
                store.Functions.First(f => f.Components[0].Name == "WaterBalance_total_volume");

            double[] expectedSeries = new[]
            {
                0.0, 216117380.39221892, 213569886.88264033, 211512224.48981249, 209755740.84053218, 208179872.92569879,
                206707387.47398412, 205320172.47705263, 204001444.64842579, 202735815.96192247, 201511154.33547282
            };

            CollectionAssert.AreEquivalent(expectedSeries, waterbalancetimeseries.GetValues<double>());
        }

        [Test]
        public void OpenStationsWaterLevelTimeSeriesCheckWithStationFilter()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"), new WaterFlowFMModelDTO());
            var waterLevelFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");

            var timeSeriesForPoint = waterLevelFunction.GetTimeSeries(waterLevelFunction.Features.Skip(1).First());
            Assert.AreEqual(388, timeSeriesForPoint.GetValues().Count);
            Assert.AreEqual(0.1957, (double) timeSeriesForPoint.GetValues()[50], 0.001);
        }

        [Test]
        public void OpenCrossSectionDischargeTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"), new WaterFlowFMModelDTO());

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
    }
}