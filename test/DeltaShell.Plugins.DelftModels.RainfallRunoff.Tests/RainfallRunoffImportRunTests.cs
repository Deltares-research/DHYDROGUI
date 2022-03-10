using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI;
using DeltaShell.Plugins.NetCDF;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class RainfallRunoffImportRunTests
    {
        private ICompositeActivity composite;
        private RainfallRunoffModel rrModel;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            TestHelper.SetDeltaresLicenseToEnvironmentVariable();
        }

        [TearDown]
        public void TearDown()
        {
            if (rrModel != null && rrModel.ModelController != null && !string.IsNullOrEmpty(rrModel.ModelController.LastCrashReason))
            {
                Console.WriteLine("Possible crash reason: " + rrModel.ModelController.LastCrashReason);
            }
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel2()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\2\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel3()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\3\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel4()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\4\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel5()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\5\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel6()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\6\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel7()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\7\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel8()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\8\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel9()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\9\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel10()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\10\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel11()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\11\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel12()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\12\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel13()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\13\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel14()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\14\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel15()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\15\NETWORK.TP");
        }

        [Test]
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
        public void ImportRunMiniModel16()
        {
            ImportModelAndRun(@"\RRMiniTestModels\DRRSA.lit\16\NETWORK.TP");
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.WorkInProgress)]
        public void RunTholen3mm4mndPerformance()
        {
            // trigger loading netcdf beforehand.. (for timing reasons)
            var val = NetCdfFunctionStoreHelper.StartTime;

            var file = TestHelper.GetTestFilePath(@"Tholen_Performance\Tholen1.lit\1\NETWORK.TP");
            composite = RainfallRunoffIntegrationTestHelper.ImportModel(file);
            rrModel = composite.Activities.OfType<RainfallRunoffModel>().First();

            //var duration = rrModel.StopTime - rrModel.StartTime;
            //rrModel.StopTime = rrModel.StartTime.AddSeconds(duration.TotalSeconds/16.0); // 1/32th 

            // enable all output
            rrModel.OutputSettings.AggregationOption = AggregationOptions.Current;
            rrModel.OutputSettings.EngineParameters.ForEach(ep => ep.IsEnabled = true);

            ReplaceStoreForOutputCoverages(rrModel);

            // Sobek 2 is 140 secs with all output on. Sobek 3 is allowed +10%
            // Did not take CPU etc into account yet.
            TestHelper.AssertIsFasterThan(154000, () => RainfallRunoffIntegrationTestHelper.RunModel(rrModel));

            Assert.Greater(rrModel.OutputCoverages.First().Time.Values.Count, 350);
        }

        private static void ReplaceStoreForOutputCoverages(RainfallRunoffModel rrModel)
        {
            foreach (var pi in rrModel.GetAllItemsRecursive().OfType<DataItem>().
                Where(di => di.Value is FeatureCoverage && (di.Role & DataItemRole.Output) == DataItemRole.Output))
            {
                var store = new NetCdfFunctionStore();
                string tempFileName = System.IO.Path.GetTempFileName();
                store.CreateNew(tempFileName);
                store.Functions.Add((IFunction)pi.Value);
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void SendFittingMeteoPerFeatureCheckPerformance()
        {
            //simulate complexity of Tholen:

            var rrmodel = new RainfallRunoffModel();

            for (int i = 0; i < 274; i++ )
            {
                var catchment = Catchment.CreateDefault();
                catchment.Name = "catch" + i;
                rrmodel.Basin.Catchments.Add(catchment);
            }
            rrmodel.Precipitation.DataDistributionType = MeteoDataDistributionType.PerFeature;
            
            var timeArgument = rrmodel.Precipitation.Data.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();

            new TimeSeriesGenerator().GenerateTimeSeries(timeArgument,
                                                         rrmodel.StartTime,
                                                         rrmodel.StartTime.AddDays(370), new TimeSpan(0, 1, 0, 0));

            rrmodel.StopTime = rrmodel.StartTime.AddTicks(rrmodel.TimeStep.Ticks * 50); //50 timesteps
            rrmodel.ModelController = new RainfallRunoffModelController(rrmodel);
            
            TestHelper.AssertIsFasterThan(1400, rrmodel.Initialize);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void SendNonFittingMeteoPerFeatureCheckPerformance()
        {
            //simulate complexity of Tholen:

            var rrmodel = new RainfallRunoffModel();

            for (int i = 0; i < 274; i++)
            {
                var catchment = Catchment.CreateDefault();
                catchment.Name = "catch" + i;
                rrmodel.Basin.Catchments.Add(catchment);
            }
            rrmodel.Precipitation.DataDistributionType = MeteoDataDistributionType.PerFeature;

            var timeArgument = rrmodel.Precipitation.Data.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();

            new TimeSeriesGenerator().GenerateTimeSeries(timeArgument,
                                                         rrmodel.StartTime,
                                                         rrmodel.StartTime.AddDays(370), new TimeSpan(0, 0, 30, 0));

            rrmodel.StopTime = rrmodel.StartTime.AddTicks(rrmodel.TimeStep.Ticks * 20); //20 time steps
            rrmodel.ModelController = new RainfallRunoffModelController(rrmodel);
            
            TestHelper.AssertIsFasterThan(750, rrmodel.Initialize);
        }

        private void ImportModelAndRun(string importPath)
        {
            string file = RainfallRunoffIntegrationTestHelper.GetSobekImportTestDir() + importPath;
            composite = RainfallRunoffIntegrationTestHelper.ImportModel(file);
            rrModel = composite.Activities.OfType<RainfallRunoffModel>().First();
            RainfallRunoffIntegrationTestHelper.RunModel(composite);
        }
    }
}