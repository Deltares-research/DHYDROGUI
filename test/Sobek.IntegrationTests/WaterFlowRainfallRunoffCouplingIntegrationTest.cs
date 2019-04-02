using System;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class WaterFlowRainfallRunoffCouplingIntegrationTest
    {
        [Test]
        [Ignore("In Progress")]
        public void RunHollandsNoorderKwartierAP()
        {
            var path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                               @"HolandseNoorderkwartier\AP.lit\9\NETWORK.TP");

            var hydroModelImporter = new SobekHydroModelImporter(true, false);
            var hydroModel = (HydroModel)hydroModelImporter.ImportItem(path);
            var rr = hydroModel.Models.OfType<RainfallRunoffModel>().First();
            var flow = hydroModel.Models.OfType<WaterFlowModel1D>().First();

            hydroModel.OverrideTimeStep = true;
            hydroModel.TimeStep = new TimeSpan(0, 2, 0); // 5 mins
            hydroModel.OverrideStopTime = true;
            hydroModel.StopTime = hydroModel.StartTime.Add(new TimeSpan(5, 0, 0)); // 5 hours

            flow.OutputTimeStep = hydroModel.TimeStep;

            ModelTestHelper.ReplaceStoreForOutputCoverages(hydroModel);
            
            var generator = new TimeSeriesGenerator();
            generator.GenerateTimeSeries(rr.Precipitation.Data, rr.StartTime, rr.StopTime, rr.TimeStep);
            generator.GenerateTimeSeries(rr.Evaporation.Data, rr.StartTime, rr.StopTime, rr.TimeStep);

            var numValues = rr.Precipitation.Data.Components[0].Values.Count;
            rr.Precipitation.Data.Components[0].SetValues(Enumerable.Range(0, numValues).Select(i => (i + 1) * 5400.0));
            rr.OutputTimeStep = hydroModel.TimeStep;

            // print validation errors
            var issues = new RainfallRunoffModelValidator().Validate(rr)
                                                           .GetAllIssuesRecursive()
                                                           .Where(i => i.Severity == ValidationSeverity.Error).ToArray();
            issues.ForEach(i => Console.WriteLine(i.Message));
            
            if (issues.Length > 0)
                Assert.Fail("RR validation errors");

            ActivityRunner.RunActivity(hydroModel);

            Assert.AreEqual(ActivityStatus.Finished, hydroModel.Status);
        }
    }
}