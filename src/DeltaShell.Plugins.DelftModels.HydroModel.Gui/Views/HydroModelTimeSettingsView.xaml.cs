using System;
using System.Windows.Controls;
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Views
{
    /// <summary>
    /// Interaction logic for HydroModelTimeSettingsView.xaml
    /// </summary>
    public partial class HydroModelTimeSettingsView : UserControl
    {
        #region Members

        public HydroModel Model
        {
            get { return ViewModel.HydroModel; }
            set { ViewModel.HydroModel = value; }
        }
        
        #endregion
        public HydroModelTimeSettingsView()
        {
            InitializeComponent();
        }

        public Func<HydroModel, IActivity> AddNewActivityCallback
        {
            get { return ViewModel.AddNewActivityCallback; }
            set { ViewModel.AddNewActivityCallback = value; }
        }
    }
}
