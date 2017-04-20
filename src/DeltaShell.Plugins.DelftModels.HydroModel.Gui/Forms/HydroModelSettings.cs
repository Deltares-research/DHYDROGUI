using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Globalization;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms
{
    public partial class HydroModelSettings : UserControl, ISuspendibleView, ILayerEditorView
    {
        private HydroModel model;
        private readonly IList<INameable> emptyDataSource = new List<INameable>();
        private bool updating;

        public HydroModelSettings()
        {
            InitializeComponent();

            startTime.CustomFormat = RegionalSettingsManager.DateTimeFormat;
            stopTime.CustomFormat = RegionalSettingsManager.DateTimeFormat;
            timeStep.ValidatingType = typeof (TimeSpan);
            BackColorChanged += HydroModelSettingsBackColorChanged;
        }

        void HydroModelSettingsBackColorChanged(object sender, EventArgs e)
        {
            workflowEditorControl.BackColor = BackColor;
            workflowEditorControl.GraphControl.BackColor = BackColor;
        }

        public object Data
        {
            get { return HydroModel; }
            set
            {
                updating = true;
                HydroModel = (HydroModel) value;
                updating = false;
            }
        }

        public HydroModel HydroModel
        {
            get { return model; } 
            set
            {
                SuspendUpdates();

                if (model != null)
                {
                    model.CollectionChanged -= ModelOnCollectionChanged;
                    ((INotifyPropertyChanged)model).PropertyChanged -= OnModelPropertyChanged;
                }

                model = value;
                
                if(model != null)
                {
                    model.CollectionChanged += ModelOnCollectionChanged;
                    ((INotifyPropertyChanged)model).PropertyChanged += OnModelPropertyChanged;
                }

                if (model == null)
                {
                    bindingSourceHydroModel.DataSource = typeof (HydroModel);
                    listBoxActivities.DataSource = emptyDataSource;

                    workflowEditorControl.Workflows = null;
                    return;
                }

                Text = model.Name + " Settings";
                startTime.Value = model.StartTime;
                stopTime.Value = model.StopTime;
                timeStep.Text = model.TimeStep.ToString();

                UpdateDurationLabel();

                checkBoxOverrideStartTime.Checked = HydroModel.OverrideStartTime;
                checkBoxOverrideStopTime.Checked = HydroModel.OverrideStopTime;
                checkBoxOverrideTimeStep.Checked = HydroModel.OverrideTimeStep;

                bindingSourceHydroModel.DataSource = new BindingList<HydroModel>(new[] {model}){RaiseListChangedEvents = false};

                RefreshActivitiesListBox();
                RefreshWorkflowsControls();

                ResumeUpdates();
            }
        }

        public void SuspendUpdates()
        {
            bindingSourceHydroModel.SuspendBinding();
            bindingSourceHydroModel.DataSource = typeof(HydroModel);
        }

        public void ResumeUpdates()
        {
            bindingSourceHydroModel.DataSource = new BindingList<HydroModel>(new[] { model }) { RaiseListChangedEvents = false };
            bindingSourceHydroModel.ResumeBinding();
        }

        private void ModelOnCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if(Equals(sender, HydroModel.Activities))
            {
                RefreshActivitiesListBox();
                RefreshWorkflowsControls();
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        public Func<HydroModel, IActivity> AddNewActivityCallback { get; set; }

        public Action<IActivity> RemoveActivityCallback { get; set; }

        public Action<HydroModel> RunCallback { get; set; }

        public Action<IActivity> WorkflowSelectedCallback { get; set; }

        private void listBoxActivities_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                OnDeleteActivityClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Insert && e.Alt)
            {
                OnAddActivityClicked();
                e.Handled = true;
            }
        }

        private void buttonAddActivity_Click(object sender, EventArgs e)
        {
            OnAddActivityClicked();
        }

        private void OnAddActivityClicked()
        {
            if (AddNewActivityCallback != null)
            {
                this.SuspendDrawing();

                var editAction = new DefaultEditAction("Add activity: <unknown>");
                HydroModel.BeginEdit(editAction);

                var activity = AddNewActivityCallback(HydroModel);
                
                if (activity != null)
                {
                    editAction.Name = String.Format("Add activity: {0}", activity.Name);
                    listBoxActivities.SelectedIndex = HydroModel.Activities.IndexOf(activity);
                    RefreshActivitiesListBox();
                    RefreshWorkflowsControls();
                }

                HydroModel.EndEdit();

                this.ResumeDrawing();
            }
            else
            {
                MessageBox.Show("Implement add new model/tool");
            }
        }

        private void buttonDeleteActivity_Click(object sender, EventArgs e)
        {
            OnDeleteActivityClick();
        }

        private void OnDeleteActivityClick()
        {
            if (listBoxActivities.SelectedIndex != -1 && listBoxActivities.SelectedIndex < model.Activities.Count)
            {
                if (RemoveActivityCallback != null)
                {
                    this.SuspendDrawing();

                    RemoveActivityCallback(model.Activities[listBoxActivities.SelectedIndex]);
                    RefreshActivitiesListBox();
                    RefreshWorkflowsControls();

                    this.ResumeDrawing();
                }
                else
                {
                    MessageBox.Show("Implement delete model/tool");
                }
            }
        }

        private void RefreshActivitiesListBox()
        {
            listBoxActivities.SuspendLayout();
            listBoxActivities.DataSource = emptyDataSource;
            listBoxActivities.DataSource = new BindingList<IActivity>(model.Activities) { RaiseListChangedEvents = false };
            listBoxActivities.ResumeLayout();
        }
        
        private void RefreshWorkflowsControls()
        {
            updating = true;
            workflowEditorControl.Workflows = model.Workflows;
            workflowEditorControl.CurrentWorkflow = model.CurrentWorkflow;
            updating = false;
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (RunCallback != null)
            {
                RunCallback(HydroModel);
            }
            else
            {
                MessageBox.Show("Implement run HydroModel");
            }
        }

        private readonly string parameterValueName = TypeUtils.GetMemberName<Parameter>(p => p.Value);

        [InvokeRequired]
        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (updating) return;
            updating = true;

            switch (e.PropertyName)
            {
                case "CurrentWorkflow":
                    workflowEditorControl.CurrentWorkflow = model.CurrentWorkflow;
                    break;
                case "OverrideStartTime":
                    checkBoxOverrideStartTime.Checked = model.OverrideStartTime;
                    break;
                case "OverrideStopTime":
                    checkBoxOverrideStopTime.Checked = model.OverrideStopTime;
                    break;
                case "OverrideTimeStep":
                    checkBoxOverrideTimeStep.Checked = model.OverrideTimeStep;
                    break;
            }

            if (sender is Parameter && e.PropertyName == parameterValueName)
            {
                // these will bubble through Parameter<T>.Value prop change
                startTime.Value = model.StartTime;
                stopTime.Value = model.StopTime;
                timeStep.Text = model.TimeStep.ToString();
            }

            updating = false;
        }

        private void checkBoxOverrideStartTime_CheckedChanged(object sender, EventArgs e)
        {
            startTime.Enabled = checkBoxOverrideStartTime.Checked;

            if (updating) return;
            updating = true;

            ValidateChildren();

            HydroModel.OverrideStartTime = checkBoxOverrideStartTime.Checked;

            UpdateDurationLabel();
            updating = false;
        }

        private void checkBoxOverrideStopTime_CheckedChanged(object sender, EventArgs e)
        {
            stopTime.Enabled = checkBoxOverrideStopTime.Checked;

            if (updating) return;
            updating = true;

            ValidateChildren();

            HydroModel.OverrideStopTime = checkBoxOverrideStopTime.Checked;

            UpdateDurationLabel();
            updating = false;
        }

        private void checkBoxOverrideTimeStep_CheckedChanged(object sender, EventArgs e)
        {
            timeStep.Enabled = checkBoxOverrideTimeStep.Checked;

            if (updating) return;
            updating = true;

            ValidateChildren();

            HydroModel.OverrideTimeStep = checkBoxOverrideTimeStep.Checked;
            updating = false;
        }

        private void UpdateDurationLabel()
        {
            var duration = model.StopTime - model.StartTime;
            labelDuration.Text = duration.Days + " days " + duration.Hours + " hours " + duration.Minutes + " minutes " +
                                 duration.Seconds + " seconds";
        }

        private void startTime_ValueChanged(object sender, EventArgs e)
        {
            if (updating) return;
            updating = true;
            if (ValidateChildren())
            {
                HydroModel.StartTime = startTime.Value;
                UpdateDurationLabel();
            }
            updating = false;
        }

        private void stopTime_ValueChanged(object sender, EventArgs e)
        {
            if (updating) return;
            updating = true;
            if (ValidateChildren())
            {
                HydroModel.StopTime = stopTime.Value;
                UpdateDurationLabel();
            }
            updating = false;
        }

        private void timeStep_Validated(object sender, EventArgs e)
        {
            if (updating) return;
            updating = true;
            if (ValidateChildren())
            {
                var result = new TimeSpan();
                if (TimeSpan.TryParse(timeStep.Text, out result))
                {
                    HydroModel.TimeStep = result;
                }
            }
            updating = false;
        }

        private void startTime_Validating(object sender, CancelEventArgs e)
        {
            if (stopTime.Value < startTime.Value)
            {
                errorProvider1.SetError(startTime, "Start time must be less than stop time");
            }
            else
            {
                errorProvider1.SetError(startTime, "");
            }
        }

        private void stopTime_Validating(object sender, CancelEventArgs e)
        {
            if (stopTime.Value < startTime.Value)
            {
                errorProvider1.SetError(stopTime, "Stop time must be greater than start time");
            }
            else
            {
                errorProvider1.SetError(stopTime, "");
            }
        }

        private void timeStep_Validating(object sender, CancelEventArgs e)
        {
            var result = new TimeSpan();
            if (TimeSpan.TryParse(timeStep.Text, out result))
            {
                if (result.TotalSeconds <= 0)
                {
                    errorProvider1.SetError(timeStep, "Time step must be positive");
                    return;
                }
            }
            else
            {
                errorProvider1.SetError(timeStep, "Can not parse time step");
                return;
            }

            errorProvider1.SetError(timeStep, "");
        }

        private void workflowEditorControl_CurrentWorkflowChanged(object sender, EventArgs e)
        {
            if (model == null || updating) return;

            model.BeginEdit(new DefaultEditAction("Setting current workflow to : " + workflowEditorControl.CurrentWorkflow));
            model.CurrentWorkflow = workflowEditorControl.CurrentWorkflow;
            model.EndEdit();

            if (WorkflowSelectedCallback != null)
                WorkflowSelectedCallback(model.CurrentWorkflow);
        }

        private void WorkflowEditorControlSelectedActivityChanged(object sender, EventArgs<IActivity> e)
        {
            // NOTE: It's necessary to respond to the current workflow being selected
            //       as the 'initially selected workflow' is set while updating == true
            //       Without this event, users would need to click 'off' and back 'on' 
            //       to the 'initially selected workflow' in order to display its properties
            if (model == null || updating || WorkflowSelectedCallback == null) return;
            WorkflowSelectedCallback(e.Value);
        }

        public IEnumerable<IFeature> SelectedFeatures { get; set; }

        public event EventHandler SelectedFeaturesChanged;
        public ILayer Layer { set; get; }
        public void OnActivated()
        {
            
        }

        public void OnDeactivated()
        {
            
        }
    }
}