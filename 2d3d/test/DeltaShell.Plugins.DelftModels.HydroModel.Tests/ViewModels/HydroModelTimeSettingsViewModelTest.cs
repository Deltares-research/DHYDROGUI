using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.ViewModels;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Views;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.Wave;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.ViewModels
{
    [TestFixture]
    public class HydroModelTimeSettingsViewModelTest
    {
        [Test]
        public void TestHydroModelInit()
        {
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);

            Assert.AreEqual(hydroModel.StartTime, viewModel.StartTime);
            Assert.AreEqual(hydroModel.StopTime, viewModel.StopTime);
            Assert.AreEqual(hydroModel.TimeStep, viewModel.TimeStep);
            Assert.AreEqual(hydroModel.OverrideStartTime, viewModel.StartTimeSynchronisationEnabled);
            Assert.AreEqual(hydroModel.OverrideStopTime, viewModel.StopTimeSynchronisationEnabled);
            Assert.AreEqual(hydroModel.OverrideTimeStep, viewModel.TimeStepSynchronisationEnabled);
        }

        [Test]
        public void TestViewModelDefaultTimes()
        {
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);

            DateTime defaultStartDateTime = DateTime.Today;
            DateTime defaultStopDateTime = defaultStartDateTime.AddDays(1);
            var defaultTimeStep = new TimeSpan(1, 0, 0);

            Assert.AreEqual(defaultStartDateTime, viewModel.StartTime);
            Assert.AreEqual(defaultStopDateTime, viewModel.StopTime);
            Assert.AreEqual(defaultTimeStep, viewModel.TimeStep);
            Assert.AreEqual(true, viewModel.StartTimeSynchronisationEnabled);
            Assert.AreEqual(true, viewModel.StopTimeSynchronisationEnabled);
            Assert.AreEqual(true, viewModel.TimeStepSynchronisationEnabled);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Wpf)]
        public void TestHydroModelTimeSettingsVisible()
        {
            var hydroModel = new HydroModel();
            var hydroModelSettings = new HydroModelTimeSettingsView {Model = hydroModel};
            WpfTestHelper.ShowModal(hydroModelSettings);
        }

        [Test]
        public void TestStartTimeChange()
        {
            DateTime initialStartTime = DateTime.Today;
            DateTime newStartTime = initialStartTime.AddDays(1);

            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel) {StartTime = initialStartTime};

            Assert.AreEqual(initialStartTime, viewModel.StartTime);
            Assert.AreEqual(initialStartTime, hydroModel.StartTime);

            viewModel.StartTime = newStartTime;

            Assert.AreEqual(newStartTime, viewModel.StartTime);
            Assert.AreEqual(newStartTime, hydroModel.StartTime);
        }

        [Test]
        public void TestStopTimeChange()
        {
            DateTime initialStopTime = DateTime.Today;
            DateTime newStopTime = initialStopTime.AddDays(1);

            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel) {StopTime = initialStopTime};

            Assert.AreEqual(initialStopTime, viewModel.StopTime);
            Assert.AreEqual(initialStopTime, hydroModel.StopTime);

            viewModel.StopTime = newStopTime;

            Assert.AreEqual(newStopTime, viewModel.StopTime);
            Assert.AreEqual(newStopTime, hydroModel.StopTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestTimeStepChange()
        {
            var initialTimeStep = new TimeSpan(1, 1, 1);
            var newTimeStep = new TimeSpan(30, 2, 0, 0);

            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel) {TimeStep = initialTimeStep};
            Assert.AreEqual(initialTimeStep, viewModel.TimeStep);
            Assert.AreEqual(initialTimeStep, hydroModel.TimeStep);

            viewModel.TimeStep = newTimeStep;

            Assert.AreEqual(newTimeStep, viewModel.TimeStep);
            Assert.AreEqual(newTimeStep, hydroModel.TimeStep);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestDurationTextChange()
        {
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);
            DateTime newStartTime = DateTime.Now;
            DateTime newStopTime = newStartTime.AddDays(1);
            TimeSpan intervalLength = newStopTime - newStartTime;
            string duration = string.Format(Resources.HydroModelTimeSettingsViewModel_UpdateDurationLabel__0__days__1__hours__2__minutes__3__seconds, intervalLength.Days, intervalLength.Hours, intervalLength.Minutes, intervalLength.Seconds);

            viewModel.StartTime = newStartTime;
            viewModel.StopTime = newStopTime;

            Assert.AreEqual(duration, viewModel.DurationText);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestOverrideStartTimeChange()
        {
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel) {StartTimeSynchronisationEnabled = true};

            Assert.IsTrue(viewModel.StartTimeSynchronisationEnabled);
            Assert.IsTrue(hydroModel.OverrideStartTime);

            viewModel.StartTimeSynchronisationEnabled = false;
            Assert.IsFalse(viewModel.StartTimeSynchronisationEnabled);
            Assert.IsFalse(hydroModel.OverrideStartTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestOverrideStopTimeChange()
        {
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel) {StopTimeSynchronisationEnabled = true};

            Assert.IsTrue(viewModel.StopTimeSynchronisationEnabled);
            Assert.IsTrue(hydroModel.OverrideStopTime);

            viewModel.StopTimeSynchronisationEnabled = false;
            Assert.IsFalse(viewModel.StopTimeSynchronisationEnabled);
            Assert.IsFalse(hydroModel.OverrideStopTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestOverrideTimeStepChange()
        {
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel) {TimeStepSynchronisationEnabled = true};

            Assert.IsTrue(viewModel.TimeStepSynchronisationEnabled);
            Assert.IsTrue(hydroModel.OverrideTimeStep);

            viewModel.TimeStepSynchronisationEnabled = false;
            Assert.IsFalse(viewModel.TimeStepSynchronisationEnabled);
            Assert.IsFalse(hydroModel.OverrideTimeStep);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenIntegratedModelWithRtcModelAtTheEnd_WhenChangingIntegratedModelStartTime_ThenAllModelsAndViemModelsHaveSynchronized()
        {
            DateTime initialStartTime = DateTime.Now;
            DateTime stopTime = initialStartTime.AddDays(3);
            var timeStep = new TimeSpan(1, 0, 0);
            const string fmModelName = "FM Model";
            const string rtcModelName = "RTC Model";

            HydroModel hydroModel = GetHydroModelWithRTC(fmModelName, rtcModelName, initialStartTime, stopTime, timeStep);

            // Check initial FM Model
            var fmModel = hydroModel.Activities.FirstOrDefault(m => m.Name == fmModelName) as WaterFlowFMModel;
            Assert.IsNotNull(fmModel);
            Assert.That(fmModel.StartTime, Is.EqualTo(initialStartTime));

            // Check initial RTC Model
            var rtcModel = hydroModel.Activities.FirstOrDefault(m => m.Name == rtcModelName) as RealTimeControlModel;
            Assert.IsNotNull(rtcModel);
            Assert.That(rtcModel.StartTime, Is.EqualTo(initialStartTime));

            // Check initial Integrated model View Model
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);
            Assert.That(viewModel.StartTime, Is.EqualTo(initialStartTime));
            Assert.That(viewModel.Models.Count, Is.EqualTo(2));

            // Check initial FM Model View Model
            TimeDependentModelViewModel fmModelViewModel = viewModel.Models.FirstOrDefault(m => m.Name == fmModelName);
            Assert.IsNotNull(fmModelViewModel);
            Assert.That(fmModelViewModel.StartTime, Is.EqualTo(initialStartTime));

            // Check initial RTC Model View Model
            TimeDependentModelViewModel rtcModelViewModel = viewModel.Models.FirstOrDefault(m => m.Name == rtcModelName);
            Assert.IsNotNull(rtcModelViewModel);
            Assert.That(rtcModelViewModel.StartTime, Is.EqualTo(initialStartTime));

            // Change VM Start time and check for synchronisation
            DateTime newStartTime = initialStartTime.AddDays(1);
            viewModel.StartTime = newStartTime;
            Assert.That(fmModel.StartTime, Is.EqualTo(newStartTime));
            Assert.That(rtcModel.StartTime, Is.EqualTo(newStartTime));
            Assert.That(fmModelViewModel.StartTime, Is.EqualTo(newStartTime));
            Assert.That(rtcModelViewModel.StartTime, Is.EqualTo(newStartTime));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenIntegratedModelWithRtcModelAtTheEnd_WhenChangingIntegratedModelStopTime_ThenAllModelsAndViemModelsHaveSynchronized()
        {
            DateTime startTime = DateTime.Now;
            DateTime initialStopTime = startTime.AddDays(3);
            var timeStep = new TimeSpan(1, 0, 0);
            const string fmModelName = "FM Model";
            const string rtcModelName = "RTC Model";

            HydroModel hydroModel = GetHydroModelWithRTC(fmModelName, rtcModelName, startTime, initialStopTime, timeStep);

            // Check initial FM Model
            var fmModel = hydroModel.Activities.FirstOrDefault(m => m.Name == fmModelName) as WaterFlowFMModel;
            Assert.IsNotNull(fmModel);
            Assert.That(fmModel.StopTime, Is.EqualTo(initialStopTime));

            // Check initial RTC Model
            var rtcModel = hydroModel.Activities.FirstOrDefault(m => m.Name == rtcModelName) as RealTimeControlModel;
            Assert.IsNotNull(rtcModel);
            Assert.That(rtcModel.StopTime, Is.EqualTo(initialStopTime));

            // Check initial Integrated model View Model
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);
            Assert.That(viewModel.StopTime, Is.EqualTo(initialStopTime));
            Assert.That(viewModel.Models.Count, Is.EqualTo(2));

            // Check initial FM Model View Model
            TimeDependentModelViewModel fmModelViewModel = viewModel.Models.FirstOrDefault(m => m.Name == fmModelName);
            Assert.IsNotNull(fmModelViewModel);
            Assert.That(fmModelViewModel.StopTime, Is.EqualTo(initialStopTime));

            // Check initial RTC Model View Model
            TimeDependentModelViewModel rtcModelViewModel = viewModel.Models.FirstOrDefault(m => m.Name == rtcModelName);
            Assert.IsNotNull(rtcModelViewModel);
            Assert.That(rtcModelViewModel.StopTime, Is.EqualTo(initialStopTime));

            // Change VM Stop time and check for synchronisation
            DateTime newStopTime = initialStopTime.AddDays(1);
            viewModel.StopTime = newStopTime;
            Assert.That(fmModel.StopTime, Is.EqualTo(newStopTime));
            Assert.That(rtcModel.StopTime, Is.EqualTo(newStopTime));
            Assert.That(fmModelViewModel.StopTime, Is.EqualTo(newStopTime));
            Assert.That(rtcModelViewModel.StopTime, Is.EqualTo(newStopTime));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenIntegratedModelWithRtcModelAtTheEnd_WhenChangingIntegratedModelTimeStep_ThenAllModelsAndViemModelsHaveSynchronized()
        {
            DateTime startTime = DateTime.Now;
            DateTime stopTime = startTime.AddDays(3);
            var initialTimeStep = new TimeSpan(1, 0, 0);
            const string fmModelName = "FM Model";
            const string rtcModelName = "RTC Model";

            HydroModel hydroModel = GetHydroModelWithRTC(fmModelName, rtcModelName, startTime, stopTime, initialTimeStep);

            // Check initial FM Model
            var fmModel = hydroModel.Activities.FirstOrDefault(m => m.Name == fmModelName) as WaterFlowFMModel;
            Assert.IsNotNull(fmModel);
            Assert.That(fmModel.TimeStep, Is.EqualTo(initialTimeStep));

            // Check initial RTC Model
            var rtcModel = hydroModel.Activities.FirstOrDefault(m => m.Name == rtcModelName) as RealTimeControlModel;
            Assert.IsNotNull(rtcModel);
            Assert.That(rtcModel.TimeStep, Is.EqualTo(initialTimeStep));

            // Check initial Integrated model View Model
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);
            Assert.That(viewModel.TimeStep, Is.EqualTo(initialTimeStep));
            Assert.That(viewModel.Models.Count, Is.EqualTo(2));

            // Check initial FM Model View Model
            TimeDependentModelViewModel fmModelViewModel = viewModel.Models.FirstOrDefault(m => m.Name == fmModelName);
            Assert.IsNotNull(fmModelViewModel);
            Assert.That(fmModelViewModel.TimeStep, Is.EqualTo(initialTimeStep));

            // Check initial RTC Model View Model
            TimeDependentModelViewModel rtcModelViewModel = viewModel.Models.FirstOrDefault(m => m.Name == rtcModelName);
            Assert.IsNotNull(rtcModelViewModel);
            Assert.That(rtcModelViewModel.TimeStep, Is.EqualTo(initialTimeStep));

            // Change VM Stop time and check for synchronisation
            TimeSpan newTimeStep = initialTimeStep.Add(new TimeSpan(1, 30, 0));
            viewModel.TimeStep = newTimeStep;
            Assert.That(fmModel.TimeStep, Is.EqualTo(newTimeStep));
            Assert.That(rtcModel.TimeStep, Is.EqualTo(newTimeStep));
            Assert.That(fmModelViewModel.TimeStep, Is.EqualTo(newTimeStep));
            Assert.That(rtcModelViewModel.TimeStep, Is.EqualTo(newTimeStep));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestSubModelStartTimeChanged()
        {
            DateTime initialStartTime = DateTime.Now;
            DateTime initialStopTime = DateTime.Now;
            var timeStep = new TimeSpan(1, 0, 0);
            DateTime newStartTime = initialStartTime.AddYears(-3);
            var modelName1 = "FM Model";
            var modelName2 = "Wave Model";

            HydroModel hydroModel = GetTestHydroModel(modelName1, modelName2, initialStartTime, initialStopTime, timeStep);
            var fmModel = (WaterFlowFMModel) hydroModel.Activities.FirstOrDefault(m => m.Name == modelName1);
            var waveModel = (WaveModel) hydroModel.Activities.FirstOrDefault(m => m.Name == modelName2);
            if (fmModel == null || waveModel == null)
            {
                Assert.Fail();
            }

            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);

            Assert.AreEqual(initialStartTime, viewModel.StartTime);
            Assert.AreEqual(2, viewModel.Models.Count);

            TimeDependentModelViewModel fmModelViewModel = viewModel.Models.FirstOrDefault(m => m.Name == modelName1);
            if (fmModelViewModel == null)
            {
                Assert.Fail();
            }

            fmModelViewModel.StartTime = newStartTime;

            Assert.AreEqual(newStartTime, fmModel.StartTime);
            Assert.AreEqual(initialStartTime, waveModel.StartTime);
            Assert.AreEqual(newStartTime, hydroModel.StartTime);
            Assert.AreEqual(newStartTime, viewModel.StartTime);
            Assert.AreEqual(false, viewModel.StartTimeSynchronisationEnabled);
            Assert.AreEqual(false, hydroModel.OverrideStartTime);

            viewModel.StartTimeSynchronisationEnabled = true;
            Assert.AreEqual(newStartTime, fmModel.StartTime);
            Assert.AreEqual(newStartTime, waveModel.StartTime);
            Assert.AreEqual(newStartTime, hydroModel.StartTime);
            Assert.AreEqual(newStartTime, viewModel.StartTime);
            Assert.AreEqual(true, viewModel.StartTimeSynchronisationEnabled);
            Assert.AreEqual(true, hydroModel.OverrideStartTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestSubModelStopTimeChanged()
        {
            DateTime initialStartTime = DateTime.Now;
            DateTime initialStopTime = initialStartTime.AddDays(1);
            var timeStep = new TimeSpan(1, 0, 0);
            DateTime newStopTime = initialStartTime.AddYears(3);
            var modelName1 = "FM Model";
            var modelName2 = "Wave Model";

            HydroModel hydroModel = GetTestHydroModel(modelName1, modelName2, initialStartTime, initialStopTime, timeStep);
            var fmModel = (WaterFlowFMModel) hydroModel.Activities.FirstOrDefault(m => m.Name == modelName1);
            var waveModel = (WaveModel) hydroModel.Activities.FirstOrDefault(m => m.Name == modelName2);
            if (fmModel == null || waveModel == null)
            {
                Assert.Fail();
            }

            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);

            Assert.AreEqual(initialStartTime, viewModel.StartTime);
            Assert.AreEqual(2, viewModel.Models.Count);

            TimeDependentModelViewModel fmModelViewModel = viewModel.Models.FirstOrDefault(m => m.Name == modelName1);
            Assert.NotNull(fmModelViewModel);
            fmModelViewModel.StopTime = newStopTime;

            Assert.AreEqual(newStopTime, fmModel.StopTime);
            Assert.AreEqual(initialStopTime, waveModel.StopTime);
            Assert.AreEqual(newStopTime, hydroModel.StopTime);
            Assert.AreEqual(newStopTime, viewModel.StopTime);
            Assert.AreEqual(false, viewModel.StopTimeSynchronisationEnabled);
            Assert.AreEqual(false, hydroModel.OverrideStopTime);

            viewModel.StopTimeSynchronisationEnabled = true;
            Assert.AreEqual(newStopTime, fmModel.StopTime);
            Assert.AreEqual(newStopTime, waveModel.StopTime);
            Assert.AreEqual(newStopTime, hydroModel.StopTime);
            Assert.AreEqual(newStopTime, viewModel.StopTime);
            Assert.AreEqual(true, viewModel.StopTimeSynchronisationEnabled);
            Assert.AreEqual(true, hydroModel.OverrideStopTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestSubModelTimeStepChanged()
        {
            DateTime initialStartTime = DateTime.Now;
            DateTime initialStopTime = initialStartTime.AddDays(1);
            var initialTimeStep = new TimeSpan(1, 0, 0);
            var newTimeStep = new TimeSpan(0, 15, 0);
            var modelName1 = "FM Model";
            var modelName2 = "Wave Model";

            HydroModel hydroModel = GetTestHydroModel(modelName1, modelName2, initialStartTime, initialStopTime, initialTimeStep);
            var fmModel = (WaterFlowFMModel) hydroModel.Activities.FirstOrDefault(m => m.Name == modelName1);
            var waveModel = (WaveModel) hydroModel.Activities.FirstOrDefault(m => m.Name == modelName2);
            if (fmModel == null || waveModel == null)
            {
                Assert.Fail();
            }

            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);

            Assert.AreEqual(initialStartTime, viewModel.StartTime);
            Assert.AreEqual(2, viewModel.Models.Count);

            TimeDependentModelViewModel waveModelViewModel = viewModel.Models.FirstOrDefault(m => m.Name == modelName2);
            Assert.NotNull(waveModelViewModel);
            waveModelViewModel.TimeStep = newTimeStep;

            Assert.AreEqual(initialTimeStep, fmModel.TimeStep);
            Assert.AreEqual(newTimeStep, waveModel.TimeStep);
            Assert.AreEqual(newTimeStep, hydroModel.TimeStep);
            Assert.AreEqual(newTimeStep, viewModel.TimeStep);
            Assert.AreEqual(false, viewModel.TimeStepSynchronisationEnabled);
            Assert.AreEqual(false, hydroModel.OverrideTimeStep);

            viewModel.TimeStepSynchronisationEnabled = true;
            Assert.AreEqual(newTimeStep, fmModel.TimeStep);
            Assert.AreEqual(newTimeStep, waveModel.TimeStep);
            Assert.AreEqual(newTimeStep, hydroModel.TimeStep);
            Assert.AreEqual(newTimeStep, viewModel.TimeStep);
            Assert.AreEqual(true, viewModel.TimeStepSynchronisationEnabled);
            Assert.AreEqual(true, hydroModel.OverrideTimeStep);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestValidErrorText()
        {
            DateTime newStartTime = DateTime.Now;
            DateTime newStopTime = newStartTime.AddDays(1);

            // Initialization
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(new[]
            {
                typeof(INotifyPropertyChanged)
            });
            timeDependentModel.Expect(a => a.Name).Return("My Test Activity Name").Repeat.Times(1);
            timeDependentModel.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel.Expect(a => a.StartTime).Return(newStartTime).Repeat.Times(4);
            timeDependentModel.Expect(a => a.StopTime).Return(newStopTime).Repeat.Times(4);
            timeDependentModel.Expect(a => a.TimeStep).Return(new TimeSpan(1, 0, 0)).Repeat.Times(3);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

            mocks.ReplayAll();

            hydroModel.Activities.Add(timeDependentModel);
            viewModel.StartTime = newStartTime;
            viewModel.StopTime = newStopTime;

            Assert.AreEqual(false, viewModel.ErrorTexts.Any());

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestInvalidDurationErrorText()
        {
            DateTime newStartTime = DateTime.Now;
            DateTime newStopTime = newStartTime.AddDays(-1);

            // Initialization
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(new[]
            {
                typeof(INotifyPropertyChanged)
            });
            timeDependentModel.Expect(a => a.Name).Return("My Test Activity Name").Repeat.Times(1);
            timeDependentModel.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel.Expect(a => a.StartTime).Return(newStartTime).Repeat.Times(4);
            timeDependentModel.Expect(a => a.StopTime).Return(newStopTime).Repeat.Times(4);
            timeDependentModel.Expect(a => a.TimeStep).Return(new TimeSpan(1, 0, 0)).Repeat.Times(3);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

            mocks.ReplayAll();

            hydroModel.Activities.Add(timeDependentModel);
            viewModel.StartTime = newStartTime;
            viewModel.StopTime = newStopTime;

            string[] expectedErrorTexts = new[]
            {
                Resources.HydroModelTimeSettingsViewModel_DetermineErrorText_Start_time_must_be_earlier_than_stop_time
            };
            Assert.AreEqual(expectedErrorTexts, viewModel.ErrorTexts.ToList());

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestInvalidTimeStepErrorText()
        {
            // Initialization
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(new[]
            {
                typeof(INotifyPropertyChanged)
            });

            // Expectations
            timeDependentModel.Expect(a => a.Name).Return("My Test Activity Name").Repeat.Times(1);
            timeDependentModel.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel.Expect(a => a.StartTime).Return(DateTime.Now).Repeat.Times(3);
            timeDependentModel.Expect(a => a.StopTime).Return(DateTime.Now.AddDays(1)).Repeat.Times(3);
            timeDependentModel.Expect(a => a.TimeStep).Return(new TimeSpan(-1, 0, 0)).Repeat.Times(3);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

            mocks.ReplayAll();

            hydroModel.Activities.Add(timeDependentModel);
            string[] expectedErrorTexts = new[]
            {
                Resources.HydroModelTimeSettingsViewModel_DetermineErrorText_Time_step_must_be_positive
            };
            Assert.AreEqual(expectedErrorTexts, viewModel.ErrorTexts.ToList());

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestInvalidDurationAndTimeStepErrorText()
        {
            DateTime newStartTime = DateTime.Now;
            DateTime newStopTime = newStartTime.AddDays(-1);
            var newTimeStep = new TimeSpan(-1, 0, 0);

            // Initialization
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(new[]
            {
                typeof(INotifyPropertyChanged)
            });

            // Expectations
            timeDependentModel.Expect(a => a.Name).Return("My Test Activity Name").Repeat.Times(1);
            timeDependentModel.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel.Expect(a => a.StartTime).Return(newStartTime).Repeat.Times(4);
            timeDependentModel.Expect(a => a.StopTime).Return(newStopTime).Repeat.Times(4);
            timeDependentModel.Expect(a => a.TimeStep).Return(newTimeStep).Repeat.Times(3);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

            mocks.ReplayAll();

            hydroModel.Activities.Add(timeDependentModel);
            viewModel.StartTime = newStartTime;
            viewModel.StopTime = newStopTime;

            string[] expectedErrorTexts = new[]
            {
                Resources.HydroModelTimeSettingsViewModel_DetermineErrorText_Start_time_must_be_earlier_than_stop_time,
                Resources.HydroModelTimeSettingsViewModel_DetermineErrorText_Time_step_must_be_positive
            };
            Assert.AreEqual(expectedErrorTexts, viewModel.ErrorTexts.ToList());

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestRemoveModelFromHydroModel()
        {
            // Initialization
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(new[]
            {
                typeof(INotifyPropertyChanged)
            });

            // Expectations
            timeDependentModel.Expect(a => a.Name).Return("My Test Activity Name").Repeat.Times(5);
            timeDependentModel.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel.Expect(a => a.StartTime).Return(new DateTime(2000, 1, 1)).Repeat.Times(3);
            timeDependentModel.Expect(a => a.StopTime).Return(new DateTime(2000, 2, 1)).Repeat.Times(3);
            timeDependentModel.Expect(a => a.TimeStep).Return(new TimeSpan(1, 0, 0)).Repeat.Times(3);
            timeDependentModel.Expect(a => a.GetDirectChildren()).Return(new List<object>()).Repeat.Times(1);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged -= Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

            mocks.ReplayAll();

            // Actions and assertions
            hydroModel.Activities.Add(timeDependentModel);

            // Test whether the model is present in the viewmodel and in the hydromodel
            Assert.AreEqual(1, viewModel.Models.Count);
            Assert.AreEqual(timeDependentModel.Name, viewModel.Models.First().Name);

            Assert.AreEqual(1, hydroModel.Activities.Count);
            Assert.AreEqual(timeDependentModel, hydroModel.Activities.First());

            // Test whether invoking RemoveSubModel with a null value (nothing is selected in the GUI)
            // is indeed not having any effect on the viewmodel and hydromodel
            viewModel.RemoveSubmodel.Execute(null);
            Assert.AreEqual(1, viewModel.Models.Count);
            Assert.AreEqual(timeDependentModel.Name, viewModel.Models.First().Name);

            Assert.AreEqual(1, hydroModel.Activities.Count);
            Assert.AreEqual(timeDependentModel, hydroModel.Activities.First());

            //Test whether invoking RemoveSubModel is indeed removing the submodel from the hydromodel
            viewModel.RemoveSubmodel.Execute(viewModel.Models.First());
            Assert.IsFalse(viewModel.Models.Any());
            Assert.IsFalse(hydroModel.Activities.Any());

            mocks.VerifyAll();
        }

        [Test]
        public void TestRemoveModelsFromHydroModel()
        {
            var name1 = "My Test Activity Name 1";
            var name2 = "My Test Activity Name 2";
            DateTime newStartTime = DateTime.Now;
            DateTime newStopTime = newStartTime.AddMonths(1);

            // Initialization
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);

            var mocks = new MockRepository();
            var timeDependentModel1 = mocks.StrictMultiMock<ITimeDependentModel>(typeof(INotifyPropertyChanged));
            var timeDependentModel2 = mocks.StrictMultiMock<ITimeDependentModel>(typeof(INotifyPropertyChanged));

            // Expectations
            timeDependentModel1.Expect(a => a.Name).Return(name1).Repeat.Times(6);
            timeDependentModel1.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel1.Expect(a => a.StartTime).Return(newStartTime).Repeat.Times(3);
            timeDependentModel1.Expect(a => a.StopTime).Return(newStopTime).Repeat.Times(3);
            timeDependentModel1.Expect(a => a.TimeStep).Return(new TimeSpan(1, 0, 0)).Repeat.Times(3);
            timeDependentModel1.Expect(a => a.GetDirectChildren()).Return(new List<object>()).Repeat.Times(1);
            timeDependentModel1.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            timeDependentModel1.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged -= Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

            timeDependentModel2.Expect(a => a.Name).Return(name2).Repeat.Times(3);
            timeDependentModel2.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel2.Expect(a => a.StartTime).PropertyBehavior().Repeat.Times(1);
            timeDependentModel2.Expect(a => a.StopTime).PropertyBehavior().Repeat.Times(1);
            timeDependentModel2.Expect(a => a.TimeStep).PropertyBehavior().Repeat.Times(1);
            timeDependentModel2.Expect(a => a.GetDirectChildren()).Return(new List<object>()).Repeat.Times(1);
            timeDependentModel2.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            timeDependentModel2.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged -= Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

            mocks.ReplayAll();

            // Actions and assertions
            hydroModel.Activities.Add(timeDependentModel1);
            hydroModel.Activities.Add(timeDependentModel2);

            // Test whether the model is present in the viewmodel and in the hydromodel
            Assert.AreEqual(2, viewModel.Models.Count);
            CollectionAssert.AreEquivalent(new[]
            {
                name1,
                name2
            }, viewModel.Models.Select(m => m.Name).ToList());

            // Test whether invoking RemoveSubModel with a null value (nothing is selected in the GUI)
            // is indeed not having any effect on the viewmodel and hydromodel
            viewModel.RemoveSubmodel.Execute(null);
            Assert.AreEqual(2, viewModel.Models.Count);
            CollectionAssert.AreEquivalent(new[]
            {
                name1,
                name2
            }, viewModel.Models.Select(m => m.Name).ToList());

            // Test whether invoking RemoveSubModel is indeed removing the specific submodel from the hydromodel
            TimeDependentModelViewModel submodelToRemove = viewModel.Models.FirstOrDefault(m => m.Name == name2);
            viewModel.RemoveSubmodel.Execute(submodelToRemove);
            Assert.AreEqual(1, viewModel.Models.Count);
            Assert.AreEqual(timeDependentModel1.Name, viewModel.Models.First().Name);

            // Test whether invoking RemoveSubModel is indeed removing the remaining submodel from the hydromodel
            submodelToRemove = viewModel.Models.FirstOrDefault(m => m.Name == name1);
            viewModel.RemoveSubmodel.Execute(submodelToRemove);
            Assert.IsFalse(viewModel.Models.Any());
            Assert.IsFalse(hydroModel.Activities.Any());

            mocks.VerifyAll();
        }

        private HydroModel GetTestHydroModel(string fmModelName, string waveModelName, DateTime initialStartTime, DateTime initialStopTime, TimeSpan timeStep)
        {
            // Initialization
            var hydroModel = new HydroModel();
            var fmModel = new WaterFlowFMModel
            {
                Name = fmModelName,
                StartTime = initialStartTime,
                StopTime = initialStopTime,
                TimeStep = timeStep
            };
            var waveModel = new WaveModel
            {
                Name = waveModelName,
                StartTime = initialStartTime,
                StopTime = initialStopTime,
                TimeStep = timeStep
            };

            hydroModel.Activities.Add(fmModel);
            hydroModel.Activities.Add(waveModel);

            return hydroModel;
        }

        private HydroModel GetHydroModelWithRTC(string fmModelName, string rtcModelName, DateTime initialStartTime, DateTime initialStopTime, TimeSpan timeStep)
        {
            // Initialization
            var hydroModel = new HydroModel();
            var fmModel = new WaterFlowFMModel
            {
                Name = fmModelName,
                StartTime = initialStartTime,
                StopTime = initialStopTime,
                TimeStep = timeStep
            };
            var rtcModel = new RealTimeControlModel
            {
                Name = rtcModelName,
                StartTime = initialStartTime,
                StopTime = initialStopTime,
                TimeStep = timeStep
            };

            hydroModel.Activities.Add(fmModel);
            hydroModel.Activities.Add(rtcModel);

            return hydroModel;
        }
    }
}