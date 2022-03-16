using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms
{
    public partial class HydroModelSettings : UserControl, ILayerEditorView
    {
        private HydroModel model;
        private bool updating;
        private Func<HydroModel, IActivity> addNewActivityCallback;

        public event EventHandler SelectedFeaturesChanged;

        public HydroModelSettings()
        {
            InitializeComponent();
            BackColorChanged += HydroModelSettingsBackColorChanged;
            UpdateRunButton();
        }

        public HydroModel HydroModel
        {
            get
            {
                return model;
            }
            set
            {
                SuspendUpdates();

                if (model != null)
                {
                    model.CollectionChanged -= ModelOnCollectionChanged;
                    ((INotifyPropertyChanged) model).PropertyChanged -= OnModelPropertyChanged;
                }

                model = value;

                view.Model = value;

                if (model != null)
                {
                    model.CollectionChanged += ModelOnCollectionChanged;
                    ((INotifyPropertyChanged) model).PropertyChanged += OnModelPropertyChanged;
                }

                if (model == null)
                {
                    bindingSourceHydroModel.DataSource = typeof(HydroModel);

                    workflowEditorControl.Workflows = null;
                    return;
                }

                Text = model.Name + " Settings";

                bindingSourceHydroModel.DataSource = new BindingList<HydroModel>(new[]
                {
                    model
                }) {RaiseListChangedEvents = false};

                RefreshWorkflowsControls();

                ResumeUpdates();
            }
        }

        public Func<HydroModel, IActivity> AddNewActivityCallback
        {
            get
            {
                return addNewActivityCallback;
            }
            set
            {
                addNewActivityCallback = value;
                view.ViewModel.AddNewActivityCallback = (hm) =>
                {
                    IActivity activity = addNewActivityCallback(hm);
                    RefreshWorkflowsControls();
                    return activity;
                };
            }
        }

        public Action<IActivity> RemoveActivityCallback { get; set; }

        public Action<HydroModel> RunCallback { get; set; }

        public Action<IActivity> WorkflowSelectedCallback { get; set; }

        public object Data
        {
            get
            {
                return HydroModel;
            }
            set
            {
                updating = true;
                HydroModel = (HydroModel) value;
                updating = false;
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public IEnumerable<IFeature> SelectedFeatures { get; set; }
        public ILayer Layer { set; get; }

        public void SuspendUpdates()
        {
            bindingSourceHydroModel.SuspendBinding();
            bindingSourceHydroModel.DataSource = typeof(HydroModel);
        }

        public void ResumeUpdates()
        {
            bindingSourceHydroModel.DataSource = new BindingList<HydroModel>(new[]
            {
                model
            }) {RaiseListChangedEvents = false};
            bindingSourceHydroModel.ResumeBinding();
        }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        public void OnActivated()
        {
            // Nothing to be done, enforced through ILayerEditorView
        }

        public void OnDeactivated()
        {
            // Nothing to be done, enforced through ILayerEditorView
        }

        private void HydroModelSettingsBackColorChanged(object sender, EventArgs e)
        {
            workflowEditorControl.BackColor = BackColor;
            workflowEditorControl.GraphControl.BackColor = BackColor;
        }

        private void ModelOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Equals(sender, HydroModel.Activities))
            {
                RefreshWorkflowsControls();
            }
        }

        private void RefreshWorkflowsControls()
        {
            updating = true;
            workflowEditorControl.Workflows = model.Workflows;
            workflowEditorControl.CurrentWorkflow = model.CurrentWorkflow;
            UpdateRunButton();
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

        [InvokeRequired]
        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (updating)
            {
                return;
            }

            updating = true;

            switch (e.PropertyName)
            {
                case "CurrentWorkflow":
                    workflowEditorControl.CurrentWorkflow = model.CurrentWorkflow;
                    UpdateRunButton();
                    break;
            }

            updating = false;
        }

        private void UpdateRunButton()
        {
            buttonRun.Enabled = workflowEditorControl.CurrentWorkflow != null;
        }

        private void workflowEditorControl_CurrentWorkflowChanged(object sender, EventArgs e)
        {
            if (model == null || updating)
            {
                return;
            }

            model.BeginEdit(new DefaultEditAction("Setting current workflow to : " + workflowEditorControl.CurrentWorkflow));
            model.CurrentWorkflow = workflowEditorControl.CurrentWorkflow;
            model.EndEdit();
            
            if (WorkflowSelectedCallback != null)
            {
                WorkflowSelectedCallback(model.CurrentWorkflow);
            }
        }

        private void WorkflowEditorControlSelectedActivityChanged(object sender, EventArgs<IActivity> e)
        {
            // NOTE: It's necessary to respond to the current workflow being selected
            //       as the 'initially selected workflow' is set while updating == true
            //       Without this event, users would need to click 'off' and back 'on' 
            //       to the 'initially selected workflow' in order to display its properties
            if (model == null || updating || WorkflowSelectedCallback == null)
            {
                return;
            }

            WorkflowSelectedCallback(e.Value);
        }
    }
}