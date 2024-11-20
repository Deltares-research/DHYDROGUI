namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Views
{
    /// <summary>
    /// Interaction logic for HydroModelTimeSettingsView.xaml
    /// </summary>
    public partial class HydroModelTimeSettingsView
    {
        public HydroModelTimeSettingsView()
        {
            InitializeComponent();
        }

        #region Members

        public HydroModel Model
        {
            get
            {
                return ViewModel.HydroModel;
            }
            set
            {
                ViewModel.HydroModel = value;
            }
        }

        #endregion
    }
}