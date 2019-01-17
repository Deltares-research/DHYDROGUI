using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class HydroModelPerformanceTest
    {
        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void RunRRInDWAQ_AC1()
        {
            var path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"DWAQ_AC1.lit\14\NETWORK.TP");

            var hydroModelImporter = new SobekHydroModelImporter(true, false);
            var hydroModel = (HydroModel)hydroModelImporter.ImportItem(path);

            var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();

            // fill missing(?) evap data
            new TimeSeriesGenerator().GenerateTimeSeries((ITimeSeries)rr.Evaporation.Data, rr.StartTime, rr.StopTime,
                new TimeSpan(1, 0, 0));

            //about 4800ms locally
            TestHelper.AssertIsFasterThan(10000, () =>
            {
                // run rr model
                ActivityRunner.RunActivity(rr);

                if (rr.Status == ActivityStatus.Failed)
                {
                    throw new InvalidOperationException("Execute failed");
                }
            });
        }
    }
}