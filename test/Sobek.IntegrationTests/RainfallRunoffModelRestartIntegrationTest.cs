using System;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class RainfallRunoffModelRestartIntegrationTest
    {
        [Test]
        public void CompareFullRunWithRestartedRunPaved()
        {
            var rrModel = CreateSimpleRRModelWithPavedArea();
            var samplePoint = rrModel.Basin.Catchments.First().Geometry.Coordinate;

            rrModel.WriteRestart = false;
            rrModel.StopTime = rrModel.StartTime.AddHours(24.0);

            // set meteo data
            new TimeSeriesGenerator().GenerateTimeSeries((ITimeSeries) rrModel.Precipitation.Data, rrModel.StartTime, rrModel.StopTime, rrModel.TimeStep);
            new TimeSeriesGenerator().GenerateTimeSeries((ITimeSeries) rrModel.Evaporation.Data, rrModel.StartTime, rrModel.StopTime, new TimeSpan(1, 0, 0, 0));
            rrModel.Precipitation.Data.Components[0].Values[0] = 5000.0;
            rrModel.Precipitation.Data.Components[0].Values[1] = 5000.0;
            rrModel.Precipitation.Data.Components[0].Values[2] = 5000.0;

            // do one full run (don't write restart)
            ActivityRunner.RunActivity(rrModel);
            Assert.AreEqual(ActivityStatus.Cleaned, rrModel.Status);

            // gather results
            var boundaryDischargeValuesFullRun = rrModel.BoundaryDischarge.GetTimeSeries(samplePoint).Components[0].Values.OfType<double>().ToList();

            // do one half run, and write restart
            rrModel.WriteRestart = true;
            rrModel.StopTime = rrModel.StartTime.AddHours(12.0);
            ActivityRunner.RunActivity(rrModel);
            Assert.AreEqual(ActivityStatus.Cleaned, rrModel.Status);
            var halfWayState = (FileBasedRestartState)rrModel.GetRestartOutputStates().Last().Clone();

            // restart from 2nd half of run, and compare results
            rrModel.StartTime = rrModel.StopTime;
            rrModel.StopTime = rrModel.StartTime.AddHours(12.0);
            rrModel.UseRestart = true;
            rrModel.RestartInput = halfWayState;
            ActivityRunner.RunActivity(rrModel);
            Assert.AreEqual(ActivityStatus.Cleaned, rrModel.Status);

            // assert results are the same
            var boundaryDischargeRestartSecondHalf = rrModel.BoundaryDischarge.GetTimeSeries(samplePoint).Components[0].Values.OfType<double>().Skip(1).ToList(); //skip t0
            var boundaryDischargeFullRunSecondHalf = boundaryDischargeValuesFullRun.Skip(boundaryDischargeValuesFullRun.Count - boundaryDischargeRestartSecondHalf.Count).ToArray();

            Assert.AreNotEqual(boundaryDischargeRestartSecondHalf.Count, boundaryDischargeValuesFullRun.Count, "after restart should be shorter! (less time values)");

            for (int i = 0; i < boundaryDischargeFullRunSecondHalf.Length; i++)
            {
                Assert.AreEqual(boundaryDischargeFullRunSecondHalf[i], boundaryDischargeRestartSecondHalf[i], 1E-4, i.ToString() + ": ");
            }
        }
        
        [Test]
        [Category(TestCategory.Slow)]
        public void CompareFullRunWithRestartedRunUnpaved()
        {
            var rrModel = CreateSimpleRRModelWithUnpavedArea();
            var samplePoint = rrModel.Basin.Catchments.First().Geometry.Coordinate;

            rrModel.WriteRestart = false;
            rrModel.StopTime = rrModel.StartTime.AddHours(24.0);

            // set meteo data
            SetMeteoData(rrModel);

            // do one full run (don't write restart)
            ActivityRunner.RunActivity(rrModel);
            Assert.AreEqual(ActivityStatus.Cleaned, rrModel.Status);

            // gather results
            var boundaryDischargeValuesFullRun = rrModel.BoundaryDischarge.GetTimeSeries(samplePoint).Components[0].Values.OfType<double>().ToList();

            // do one half run, and write restart
            rrModel.WriteRestart = true;
            rrModel.StopTime = rrModel.StartTime.AddHours(12.0);
            ActivityRunner.RunActivity(rrModel);
            Assert.AreEqual(ActivityStatus.Cleaned, rrModel.Status);
            var halfWayState = (FileBasedRestartState)rrModel.GetRestartOutputStates().Last().Clone();

            // restart from 2nd half of run, and compare results
            rrModel.StartTime = rrModel.StopTime;
            rrModel.StopTime = rrModel.StartTime.AddHours(12.0);
            rrModel.UseRestart = true;
            rrModel.RestartInput = halfWayState;
            ActivityRunner.RunActivity(rrModel);
            Assert.AreEqual(ActivityStatus.Cleaned, rrModel.Status);

            // assert results are the same
            var boundaryDischargeRestartSecondHalf = rrModel.BoundaryDischarge.GetTimeSeries(samplePoint).Components[0].Values.OfType<double>().Skip(1).ToList(); //skip t0
            var boundaryDischargeFullRunSecondHalf = boundaryDischargeValuesFullRun.Skip(boundaryDischargeValuesFullRun.Count - boundaryDischargeRestartSecondHalf.Count).ToArray();

            Assert.AreNotEqual(boundaryDischargeRestartSecondHalf.Count, boundaryDischargeValuesFullRun.Count, "after restart should be shorter! (less time values)");

            for (int i = 0; i < boundaryDischargeFullRunSecondHalf.Length; i++)
            {
                Assert.AreEqual(boundaryDischargeFullRunSecondHalf[i], boundaryDischargeRestartSecondHalf[i], 1E-4, i + ": ");
            }
        }

        private static void SetMeteoData(RainfallRunoffModel rrModel)
        {
            new TimeSeriesGenerator().GenerateTimeSeries((ITimeSeries) rrModel.Precipitation.Data, rrModel.StartTime,
                                                         rrModel.StopTime, rrModel.TimeStep);
            new TimeSeriesGenerator().GenerateTimeSeries((ITimeSeries) rrModel.Evaporation.Data, rrModel.StartTime,
                                                         rrModel.StopTime, new TimeSpan(1, 0, 0, 0));
            rrModel.Precipitation.Data.Components[0].Values[0] = 5000.0;
            rrModel.Precipitation.Data.Components[0].Values[1] = 5000.0;
            rrModel.Precipitation.Data.Components[0].Values[2] = 5000.0;
        }

        [Test]
        public void CheckRestartIsWrittenEachTimeStep()
        {
            var rrModel = CreateSimpleRRModelWithUnpavedArea();
            rrModel.StopTime = rrModel.StartTime.AddHours(24.0);
            
            SetMeteoData(rrModel);

            rrModel.WriteRestart = true;
            rrModel.UseSaveStateTimeRange = true;
            rrModel.SaveStateStartTime = rrModel.StartTime;
            rrModel.SaveStateTimeStep = rrModel.TimeStep;
            rrModel.SaveStateStopTime = rrModel.StartTime + rrModel.TimeStep;

            try
            {
                rrModel.Initialize();
                if (rrModel.Status == ActivityStatus.Initialized)
                {
                    rrModel.Execute();
                }
                else
                {
                    var report = rrModel.Validate();
                    var validationReport =
                        report.SubReports.FirstOrDefault(r => r.Category == "Dimr intermediate restart files");
                    Assert.That(validationReport, Is.Not.Null);
                    Assert.That(validationReport.Issues.Count(), Is.EqualTo(1));
                    Assert.That(validationReport.Issues.First().Message,
                        Is.StringContaining("Currently, Rainfall Runoff models cannot create intermediate restart files. At the moment, a single restart file may only be written for the final time-step after a complete run."));
                    return;
                }

                Assert.AreEqual(1, rrModel.GetRestartOutputStates().Count());
            }
            finally
            {
                rrModel.Cleanup();
            }
        }

        private static RainfallRunoffModel CreateSimpleRRModelWithPavedArea()
        {
            var rrModel = new RainfallRunoffModel();
            var c1 = Catchment.CreateDefault();
            c1.CatchmentType = CatchmentType.Paved;
            rrModel.Basin.Catchments.Add(c1);
            var wwtp = WasteWaterTreatmentPlant.CreateDefault();
            rrModel.Basin.WasteWaterTreatmentPlants.Add(wwtp);
            
            c1.LinkTo(wwtp);
            var pavedData = (PavedData) rrModel.GetCatchmentModelData(c1);
            pavedData.CalculationArea = 25000;
            pavedData.MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
            pavedData.CapacityMixedAndOrRainfall = 1.0;
            
            pavedData.SpillingDefinition = PavedEnums.SpillingDefinition.UseRunoffCoefficient;
            pavedData.RunoffCoefficient = 0.001; //drag it out: gives us a nice spread of boundary outflow
            
            return rrModel;
        }

        private static RainfallRunoffModel CreateSimpleRRModelWithUnpavedArea()
        {
            var builder = new HydroModelBuilder();
            var hydroModel = builder.BuildModel(ModelGroup.All);
            hydroModel.Activities.RemoveAllWhere(a => !(a is RainfallRunoffModel));
            var rrModel = (RainfallRunoffModel) hydroModel.Activities.First();

            var c1 = Catchment.CreateDefault();
            c1.CatchmentType = CatchmentType.Unpaved;
            rrModel.Basin.Catchments.Add(c1);

            var network = hydroModel.Region.SubRegions.OfType<IHydroNetwork>().First();
            var node1 = new HydroNode("node1");
            var node2 = new HydroNode("node1");
            var lateral = new LateralSource();
            var branch = new Branch {Source = node1, Target = node2, BranchFeatures = {lateral}};
            network.Branches.Add(branch);
            
            c1.LinkTo(lateral);
            var unpavedData = (UnpavedData)rrModel.GetCatchmentModelData(c1);
            unpavedData.CalculationArea = 250000;
            unpavedData.InfiltrationCapacity = 30;//30mm
            unpavedData.InfiltrationCapacityUnit = RainfallRunoffEnums.RainfallCapacityUnit.mm_hr;
            ((DeZeeuwHellingaDrainageFormula) unpavedData.DrainageFormula).InfiniteDrainageLevelRunoff = 0.8;
            return rrModel;
        }
    }
}