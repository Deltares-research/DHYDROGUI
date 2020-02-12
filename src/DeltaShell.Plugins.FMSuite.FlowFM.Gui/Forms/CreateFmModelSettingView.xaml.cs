using System;
using System.Windows.Controls;
using System.Windows.Input;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    /// <summary>
    /// Interaction logic for CreateFmModelSettingView.xaml
    /// </summary>
    public partial class CreateFmModelSettingView : UserControl, IProjectTemplateSettingsView
    {
        public CreateFmModelSettingView()
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

        private void UIElement_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            CoordinateSystemComboBox.IsDropDownOpen = true;
        }
    }
}
