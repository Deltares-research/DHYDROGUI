using System;
using System.ComponentModel;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.ViewModels
{
    [Entity]
    public class TimeDependentModelBaseViewModel : IDisposable
    {
        private readonly ITimeDependentModel timeDependentModel;
        private DateTime startTime;
        private DateTime stopTime;
        private bool isUpdatingModel;
        private TimeSpan timeStep;

        public TimeDependentModelBaseViewModel(ITimeDependentModel timeDependentModel)
        {
            this.timeDependentModel = timeDependentModel;

            SyncTimesAfterAction();
            Name = timeDependentModel.Name;

            ((INotifyPropertyChanged)timeDependentModel).PropertyChanged +=  OnTimeDependentModelPropertyChanged;
        }

        public string Name { get; set; }

        public DateTime StartTime
        {
            get { return startTime; }
            set
            {
                startTime = value;
                SyncTimesAfterAction(() => Model.StartTime = startTime);
            }
        }

        public DateTime StopTime
        {
            get { return stopTime; }
            set
            {
                stopTime = value;
                SyncTimesAfterAction(() => Model.StopTime = stopTime);
            }
        }

        public TimeSpan TimeStep
        {
            get { return timeStep; }
            set
            {
                timeStep = value;
                SyncTimesAfterAction(()=> Model.TimeStep = timeStep);
            }
        }

        public string DurationText { get; set; }

        public bool DurationIsValid { get; set; }

        public ITimeDependentModel Model
        {
            get { return timeDependentModel; }
        }

        private void UpdateDurationText()
        {
            var intervalLength = StopTime - StartTime;
            DurationText = intervalLength.Days + " days " + intervalLength.Hours + " hours " + intervalLength.Minutes + " minutes " +
                       intervalLength.Seconds + " seconds";
            DurationIsValid = ! string.IsNullOrEmpty(DurationText) && intervalLength > TimeSpan.Zero;
        }

        private void OnTimeDependentModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is TimeDependentModelBase && e.PropertyName == TypeUtils.GetMemberName<TimeDependentModelBase>(m => m.Name))
            {
                Name = timeDependentModel.Name;
            }

            var parameter = sender as Parameter;
            if (parameter == null || e.PropertyName != TypeUtils.GetMemberName<Parameter>(p => p.Value)) return;

            SyncTimesAfterAction();
        }

        private void SyncTimesAfterAction(Action action = null)
        {
            if (isUpdatingModel) return;

            isUpdatingModel = true;

            if (action != null)
            {
                action();
            }

            StartTime = Model.StartTime;
            StopTime = Model.StopTime;
            TimeStep = Model.TimeStep;

            UpdateDurationText();

            isUpdatingModel = false;
        }

        public void Dispose()
        {
            ((INotifyPropertyChanged)timeDependentModel).PropertyChanged -= OnTimeDependentModelPropertyChanged;
        }
    }
}