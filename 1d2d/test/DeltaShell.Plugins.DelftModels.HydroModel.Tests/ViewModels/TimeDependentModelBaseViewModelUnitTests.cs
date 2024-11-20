using System;
using System.ComponentModel;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.ViewModels;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.ViewModels
{
    [TestFixture]
    public class TimeDependentModelBaseViewModelUnitTests
    {
        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestNameChange()
        {
            var initialModelName = "Initial Model Name";
            var newModelName = "New Model Name";
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(typeof(INotifyPropertyChanged));

            timeDependentModel.Expect(m => m.Name).Return(initialModelName).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StartTime).Return(DateTime.Now).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StopTime).Return(DateTime.Now).Repeat.Times(1);
            timeDependentModel.Expect(m => m.TimeStep).Return(new TimeSpan(1, 0, 0)).Repeat.Times(1);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            mocks.ReplayAll();

            var timeDependentModelBaseViewModel = new TimeDependentModelBaseViewModel(timeDependentModel);
            Assert.AreEqual(initialModelName, timeDependentModelBaseViewModel.Name);

            timeDependentModelBaseViewModel.Name = newModelName;
            Assert.AreEqual(newModelName, timeDependentModelBaseViewModel.Name);

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestStartTimeChange()
        {
            var initialStartTime = DateTime.Now;
            var newStartTime = initialStartTime.AddMonths(2);
            var initialStopTime = initialStartTime.AddDays(1);
            var initialTimeStep = new TimeSpan(1, 0, 0);

            // Initialize mock
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(typeof(INotifyPropertyChanged));

            timeDependentModel.Expect(m => m.Name).Return("").Repeat.Times(1);

            timeDependentModel.Expect(m => m.StartTime).Return(initialStartTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StartTime = newStartTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StartTime).Return(newStartTime).Repeat.Times(1);

            timeDependentModel.Expect(m => m.StopTime).Return(initialStopTime).Repeat.Times(2);
            timeDependentModel.Expect(m => m.TimeStep).Return(initialTimeStep).Repeat.Times(2);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            mocks.ReplayAll();

            var timeDependentModelBaseViewModel = new TimeDependentModelBaseViewModel(timeDependentModel);
            Assert.AreEqual(initialStartTime, timeDependentModelBaseViewModel.StartTime);

            timeDependentModelBaseViewModel.StartTime = newStartTime;
            Assert.AreEqual(newStartTime, timeDependentModelBaseViewModel.StartTime);

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestStopTimeChange()
        {
            var initialStartTime = DateTime.Now;
            var initialStopTime = initialStartTime.AddDays(1);
            var newStopTime = initialStartTime.AddMonths(2);
            var initialTimeStep = new TimeSpan(1, 0, 0);

            // Initialize mock
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(typeof(INotifyPropertyChanged));

            timeDependentModel.Expect(m => m.Name).Return("").Repeat.Times(1);
            timeDependentModel.Expect(m => m.StartTime).Return(initialStartTime).Repeat.Times(2);

            timeDependentModel.Expect(m => m.StopTime).Return(initialStopTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StopTime = newStopTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StopTime).Return(newStopTime).Repeat.Times(1);

            timeDependentModel.Expect(m => m.TimeStep).Return(initialTimeStep).Repeat.Times(2);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            mocks.ReplayAll();

            var timeDependentModelBaseViewModel = new TimeDependentModelBaseViewModel(timeDependentModel);
            Assert.AreEqual(initialStopTime, timeDependentModelBaseViewModel.StopTime);

            timeDependentModelBaseViewModel.StopTime = newStopTime;
            Assert.AreEqual(newStopTime, timeDependentModelBaseViewModel.StopTime);

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestTimeStepChange()
        {
            var initialStartTime = DateTime.Now;
            var initialStopTime = initialStartTime.AddDays(1);
            var initialTimeStep = new TimeSpan(1, 0, 0);
            var newTimeStep = new TimeSpan(0, 30, 0);

            // Initialize mock
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(typeof(INotifyPropertyChanged));

            timeDependentModel.Expect(m => m.Name).Return("").Repeat.Times(1);
            timeDependentModel.Expect(m => m.StartTime).Return(initialStartTime).Repeat.Times(2);
            timeDependentModel.Expect(m => m.StopTime).Return(initialStopTime).Repeat.Times(2);

            timeDependentModel.Expect(m => m.TimeStep).Return(initialTimeStep).Repeat.Times(1);
            timeDependentModel.Expect(m => m.TimeStep = newTimeStep).Repeat.Times(1);
            timeDependentModel.Expect(m => m.TimeStep).Return(newTimeStep).Repeat.Times(1);

            timeDependentModel.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            mocks.ReplayAll();

            var timeDependentModelBaseViewModel = new TimeDependentModelBaseViewModel(timeDependentModel);
            Assert.AreEqual(initialTimeStep, timeDependentModelBaseViewModel.TimeStep);

            timeDependentModelBaseViewModel.TimeStep = newTimeStep;
            Assert.AreEqual(newTimeStep, timeDependentModelBaseViewModel.TimeStep);

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestDurationValidity()
        {
            var initialStartTime = DateTime.Now;
            var initialStopTime = initialStartTime.AddDays(1);
            var initialTimeStep = new TimeSpan(1, 0, 0);

            var invalidStartTime = initialStopTime.AddMonths(3);

            // Initialize mock
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(typeof(INotifyPropertyChanged));

            timeDependentModel.Expect(m => m.Name).Return("").Repeat.Times(1);
            timeDependentModel.Expect(m => m.StartTime).Return(initialStartTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StartTime).Return(invalidStartTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StartTime).Return(initialStopTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StartTime = invalidStartTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StartTime = initialStopTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StopTime).Return(initialStopTime).Repeat.Times(3);
            timeDependentModel.Expect(m => m.TimeStep).Return(initialTimeStep).Repeat.Times(3);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            mocks.ReplayAll();

            var timeDependentModelBaseViewModel = new TimeDependentModelBaseViewModel(timeDependentModel);
            Assert.IsTrue(timeDependentModelBaseViewModel.DurationIsValid);
            
            timeDependentModelBaseViewModel.StartTime = invalidStartTime;
            Assert.IsFalse(timeDependentModelBaseViewModel.DurationIsValid);
            
            timeDependentModelBaseViewModel.StartTime = initialStopTime;
            Assert.IsFalse(timeDependentModelBaseViewModel.DurationIsValid);

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestDurationTextChange()
        {
            var initialStartTime = DateTime.Now;
            var initialStopTime = initialStartTime.AddDays(1).AddMinutes(30);
            var initialTimeStep = new TimeSpan(1, 0, 0);
            var intervalLength = initialStopTime - initialStartTime;
            var duration = string.Format(Resources.HydroModelTimeSettingsViewModel_UpdateDurationLabel__0__days__1__hours__2__minutes__3__seconds, intervalLength.Days, intervalLength.Hours, intervalLength.Minutes, intervalLength.Seconds);

            // Initialize mock
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(typeof(INotifyPropertyChanged));

            timeDependentModel.Expect(m => m.Name).Return("").Repeat.Times(1);
            timeDependentModel.Expect(m => m.StartTime).Return(initialStartTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StopTime).Return(initialStopTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.TimeStep).Return(initialTimeStep).Repeat.Times(1);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged)a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            mocks.ReplayAll();

            var timeDependentModelBaseViewModel = new TimeDependentModelBaseViewModel(timeDependentModel);
            Assert.AreEqual(duration, timeDependentModelBaseViewModel.DurationText);

            mocks.VerifyAll();
        }
    }
}
