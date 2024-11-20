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
    public class TimeDependentModelViewModelTests
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
            timeDependentModel.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            mocks.ReplayAll();

            var timeDependentModelViewModel = new TimeDependentModelViewModel(timeDependentModel);
            Assert.AreEqual(initialModelName, timeDependentModelViewModel.Name);

            timeDependentModelViewModel.Name = newModelName;
            Assert.AreEqual(newModelName, timeDependentModelViewModel.Name);

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestStartTimeChange()
        {
            DateTime initialStartTime = DateTime.Now;
            DateTime newStartTime = initialStartTime.AddMonths(2);
            DateTime initialStopTime = initialStartTime.AddDays(1);
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
            timeDependentModel.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            mocks.ReplayAll();

            var timeDependentModelViewModel = new TimeDependentModelViewModel(timeDependentModel);
            Assert.AreEqual(initialStartTime, timeDependentModelViewModel.StartTime);

            timeDependentModelViewModel.StartTime = newStartTime;
            Assert.AreEqual(newStartTime, timeDependentModelViewModel.StartTime);

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestStopTimeChange()
        {
            DateTime initialStartTime = DateTime.Now;
            DateTime initialStopTime = initialStartTime.AddDays(1);
            DateTime newStopTime = initialStartTime.AddMonths(2);
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
            timeDependentModel.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            mocks.ReplayAll();

            var timeDependentModelViewModel = new TimeDependentModelViewModel(timeDependentModel);
            Assert.AreEqual(initialStopTime, timeDependentModelViewModel.StopTime);

            timeDependentModelViewModel.StopTime = newStopTime;
            Assert.AreEqual(newStopTime, timeDependentModelViewModel.StopTime);

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestTimeStepChange()
        {
            DateTime initialStartTime = DateTime.Now;
            DateTime initialStopTime = initialStartTime.AddDays(1);
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

            timeDependentModel.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            mocks.ReplayAll();

            var timeDependentModelViewModel = new TimeDependentModelViewModel(timeDependentModel);
            Assert.AreEqual(initialTimeStep, timeDependentModelViewModel.TimeStep);

            timeDependentModelViewModel.TimeStep = newTimeStep;
            Assert.AreEqual(newTimeStep, timeDependentModelViewModel.TimeStep);

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestDurationValidity()
        {
            DateTime initialStartTime = DateTime.Now;
            DateTime initialStopTime = initialStartTime.AddDays(1);
            var initialTimeStep = new TimeSpan(1, 0, 0);

            DateTime invalidStartTime = initialStopTime.AddMonths(3);

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
            timeDependentModel.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            mocks.ReplayAll();

            var timeDependentModelViewModel = new TimeDependentModelViewModel(timeDependentModel);
            Assert.IsTrue(timeDependentModelViewModel.DurationIsValid);

            timeDependentModelViewModel.StartTime = invalidStartTime;
            Assert.IsFalse(timeDependentModelViewModel.DurationIsValid);

            timeDependentModelViewModel.StartTime = initialStopTime;
            Assert.IsFalse(timeDependentModelViewModel.DurationIsValid);

            mocks.VerifyAll();
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestDurationTextChange()
        {
            DateTime initialStartTime = DateTime.Now;
            DateTime initialStopTime = initialStartTime.AddDays(1).AddMinutes(30);
            var initialTimeStep = new TimeSpan(1, 0, 0);
            TimeSpan intervalLength = initialStopTime - initialStartTime;
            string duration = string.Format(Resources.HydroModelTimeSettingsViewModel_UpdateDurationLabel__0__days__1__hours__2__minutes__3__seconds, intervalLength.Days, intervalLength.Hours, intervalLength.Minutes, intervalLength.Seconds);

            // Initialize mock
            var mocks = new MockRepository();
            var timeDependentModel = mocks.StrictMultiMock<ITimeDependentModel>(typeof(INotifyPropertyChanged));

            timeDependentModel.Expect(m => m.Name).Return("").Repeat.Times(1);
            timeDependentModel.Expect(m => m.StartTime).Return(initialStartTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.StopTime).Return(initialStopTime).Repeat.Times(1);
            timeDependentModel.Expect(m => m.TimeStep).Return(initialTimeStep).Repeat.Times(1);
            timeDependentModel.Expect(a => ((INotifyPropertyChanged) a).PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Times(1);
            mocks.ReplayAll();

            var timeDependentModelViewModel = new TimeDependentModelViewModel(timeDependentModel);
            Assert.AreEqual(duration, timeDependentModelViewModel.DurationText);

            mocks.VerifyAll();
        }
    }
}