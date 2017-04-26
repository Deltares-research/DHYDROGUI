using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;
using DelftTools.Units;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.ViewModels
{
    [Entity]
    public class HydroModelTimeSettingsUserControlViewModel
    {
        private DateTime startTime;
        private DateTime stopTime;
        private TimeSpan timeStep;
        private bool startTimeEnabled = true;
        private bool stopTimeEnabled = true;
        private bool timeStepEnabled = true;

        private HydroModel model;
        private bool isUpdatingModel;

        public HydroModel Model
        {
            get { return model; }
            set
            {
                if (model != null)
                {
                    ((INotifyPropertyChanged)model).PropertyChanged -= OnModelPropertyChanged;
                }

                model = value;

                if (model == null) return;
                
                StartTime = model.StartTime;
                StopTime = model.StopTime;
                TimeStep = model.TimeStep;

                UpdateDurationLabel();

                ((INotifyPropertyChanged)model).PropertyChanged += OnModelPropertyChanged;
            }
        }

        [InvokeRequired]
        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (isUpdatingModel) return;

            switch (e.PropertyName)
            {
                case "OverrideStartTime":
                    StartTimeEnabled = Model.OverrideStartTime;
                    break;
                case "OverrideStopTime":
                    StopTimeEnabled = Model.OverrideStopTime;
                    break;
                case "OverrideTimeStep":
                    TimeStepEnabled = Model.OverrideTimeStep;
                    break;
            }

            if (sender is Parameter /*&& e.PropertyName == parameterValueName*/)
            {
                // these will bubble through Parameter<T>.Value prop change
                StartTime = Model.StartTime;
                StopTime = Model.StopTime;
                TimeStep = Model.TimeStep;
                UpdateDurationLabel();
            }
        }

        #region Properties
        public string Duration { get; set; }

        public string ErrorText { get; set; }

        public DateTime StartTime
        {
            get{ return startTime; }
            set
            {
                startTime = value;

                isUpdatingModel = true;
                Model.StartTime = value;
                UpdateDurationLabel();
                isUpdatingModel = false;
            }
        }
        public DateTime StopTime
        {
            get { return stopTime; }
            set
            {
                stopTime = value;
                isUpdatingModel = true;
                Model.StopTime = value;
                UpdateDurationLabel();
                isUpdatingModel = false;
            }
        }

        public TimeSpan TimeStep
        {
            get { return timeStep; }
            set
            {
                timeStep = value;

                isUpdatingModel = true;
                Model.TimeStep = value;
                DetermineErrorText();
                isUpdatingModel = false;
            }
        }
        public bool StartTimeEnabled
        {
            get { return startTimeEnabled; }
            set
            {
                startTimeEnabled = value;

                isUpdatingModel = true;
                Model.OverrideStartTime = value;
                isUpdatingModel = false;
            }
        }
        public bool StopTimeEnabled
        {
            get { return stopTimeEnabled; }
            set
            {
                stopTimeEnabled = value;
                isUpdatingModel = true;
                Model.OverrideStopTime = value;
                isUpdatingModel = false;
            }
        }
        public bool TimeStepEnabled
        {
            get { return timeStepEnabled; }
            set
            {
                timeStepEnabled = value;
                isUpdatingModel = true;
                Model.OverrideTimeStep = value;
                isUpdatingModel = false;
            }
        }
        #endregion

        #region Methods
        private void DetermineErrorText()
        {
            if (DurationIsValid && TimeStep.TotalSeconds > 0)
            {
                ErrorText = "";
            }
            else
            {
                ErrorText = !DurationIsValid 
                    ? "Start time must be earlier than stop time" 
                    : "Time step must be positive";
            }
        }

        public bool DurationIsValid { get; set; }

        private void UpdateDurationLabel()
        {
            var intervalLength = StopTime - StartTime;
            Duration = intervalLength.Days + " days " + intervalLength.Hours + " hours " + intervalLength.Minutes + " minutes " +
                                 intervalLength.Seconds + " seconds";

            DurationIsValid = !string.IsNullOrEmpty(Duration) && !Duration.Substring(0, 1).Equals("-");
            DetermineErrorText();
        }
        #endregion
    }
}
