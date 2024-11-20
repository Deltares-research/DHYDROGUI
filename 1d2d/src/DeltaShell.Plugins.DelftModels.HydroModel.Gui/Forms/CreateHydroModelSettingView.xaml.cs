using System;
using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms
{
    /// <summary>
    /// Interaction logic for CreateHydroModelSettingView.xaml
    /// </summary>
    public partial class CreateHydroModelSettingView : UserControl, IProjectTemplateSettingsView
    {
        public CreateHydroModelSettingView()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        public object Data
        {
            get { return ViewModel.ProjectTemplate; }
            set { ViewModel.ProjectTemplate = (ProjectTemplate)value; }
        }

        public string Text { get; set; }
        
        public Image Image { get; set; }
        
        public ViewInfo ViewInfo { get; set; }

        /// <summary>
        /// Action for executing the <see cref="ProjectTemplate"/>
        /// </summary>
        public Action<object> ExecuteProjectTemplate
        {
            get { return ViewModel.ExecuteProjectTemplate; }
            set { ViewModel.ExecuteProjectTemplate = value; }
        }

        /// <summary>
        /// Action for canceling the view
        /// </summary>
        public Action Cancel
        {
            get { return ViewModel.CancelProjectTemplate; }
            set { ViewModel.CancelProjectTemplate = value; }
        }

        public void Dispose() { }

        public void EnsureVisible(object item) { }
    }
}
