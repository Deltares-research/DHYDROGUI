using System;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.ViewModels;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Views;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.ViewModels
{
    [TestFixture]
    class HydroModelTimeSettingsUserControlTest
    {
        [Test]
        public void TestHydroModelInit()
        {
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsUserControlViewModel {Model = hydroModel};

            Assert.AreEqual(hydroModel.StartTime, viewModel.StartTime);
            Assert.AreEqual(hydroModel.StopTime, viewModel.StopTime);
            Assert.AreEqual(hydroModel.TimeStep, viewModel.TimeStep);
            Assert.AreEqual(hydroModel.OverrideStartTime, viewModel.StartTimeEnabled);
            Assert.AreEqual(hydroModel.OverrideStopTime, viewModel.StopTimeEnabled);
            Assert.AreEqual(hydroModel.OverrideTimeStep, viewModel.TimeStepEnabled);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void TestHydroModelTimeSettingsVisible()
        {
            var hydroModel = new HydroModel
            {
                StartTime = new DateTime(2017, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2017, 1, 2, 0, 0, 0),
                TimeStep = new TimeSpan(1, 0, 0),
                OverrideStartTime = false,
                OverrideStopTime = false,
                OverrideTimeStep = false
            };

            var hydroModelSettings = new HydroModelTimeSettingsUserControl { Model = hydroModel };
            WpfTestHelper.ShowModal(hydroModelSettings);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void TestTimeSettingsChanges()
        {
            var hydroModel = new HydroModel
            {
                StartTime = new DateTime(2017, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2017, 1, 2, 0, 0, 0),
                TimeStep = new TimeSpan(1, 0, 0),
                OverrideStartTime = false,
                OverrideStopTime = false,
                OverrideTimeStep = false
            };
            var viewModel = new HydroModelTimeSettingsUserControlViewModel { Model = hydroModel };

            viewModel.StartTime = new DateTime(2016, 1, 5, 3, 2, 1);
            viewModel.StopTime = new DateTime(2018, 1, 2, 3, 4, 5);
            var intervalLength = viewModel.StopTime - viewModel.StartTime;
            var duration = intervalLength.Days + " days " + intervalLength.Hours + " hours " + intervalLength.Minutes + " minutes " +
                                 intervalLength.Seconds + " seconds";

            Assert.AreEqual(hydroModel.StartTime, viewModel.StartTime);
            Assert.AreEqual(hydroModel.StopTime, viewModel.StopTime);
            Assert.AreEqual(viewModel.Duration, duration);
            Assert.AreEqual(viewModel.DurationIsValid, true);
            Assert.AreEqual(viewModel.ErrorText, "");

            viewModel.StartTimeEnabled = true;
            viewModel.StopTimeEnabled = true;
            viewModel.TimeStepEnabled = true;
            Assert.AreEqual(hydroModel.OverrideStartTime, viewModel.StartTimeEnabled);
            Assert.AreEqual(hydroModel.OverrideStopTime, viewModel.StopTimeEnabled);
            Assert.AreEqual(hydroModel.OverrideTimeStep, viewModel.TimeStepEnabled);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void TestInvalidTimeSettings()
        {
            var hydroModel = new HydroModel
            {
                StartTime = new DateTime(2017, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2017, 1, 2, 0, 0, 0),
                TimeStep = new TimeSpan(1, 0, 0),
                OverrideStartTime = false,
                OverrideStopTime = false,
                OverrideTimeStep = false
            };
            var viewModel = new HydroModelTimeSettingsUserControlViewModel { Model = hydroModel };

            // Set Start Time and Stop Time and check whether the right error message is returned
            viewModel.StartTime = new DateTime(2017, 1, 10, 0, 0, 0);
            viewModel.StopTime = new DateTime(2017, 1, 1, 0, 0, 0);
            var intervalLength = viewModel.StopTime - viewModel.StartTime;
            var duration = intervalLength.Days + " days " + intervalLength.Hours + " hours " + intervalLength.Minutes + " minutes " +
                                 intervalLength.Seconds + " seconds";

            Assert.AreEqual(hydroModel.StartTime, viewModel.StartTime);
            Assert.AreEqual(hydroModel.StopTime, viewModel.StopTime);
            Assert.AreEqual(viewModel.Duration, duration);
            Assert.AreEqual(viewModel.DurationIsValid, false);
            Assert.AreEqual(viewModel.ErrorText, "Start time must be earlier than stop time");

            // Set TimeSpan to an invalid value
            viewModel.StartTime = new DateTime(2017, 1, 1, 0, 0, 0);
            viewModel.StopTime = new DateTime(2017, 1, 2, 0, 0, 0);
            viewModel.TimeStep = new TimeSpan(-1, 0, 0);

            Assert.AreEqual(viewModel.DurationIsValid, true);
            Assert.AreEqual(viewModel.ErrorText, "Time step must be positive");

            // Also set Start Time after Stop Time and see that the message about Start Time is shown
            viewModel.StartTime = new DateTime(2017, 1, 3, 0, 0, 0);
            viewModel.StopTime = new DateTime(2017, 1, 2, 0, 0, 0);

            Assert.AreEqual(viewModel.DurationIsValid, false);
            Assert.AreEqual(viewModel.ErrorText, "Start time must be earlier than stop time");
        }
    }
}
