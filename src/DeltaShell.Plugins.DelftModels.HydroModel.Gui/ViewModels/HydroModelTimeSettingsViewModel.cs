using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Commands;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.ViewModels
{
    [Entity]
    public class HydroModelTimeSettingsViewModel
    {
        private DateTime startTime;
        private DateTime stopTime;
        private TimeSpan timeStep;
        private bool startTimeEnabled = true;
        private bool stopTimeEnabled = true;
        private bool timeStepEnabled = true;
        private ICommand deleteModelCommand;
        private ICommand addModelCommand;
        private HydroModel hydroModel;
        private bool isUpdatingModel;

        private readonly string parameterValueName = TypeUtils.GetMemberName<Parameter>(p => p.Value);

        #region Properties
        
        public HydroModel HydroModel
        {
            get { return hydroModel; }
            set
            {
                if (hydroModel != null)
                {
                    ((INotifyPropertyChanged)hydroModel).PropertyChanged -= OnModelPropertyChanged;
                    hydroModel.CollectionChanged -= OnModelCollectionChanged;
                    var models = Models.ToList();
                    Models.Clear();
                    models.ForEach(m => m.Dispose());
                }

                hydroModel = value;

                if (hydroModel == null) return;

                SyncTimesAfterAction();

                Models = new ObservableCollection<TimeDependentModelBaseViewModel>(hydroModel.Activities
                    .OfType<ITimeDependentModel>()
                    .Select(m => new TimeDependentModelBaseViewModel(m)));

                ((INotifyPropertyChanged)hydroModel).PropertyChanged += OnModelPropertyChanged;
                hydroModel.CollectionChanged += OnModelCollectionChanged;
            }
        }

        public Func<HydroModel, IActivity> AddNewActivityCallback { get; set; }

        public ICommand RemoveSubmodel
        {
            get { return deleteModelCommand ?? (deleteModelCommand = new RelayCommand(RemoveModelFromHydroModel, CanRemoveModel)); }
        }

        public ICommand AddSubmodel
        {
            get { return addModelCommand ?? (addModelCommand = new RelayCommand(m => AddModelToHydroModel(), m => CanAddSubmodel())); }
        }

        public string DurationText { get; set; }

        public ObservableCollection<string> ErrorTexts { get; set; }

        public ObservableCollection<TimeDependentModelBaseViewModel> Models { get; set; }

        public DateTime StartTime
        {
            get{ return startTime; }
            set
            {
                startTime = value;
                SyncTimesAfterAction(() => HydroModel.StartTime = value);
            }
        }

        public DateTime StopTime
        {
            get { return stopTime; }
            set
            {
                stopTime = value;

                SyncTimesAfterAction(() => HydroModel.StopTime = value);
            }
        }

        public TimeSpan TimeStep
        {
            get { return timeStep; }
            set
            {
                timeStep = value;

                SyncTimesAfterAction(() => HydroModel.TimeStep = value);
            }
        }

        public bool StartTimeEnabled
        {
            get { return startTimeEnabled; }
            set
            {
                startTimeEnabled = value;
                
                SyncTimesAfterAction(() => HydroModel.OverrideStartTime = value);
            }
        }

        public bool StopTimeEnabled
        {
            get { return stopTimeEnabled; }
            set
            {
                stopTimeEnabled = value;
                SyncTimesAfterAction(() => HydroModel.OverrideStopTime = value);
            }
        }

        public bool TimeStepEnabled
        {
            get { return timeStepEnabled; }
            set
            {
                timeStepEnabled = value;
                SyncTimesAfterAction(() => HydroModel.OverrideTimeStep = value);
            }
        }

        public bool DurationIsValid { get; set; }

        #endregion

        #region Methods

        private void DetermineErrorTexts()
        {
            if(ErrorTexts == null) ErrorTexts = new ObservableCollection<string>();
            ErrorTexts.Clear();

            if (!DurationIsValid)
                ErrorTexts.Add(Resources.HydroModelTimeSettingsViewModel_DetermineErrorText_Start_time_must_be_earlier_than_stop_time);
            if (TimeStep <= TimeSpan.Zero)
                ErrorTexts.Add(Resources.HydroModelTimeSettingsViewModel_DetermineErrorText_Time_step_must_be_positive);
        }

        private void UpdateDurationText()
        {
            var intervalLength = StopTime - StartTime;
            DurationText = string.Format(Resources.HydroModelTimeSettingsViewModel_UpdateDurationLabel__0__days__1__hours__2__minutes__3__seconds, intervalLength.Days, intervalLength.Hours , intervalLength.Minutes, intervalLength.Seconds );

            DurationIsValid = !string.IsNullOrEmpty(DurationText) && intervalLength > TimeSpan.Zero;
            DetermineErrorTexts();
        }

        private void RemoveModelFromHydroModel(object model)
        {
            if (!CanRemoveModel(model)) return;

            var modelToRemove = model as TimeDependentModelBaseViewModel;
            HydroModel.Activities.RemoveAllWhere(m => { return modelToRemove != null && m.Name == modelToRemove.Name; });
        }

        private bool CanRemoveModel(object model)
        {
            var modelToRemove = model as TimeDependentModelBaseViewModel;
            return modelToRemove != null && HydroModel != null &&
                   HydroModel.Activities.Any(a => a.Name == modelToRemove.Name);
        }

        private void AddModelToHydroModel()
        {
            if (AddNewActivityCallback == null || !CanAddSubmodel()) return;
            
            var editAction = new DefaultEditAction("Add activity: <unknown>");
            HydroModel.BeginEdit(editAction);

            var activity = AddNewActivityCallback(HydroModel);

            if (activity != null)
            {
                editAction.Name = string.Format("Add activity: {0}", activity.Name);
            }

            HydroModel.EndEdit();
        }

        private bool CanAddSubmodel()
        {
            return HydroModel != null;
        }

        private void OnModelCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            var timeDependentModel = e.Item as ITimeDependentModel;
            if (sender != HydroModel.Activities || timeDependentModel == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    if (Models.Any(m => m.Model == timeDependentModel)) return;
                    Models.Add(new TimeDependentModelBaseViewModel(timeDependentModel));
                    break;
                case NotifyCollectionChangeAction.Remove:
                    var tdViewModel = Models.FirstOrDefault(m => m.Model == timeDependentModel);
                    if (tdViewModel == null) return;
                    Models.Remove(tdViewModel);
                    tdViewModel.Dispose();
                    break;
            }
        }

        [InvokeRequired]
        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((!(sender is Parameter) || e.PropertyName != parameterValueName) &&
                e.PropertyName != "OverrideStartTime" && 
                e.PropertyName != "OverrideStopTime" &&
                e.PropertyName != "OverrideTimeStep")
            {
                return;
            }

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

            StartTime = HydroModel.StartTime;
            StopTime = HydroModel.StopTime;
            TimeStep = HydroModel.TimeStep;

            StartTimeEnabled = HydroModel.OverrideStartTime;
            StopTimeEnabled = HydroModel.OverrideStopTime;
            TimeStepEnabled = HydroModel.OverrideTimeStep;

            UpdateDurationText();

            isUpdatingModel = false;
        }

        #endregion
    }
}
