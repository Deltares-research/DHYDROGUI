using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class RainfallRunoffImportRunTests
    {
        private ICompositeActivity composite;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            TestHelper.SetDeltaresLicenseToEnvironmentVariable();
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void SendFittingMeteoPerFeatureCheckPerformance()
        {
            //simulate complexity of Tholen:

            var rrmodel = new RainfallRunoffModel();

            for (var i = 0; i < 274; i++)
            {
                var catchment = Catchment.CreateDefault();
                catchment.Name = "catch" + i;
                rrmodel.Basin.Catchments.Add(catchment);
            }

            rrmodel.Precipitation.DataDistributionType = MeteoDataDistributionType.PerFeature;

            IVariable<DateTime> timeArgument = rrmodel.Precipitation.Data.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();

            var timeSeriesGenerator = new TimeSeriesGenerator();
            timeSeriesGenerator.GenerateTimeSeries(timeArgument,
                                                   rrmodel.StartTime,
                                                   rrmodel.StartTime.AddDays(370), new TimeSpan(0, 1, 0, 0));

            rrmodel.StopTime = rrmodel.StartTime.AddTicks(rrmodel.TimeStep.Ticks * 50); //50 timesteps
            rrmodel.ModelController = new RainfallRunoffModelController(rrmodel);

            IVariable<DateTime> timeArgumentEvaporation = rrmodel.Evaporation.Data.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
            timeSeriesGenerator.GenerateTimeSeries(timeArgumentEvaporation,
                                                   rrmodel.StartTime,
                                                   rrmodel.StartTime.AddDays(370), new TimeSpan(1, 0, 0, 0));

            try
            {
                TestHelper.AssertIsFasterThan(4000, () =>
                {
                    rrmodel.Initialize(); // validates, exports and initializes model
                    Assert.AreEqual(ActivityStatus.Initialized, rrmodel.Status, "Model should be initialized");
                });
            }
            finally
            {
                rrmodel.Cleanup();
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void SendNonFittingMeteoPerFeatureCheckPerformance()
        {
            //simulate complexity of Tholen:

            var rrmodel = new RainfallRunoffModel();

            for (var i = 0; i < 274; i++)
            {
                var catchment = Catchment.CreateDefault();
                catchment.Name = "catch" + i;
                rrmodel.Basin.Catchments.Add(catchment);
            }

            rrmodel.Precipitation.DataDistributionType = MeteoDataDistributionType.PerFeature;

            IVariable<DateTime> timeArgument = rrmodel.Precipitation.Data.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();

            new TimeSeriesGenerator().GenerateTimeSeries(timeArgument,
                                                         rrmodel.StartTime,
                                                         rrmodel.StartTime.AddDays(370), new TimeSpan(0, 0, 30, 0));

            rrmodel.StopTime = rrmodel.StartTime.AddTicks(rrmodel.TimeStep.Ticks * 20); //20 time steps
            rrmodel.ModelController = new RainfallRunoffModelController(rrmodel);

            TestHelper.AssertIsFasterThan(750, rrmodel.Initialize);
        }
    }
}