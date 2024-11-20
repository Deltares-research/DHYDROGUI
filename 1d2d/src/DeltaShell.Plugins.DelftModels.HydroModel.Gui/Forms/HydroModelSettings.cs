using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms
{
    public partial class HydroModelSettings : UserControl, ILayerEditorView
    {
        public HydroModelSettings()
        {
            InitializeComponent();
        }

        public object Data
        {
            get { return view.Model; }
            set
            {
                view.Model = (HydroModel) value;
                if (view.Model != null)
                {
                    Text = $"{view.Model.Name} Settings";
                }
            }
        }
        
        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        public Func<HydroModel, IActivity> AddNewActivityCallback
        {
            get { return view.ViewModel.AddNewActivityCallback; }
            set { view.ViewModel.AddNewActivityCallback = value; }
        }

        public Action<HydroModel> RunCallback
        {
            get { return view.ViewModel.RunActivityCallback; }
            set { view.ViewModel.RunActivityCallback = value; }
        }

        public Action<IActivity> WorkflowSelectedCallback { get; set; }

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