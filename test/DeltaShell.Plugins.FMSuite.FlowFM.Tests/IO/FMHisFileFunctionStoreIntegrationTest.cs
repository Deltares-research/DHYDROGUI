using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class FMHisFileFunctionStoreIntegrationTest
    {
        [Test]
        public void GivenFMModelWithStructures_WhenRunningTheFMModel_ThenOutputFileStoreContainsFunctionsAndFeatures()
        {
            //Given
            var mduPath = TestHelper.GetTestFilePath(@"roughness\bendprof.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);

            var gate = CreateGatedWeir(model);
            model.Area.Weirs.Add(gate);

            var pump = CreatePump(model);
            model.Area.Pumps.Add(pump);

            var weir = CreateSimpleWeir(model);
            model.Area.Weirs.Add(weir);

            //When
            ActivityRunner.RunActivity(model);

            //Then
            GateFunctionsAndFeaturesArePresent(model);
            PumpFunctionsAndFeaturesArePresent(model);
            WeirFunctionsAndFeaturesArePresent(model);
        }

        private void WeirFunctionsAndFeaturesArePresent(WaterFlowFMModel model)
        {
            var weirDischargeFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "weirgen_discharge"));
            var weirCrestLevelFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "weirgen_crest_level"));
            var weirCrestWidthFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "weirgen_crest_width"));
            var weirWaterlevelUpFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "weirgen_s1up"));
            var weirWaterLevelDownFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "weirgen_s1dn"));
            Assert.IsNotNull(weirDischargeFunction, "The outputHisFileStore does not contain the weir discharge function");
            Assert.IsNotNull(weirCrestLevelFunction, "The outputHisFileStore does not contain the weir crest level function");
            Assert.IsNotNull(weirCrestWidthFunction, "The outputHisFileStore does not contain the weir crest width function");
            Assert.IsNotNull(weirWaterlevelUpFunction, "The outputHisFileStore does not contain the weir water level up function");
            Assert.IsNotNull(weirWaterLevelDownFunction, "The outputHisFileStore does not contain the weir water level down function");

            var weirDischargeFeatures = weirDischargeFunction.Features;
            var weirCrestLevelFeatures = weirCrestLevelFunction.Features;
            var weirCrestWidthFeatures = weirCrestWidthFunction.Features;
            var weirWaterLevelUpFeatures = weirWaterlevelUpFunction.Features;
            var weirWaterLevelDownFeatures = weirWaterLevelDownFunction.Features;
            Assert.That(weirDischargeFeatures.OfType<Weir2D>().Count(), Is.EqualTo(1), "The function does not contain any weir discharge features");
            Assert.That(weirCrestLevelFeatures.OfType<Weir2D>().Count(), Is.EqualTo(1), "The function does not contain any weir crest level features");
            Assert.That(weirCrestWidthFeatures.OfType<Weir2D>().Count(), Is.EqualTo(1), "The function does not contain any weir crest width features");
            Assert.That(weirWaterLevelDownFeatures.OfType<Weir2D>().Count(), Is.EqualTo(1), "The function does not contain any weir water level up features");
            Assert.That(weirWaterLevelUpFeatures.OfType<Weir2D>().Count(), Is.EqualTo(1), "The function does not contain any weir water level down features");
        }

        [Test]
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
            Assert.AreSame(model.Area.ObservationPoints.First(), waterLevelFunction.Features.First());
        }

        [Test]
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
                           dischargeFunction.Arguments[1].Values.OfType<IFeature>().First(), "feat discharge1");
            Assert.AreSame(model.Area.ObservationCrossSections.First(),
                           dischargeFunction.Features.First(), "feat discharge2");
        }

        #region Helper methods
        private static void PumpFunctionsAndFeaturesArePresent(WaterFlowFMModel model)
        {
            var pumpDischargeFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "pump_discharge"));
            var pumpCapacityFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "pump_capacity"));
            var pumpWaterLevelUpFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "pump_s1up"));
            var pumpWaterLevelDownFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "pump_s1dn"));
            Assert.IsNotNull(pumpDischargeFunction, "The pump discharge function does not exist");
            Assert.IsNotNull(pumpCapacityFunction, "The pump capacity function does not exist");
            Assert.IsNotNull(pumpWaterLevelUpFunction, "The pump water level up function does not exist");
            Assert.IsNotNull(pumpWaterLevelDownFunction, "The pump water level down function does not exist");

            //Then
            var pumpDischargeFeatures = pumpDischargeFunction.Features;
            var pumpCapacityFeatures = pumpCapacityFunction.Features;
            var pumpWaterLevelUpFeatures = pumpWaterLevelUpFunction.Features;
            var pumpWaterLevelDownFeatures = pumpWaterLevelDownFunction.Features;
            Assert.That(pumpDischargeFeatures.OfType<Pump2D>().Count(), Is.EqualTo(1), "The function does not contain any pump discharge features");
            Assert.That(pumpCapacityFeatures.OfType<Pump2D>().Count(), Is.EqualTo(1), "The function does not contain any pump capacity features");
            Assert.That(pumpWaterLevelUpFeatures.OfType<Pump2D>().Count(), Is.EqualTo(1), "The function does not contain any pump water level up features");
            Assert.That(pumpWaterLevelDownFeatures.OfType<Pump2D>().Count(), Is.EqualTo(1), "The function does not contain any pump water level down features");
        }

        private static void GateFunctionsAndFeaturesArePresent(WaterFlowFMModel model)
        {
            var gateDischargeFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "gategen_discharge"));
            var gateFlowFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "gategen_flow_through_height"));
            var gateLowerEdgeLevelFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "gategen_lower_edge_level"));
            var gateOpeningWidthFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "gategen_opening_width"));
            var gateWaterLevelDownFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "gategen_s1dn"));
            var gateWaterLevelUpFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "gategen_s1up"));
            var gateSillLevelFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components.Any(c => c.Name == "gategen_sill_level"));
            Assert.IsNotNull(gateDischargeFunction, "The outputHisFileStore does not contain the gate discharge function");
            Assert.IsNotNull(gateFlowFunction, "The outputHisFileStore does not contain the gate capacity function");
            Assert.IsNotNull(gateLowerEdgeLevelFunction, "The outputHisFileStore does not contain the gate lower edge level function");
            Assert.IsNotNull(gateOpeningWidthFunction, "The outputHisFileStore does not contain the gate opening width function");
            Assert.IsNotNull(gateWaterLevelDownFunction, "The outputHisFileStore does not contain the gate water level down function");
            Assert.IsNotNull(gateWaterLevelUpFunction, "The outputHisFileStore does not contain the gate water level up function");
            Assert.IsNotNull(gateSillLevelFunction, "The outputHisFileStore does not contain the gate sill level function");

            var gateDischargeFeatures = gateDischargeFunction.Features;
            var gateFlowFeatures = gateFlowFunction.Features;
            var gateLowerEdgeLevelFeatures = gateLowerEdgeLevelFunction.Features;
            var gateOpeningWidthFeatures = gateOpeningWidthFunction.Features;
            var gateWaterLevelDownFeatures = gateWaterLevelDownFunction.Features;
            var gateWaterLevelUpFeatures = gateWaterLevelUpFunction.Features;
            var gateSillLevelFeatures = gateSillLevelFunction.Features;
            Assert.That(gateDischargeFeatures.OfType<Weir2D>().Count(), Is.EqualTo(1), "The function does not contain any gate discharge features");
            Assert.That(gateFlowFeatures.OfType<Weir2D>().Count(), Is.EqualTo(1), "The function does not contain any gate flow features");
            Assert.That(gateLowerEdgeLevelFeatures.OfType<Weir2D>().Count(), Is.EqualTo(1), "The function does not contain any gate lower edge level features");
            Assert.That(gateOpeningWidthFeatures.OfType<Weir2D>().Count(), Is.EqualTo(1), "The function does not contain any gate opening width features");
            Assert.That(gateWaterLevelDownFeatures.OfType<Weir2D>().Count(), Is.EqualTo(1), "The function does not contain any gate water level down features");
            Assert.That(gateWaterLevelUpFeatures.OfType<Weir2D>().Count(), Is.EqualTo(1), "The function does not contain any gate water level up features");
            Assert.That(gateSillLevelFeatures.OfType<Weir2D>().Count(), Is.EqualTo(1), "The function does not contain any gate sill level features");
        }

        private static Pump2D CreatePump(WaterFlowFMModel model)
        {
            var pump = new Pump2D("pump", true)
            {
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
            return pump;
        }

        private static Weir2D CreateGatedWeir(WaterFlowFMModel model)
        {
            var gate = new Weir2D("weir", true)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(-149.1, -180.0),
                    new Coordinate(-50.1, -180.0)
                }),
                WeirFormula = new GatedWeirFormula(true)
                {
                    UseHorizontalDoorOpeningWidthTimeSeries = true,
                    UseLowerEdgeLevelTimeSeries = true
                },
                CrestLevel = 102.0,
                CrestWidth = 42.0
            };

            var gatedWeirFormula = gate.WeirFormula as GatedWeirFormula;

            Assert.NotNull(gatedWeirFormula);

            gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries[model.StartTime] = 0.0;
            gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries[model.StartTime.AddHours(1)] = 0.0;
            gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries[model.StartTime.AddHours(2)] = 25.0;
            gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries[model.StopTime.AddSeconds(1)] = 25.0;

            gatedWeirFormula.LowerEdgeLevelTimeSeries[model.StartTime] = 8.5;
            gatedWeirFormula.LowerEdgeLevelTimeSeries[model.StartTime.AddHours(1)] = 6.5;
            gatedWeirFormula.LowerEdgeLevelTimeSeries[model.StartTime.AddHours(2)] = 0.0;
            gatedWeirFormula.LowerEdgeLevelTimeSeries[model.StopTime.AddSeconds(1)] = -10.0;

            return gate;
        }

        private static Weir2D CreateSimpleWeir(WaterFlowFMModel model)
        {
            var weir = new Weir2D("weir", true)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(51.0, -180.0),
                    new Coordinate(150.0, -180.0)
                }),
                CrestWidth = 42.0,
                UseCrestLevelTimeSeries = true,
            };
            weir.CrestLevelTimeSeries[model.StartTime] = 10.0;
            weir.CrestLevelTimeSeries[model.StartTime.AddHours(1)] = 7.5;
            weir.CrestLevelTimeSeries[model.StartTime.AddHours(2)] = 2.5;
            weir.CrestLevelTimeSeries[model.StopTime.AddSeconds(1)] = 5.5;
            return weir;
        }
        #endregion
    }
}
