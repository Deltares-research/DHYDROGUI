using System;
using System.Windows.Controls;
using System.Windows.Input;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.ViewModels;
using Xceed.Wpf.Toolkit;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Views
{
    /// <summary>
    /// Interaction logic for HydroModelTimeSettingsUserControl.xaml
    /// </summary>
    public partial class HydroModelTimeSettingsUserControl : UserControl
    {
        #region Members

        public HydroModel Model
        {
            get { return ViewModel.Model; }
            set { ViewModel.Model = value; }
        }
        
        #endregion
        public HydroModelTimeSettingsUserControl()
        {
            InitializeComponent();
        }
    }
}
