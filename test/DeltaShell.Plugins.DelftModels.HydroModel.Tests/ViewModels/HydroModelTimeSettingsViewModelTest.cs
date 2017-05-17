using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.ViewModels;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Views;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM;
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
            Assert.AreEqual(hydroModel.OverrideStartTime, viewModel.StartTimeEnabled);
            Assert.AreEqual(hydroModel.OverrideStopTime, viewModel.StopTimeEnabled);
            Assert.AreEqual(hydroModel.OverrideTimeStep, viewModel.TimeStepEnabled);
        }

        [Test]
        public void TestViewModelDefaultTimes()
        {
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);

            var defaultStartDateTime = DateTime.Today;
            var defaultStopDateTime = defaultStartDateTime.AddDays(1);
            var defaultTimeStep = new TimeSpan(1, 0, 0);

            Assert.AreEqual(defaultStartDateTime, viewModel.StartTime);
            Assert.AreEqual(defaultStopDateTime, viewModel.StopTime);
            Assert.AreEqual(defaultTimeStep, viewModel.TimeStep);
            Assert.AreEqual(true, viewModel.StartTimeEnabled);
            Assert.AreEqual(true, viewModel.StopTimeEnabled);
            Assert.AreEqual(true, viewModel.TimeStepEnabled);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void TestHydroModelTimeSettingsVisible()
        {
            var hydroModel = new HydroModel();
            var hydroModelSettings = new HydroModelTimeSettingsView { Model = hydroModel };
            WpfTestHelper.ShowModal(hydroModelSettings);
        }

        [Test]
        public void TestStartTimeChange()
        {
            var initialStartTime = DateTime.Today;
            var newStartTime = initialStartTime.AddDays(1);

            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel) { StartTime = initialStartTime };

            Assert.AreEqual(initialStartTime, viewModel.StartTime);
            Assert.AreEqual(initialStartTime, hydroModel.StartTime);

            viewModel.StartTime = newStartTime;

            Assert.AreEqual(newStartTime, viewModel.StartTime);
            Assert.AreEqual(newStartTime, hydroModel.StartTime);
        }

        [Test]
        public void TestStopTimeChange()
        {
            var initialStopTime = DateTime.Today;
            var newStopTime = initialStopTime.AddDays(1);

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
            var newStartTime = DateTime.Now;
            var newStopTime = newStartTime.AddDays(1);
            var intervalLength = newStopTime - newStartTime;
            var duration = string.Format(Resources.HydroModelTimeSettingsViewModel_UpdateDurationLabel__0__days__1__hours__2__minutes__3__seconds, intervalLength.Days, intervalLength.Hours, intervalLength.Minutes, intervalLength.Seconds);

            viewModel.StartTime = newStartTime;
            viewModel.StopTime = newStopTime;

            Assert.AreEqual(duration, viewModel.DurationText);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestOverrideStartTimeChange()
        {
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel)
            {
                StartTimeEnabled = true
            };

            Assert.IsTrue(viewModel.StartTimeEnabled);
            Assert.IsTrue(hydroModel.OverrideStartTime);

            viewModel.StartTimeEnabled = false;
            Assert.IsFalse(viewModel.StartTimeEnabled);
            Assert.IsFalse(hydroModel.OverrideStartTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestOverrideStopTimeChange()
        {
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel)
            {
                StopTimeEnabled = true
            };

            Assert.IsTrue(viewModel.StopTimeEnabled);
            Assert.IsTrue(hydroModel.OverrideStopTime);

            viewModel.StopTimeEnabled = false;
            Assert.IsFalse(viewModel.StopTimeEnabled);
            Assert.IsFalse(hydroModel.OverrideStopTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestOverrideTimeStepChange()
        {
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel)
            {
                TimeStepEnabled = true
            };

            Assert.IsTrue(viewModel.TimeStepEnabled);
            Assert.IsTrue(hydroModel.OverrideTimeStep);

            viewModel.TimeStepEnabled = false;
            Assert.IsFalse(viewModel.TimeStepEnabled);
            Assert.IsFalse(hydroModel.OverrideTimeStep);
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

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestSubModelStartTimeChanged()
        {
            var initialStartTime = DateTime.Now;
            var initialStopTime = DateTime.Now;
            var timeStep = new TimeSpan(1, 0, 0);
            var newStartTime = initialStartTime.AddYears(-3);
            var modelName1 = "FM Model";
            var modelName2 = "Wave Model";

            var hydroModel = GetTestHydroModel(modelName1, modelName2, initialStartTime, initialStopTime, timeStep);
            var fmModel = (WaterFlowFMModel)hydroModel.Activities.FirstOrDefault(m => m.Name == modelName1);
            var waveModel = (WaveModel)hydroModel.Activities.FirstOrDefault(m => m.Name == modelName2);
            if(fmModel == null || waveModel == null) Assert.Fail();

            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);

            Assert.AreEqual(initialStartTime, viewModel.StartTime);
            Assert.AreEqual(2, viewModel.Models.Count);

            var fmModelViewModel = viewModel.Models.FirstOrDefault(m => m.Name == modelName1);
            if (fmModelViewModel == null) Assert.Fail();
            fmModelViewModel.StartTime = newStartTime;

            Assert.AreEqual(newStartTime, fmModel.StartTime);
            Assert.AreEqual(initialStartTime, waveModel.StartTime);
            Assert.AreEqual(newStartTime, hydroModel.StartTime);
            Assert.AreEqual(newStartTime, viewModel.StartTime);
            Assert.AreEqual(false, viewModel.StartTimeEnabled);
            Assert.AreEqual(false, hydroModel.OverrideStartTime);

            viewModel.StartTimeEnabled = true;
            Assert.AreEqual(newStartTime, fmModel.StartTime);
            Assert.AreEqual(newStartTime, waveModel.StartTime);
            Assert.AreEqual(newStartTime, hydroModel.StartTime);
            Assert.AreEqual(newStartTime, viewModel.StartTime);
            Assert.AreEqual(true, viewModel.StartTimeEnabled);
            Assert.AreEqual(true, hydroModel.OverrideStartTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestSubModelStopTimeChanged()
        {
            var initialStartTime = DateTime.Now;
            var initialStopTime = initialStartTime.AddDays(1);
            var timeStep = new TimeSpan(1, 0, 0);
            var newStopTime = initialStartTime.AddYears(3);
            var modelName1 = "FM Model";
            var modelName2 = "Wave Model";

            var hydroModel = GetTestHydroModel(modelName1, modelName2, initialStartTime, initialStopTime, timeStep);
            var fmModel = (WaterFlowFMModel)hydroModel.Activities.FirstOrDefault(m => m.Name == modelName1);
            var waveModel = (WaveModel)hydroModel.Activities.FirstOrDefault(m => m.Name == modelName2);
            if (fmModel == null || waveModel == null) Assert.Fail();

            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);

            Assert.AreEqual(initialStartTime, viewModel.StartTime);
            Assert.AreEqual(2, viewModel.Models.Count);

            var fmModelViewModel = viewModel.Models.FirstOrDefault(m => m.Name == modelName1);
            Assert.NotNull(fmModelViewModel);
            fmModelViewModel.StopTime = newStopTime;

            Assert.AreEqual(newStopTime, fmModel.StopTime);
            Assert.AreEqual(initialStopTime, waveModel.StopTime);
            Assert.AreEqual(newStopTime, hydroModel.StopTime);
            Assert.AreEqual(newStopTime, viewModel.StopTime);
            Assert.AreEqual(false, viewModel.StopTimeEnabled);
            Assert.AreEqual(false, hydroModel.OverrideStopTime);

            viewModel.StopTimeEnabled = true;
            Assert.AreEqual(newStopTime, fmModel.StopTime);
            Assert.AreEqual(newStopTime, waveModel.StopTime);
            Assert.AreEqual(newStopTime, hydroModel.StopTime);
            Assert.AreEqual(newStopTime, viewModel.StopTime);
            Assert.AreEqual(true, viewModel.StopTimeEnabled);
            Assert.AreEqual(true, hydroModel.OverrideStopTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestSubModelTimeStepChanged()
        {
            var initialStartTime = DateTime.Now;
            var initialStopTime = initialStartTime.AddDays(1);
            var initialTimeStep = new TimeSpan(1, 0, 0);
            var newTimeStep = new TimeSpan(0, 15, 0);
            var modelName1 = "FM Model";
            var modelName2 = "Wave Model";

            var hydroModel = GetTestHydroModel(modelName1, modelName2, initialStartTime, initialStopTime, initialTimeStep);
            var fmModel = (WaterFlowFMModel)hydroModel.Activities.FirstOrDefault(m => m.Name == modelName1);
            var waveModel = (WaveModel)hydroModel.Activities.FirstOrDefault(m => m.Name == modelName2);
            if (fmModel == null || waveModel == null) Assert.Fail();

            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);

            Assert.AreEqual(initialStartTime, viewModel.StartTime);
            Assert.AreEqual(2, viewModel.Models.Count);

            var waveModelViewModel = viewModel.Models.FirstOrDefault(m => m.Name == modelName2);
            Assert.NotNull(waveModelViewModel);
            waveModelViewModel.TimeStep = newTimeStep;

            Assert.AreEqual(initialTimeStep, fmModel.TimeStep);
            Assert.AreEqual(newTimeStep, waveModel.TimeStep);
            Assert.AreEqual(newTimeStep, hydroModel.TimeStep);
            Assert.AreEqual(newTimeStep, viewModel.TimeStep);
            Assert.AreEqual(false, viewModel.TimeStepEnabled);
            Assert.AreEqual(false, hydroModel.OverrideTimeStep);

            viewModel.TimeStepEnabled = true;
            Assert.AreEqual(newTimeStep, fmModel.TimeStep);
            Assert.AreEqual(newTimeStep, waveModel.TimeStep);
            Assert.AreEqual(newTimeStep, hydroModel.TimeStep);
            Assert.AreEqual(newTimeStep, viewModel.TimeStep);
            Assert.AreEqual(true, viewModel.TimeStepEnabled);
            Assert.AreEqual(true, hydroModel.OverrideTimeStep);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestValidErrorText()
        {
            var newStartTime = DateTime.Now;
            var newStopTime = newStartTime.AddDays(1);

            // Initialization
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(new[] { typeof(INotifyPropertyChanged) });
            timeDependentModel.Expect(a => a.Name).Return("My Test Activity Name").Repeat.Times(1);
            timeDependentModel.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel.Expect(a => a.StartTime).Return(newStartTime).Repeat.Times(4);
            timeDependentModel.Expect(a => a.StopTime).Return(newStopTime).Repeat.Times(4);
            timeDependentModel.Expect(a => a.TimeStep).Return(new TimeSpan(1, 0, 0)).Repeat.Times(3);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

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
            var newStartTime = DateTime.Now;
            var newStopTime = newStartTime.AddDays(-1);

            // Initialization
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(new[] { typeof(INotifyPropertyChanged) });
            timeDependentModel.Expect(a => a.Name).Return("My Test Activity Name").Repeat.Times(1);
            timeDependentModel.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel.Expect(a => a.StartTime).Return(newStartTime).Repeat.Times(4);
            timeDependentModel.Expect(a => a.StopTime).Return(newStopTime).Repeat.Times(4);
            timeDependentModel.Expect(a => a.TimeStep).Return(new TimeSpan(1, 0, 0)).Repeat.Times(3);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

            mocks.ReplayAll();

            hydroModel.Activities.Add(timeDependentModel);
            viewModel.StartTime = newStartTime;
            viewModel.StopTime = newStopTime;

            var expectedErrorTexts = new[]
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
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(new[] { typeof(INotifyPropertyChanged) });

            // Expectations
            timeDependentModel.Expect(a => a.Name).Return("My Test Activity Name").Repeat.Times(1);
            timeDependentModel.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel.Expect(a => a.StartTime).Return(DateTime.Now).Repeat.Times(3);
            timeDependentModel.Expect(a => a.StopTime).Return(DateTime.Now.AddDays(1)).Repeat.Times(3);
            timeDependentModel.Expect(a => a.TimeStep).Return(new TimeSpan(-1, 0, 0)).Repeat.Times(3);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

            mocks.ReplayAll();

            hydroModel.Activities.Add(timeDependentModel);
            var expectedErrorTexts = new[]
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
            var newStartTime = DateTime.Now;
            var newStopTime = newStartTime.AddDays(-1);
            var newTimeStep = new TimeSpan(-1, 0, 0);

            // Initialization
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(new[] { typeof(INotifyPropertyChanged) });

            // Expectations
            timeDependentModel.Expect(a => a.Name).Return("My Test Activity Name").Repeat.Times(1);
            timeDependentModel.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel.Expect(a => a.StartTime).Return(newStartTime).Repeat.Times(4);
            timeDependentModel.Expect(a => a.StopTime).Return(newStopTime).Repeat.Times(4);
            timeDependentModel.Expect(a => a.TimeStep).Return(newTimeStep).Repeat.Times(3);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

            mocks.ReplayAll();

            hydroModel.Activities.Add(timeDependentModel);
            viewModel.StartTime = newStartTime;
            viewModel.StopTime = newStopTime;

            var expectedErrorTexts = new[]
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
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(new [] {typeof(INotifyPropertyChanged)});

            // Expectations
            timeDependentModel.Expect(a => a.Name).Return("My Test Activity Name").Repeat.Times(5);
            timeDependentModel.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel.Expect(a => a.StartTime).Return(new DateTime(2000,1,1)).Repeat.Times(3);
            timeDependentModel.Expect(a => a.StopTime).Return(new DateTime(2000,2,1)).Repeat.Times(3);
            timeDependentModel.Expect(a => a.TimeStep).Return(new TimeSpan(1,0,0)).Repeat.Times(3);
            timeDependentModel.Expect(a => a.GetDirectChildren()).Return(new List<object>()).Repeat.Times(1);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged -= Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

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
            var newStartTime = DateTime.Now;
            var newStopTime = newStartTime.AddMonths(1);

            // Initialization
            var hydroModel = new HydroModel();
            var viewModel = new HydroModelTimeSettingsViewModel(hydroModel);

            var mocks = new MockRepository();
            var timeDependentModel1 = mocks.StrictMultiMock<ITimeDependentModel>(new[] { typeof(INotifyPropertyChanged) });
            var timeDependentModel2 = mocks.StrictMultiMock<ITimeDependentModel>(new[] { typeof(INotifyPropertyChanged) });

            // Expectations
            timeDependentModel1.Expect(a => a.Name).Return(name1).Repeat.Times(6);
            timeDependentModel1.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel1.Expect(a => a.StartTime).Return(newStartTime).Repeat.Times(3);
            timeDependentModel1.Expect(a => a.StopTime).Return(newStopTime).Repeat.Times(3);
            timeDependentModel1.Expect(a => a.TimeStep).Return(new TimeSpan(1, 0, 0)).Repeat.Times(3);
            timeDependentModel1.Expect(a => a.GetDirectChildren()).Return(new List<object>()).Repeat.Times(1);
            timeDependentModel1.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            timeDependentModel1.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged -= Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

            timeDependentModel2.Expect(a => a.Name).Return(name2).Repeat.Times(3);
            timeDependentModel2.Expect(a => a.Owner).PropertyBehavior().Repeat.Times(1);
            timeDependentModel2.Expect(a => a.StartTime).PropertyBehavior().Repeat.Times(1);
            timeDependentModel2.Expect(a => a.StopTime).PropertyBehavior().Repeat.Times(1);
            timeDependentModel2.Expect(a => a.TimeStep).PropertyBehavior().Repeat.Times(1);
            timeDependentModel2.Expect(a => a.GetDirectChildren()).Return(new List<object>()).Repeat.Times(1);
            timeDependentModel2.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            timeDependentModel2.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged -= Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);

            mocks.ReplayAll();

            // Actions and assertions
            hydroModel.Activities.Add(timeDependentModel1);
            hydroModel.Activities.Add(timeDependentModel2);

            // Test whether the model is present in the viewmodel and in the hydromodel
            Assert.AreEqual(2, viewModel.Models.Count);
            CollectionAssert.AreEquivalent(new[] { name1, name2 }, viewModel.Models.Select(m => m.Name).ToList());

            // Test whether invoking RemoveSubModel with a null value (nothing is selected in the GUI)
            // is indeed not having any effect on the viewmodel and hydromodel
            viewModel.RemoveSubmodel.Execute(null);
            Assert.AreEqual(2, viewModel.Models.Count);
            CollectionAssert.AreEquivalent(new[] { name1, name2 }, viewModel.Models.Select(m => m.Name).ToList());

            // Test whether invoking RemoveSubModel is indeed removing the specific submodel from the hydromodel
            var submodelToRemove = viewModel.Models.FirstOrDefault(m => m.Name == name2);
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
    }
}
