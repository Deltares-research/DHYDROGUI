using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Extensions;
using DelftTools.Shell.Core;
using Microsoft.Win32;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.ImportExport.Sobek.Wizard
{
    /// <summary>
    /// Interaction logic for SobekImportWizardControl.xaml
    /// </summary>
    public partial class SobekImportWizardControl : UserControl, IProjectTemplateSettingsView
    {
        private AdornerLayer parentControl;
        private Window parentWindow;

        public SobekImportWizardControl()
        {
            InitializeComponent();
            ViewModel.StartingImport = () =>
            {
                // disable clicking events
                parentControl = this.TryFindParent<AdornerLayer>();
                parentControl.IsEnabled = false;
                
                // disable esc key
                parentWindow = this.TryFindParent<Window>();
                parentWindow.PreviewKeyDown += OnParentWindowOnPreviewKeyDown;
            };
            
            ViewModel.FinishedImport = () =>
            {
                parentControl.IsEnabled = true;
                parentWindow.PreviewKeyDown -= OnParentWindowOnPreviewKeyDown;
                parentControl = null;
                parentWindow = null;
            };

            ViewModel.GetFilePath = () =>
            {
                var dialog = new OpenFileDialog
                {
                    Multiselect = false,
                    CheckPathExists = true,
                    Title = "Select sobek file",
                    Filter = "All supported files|network.tp;deftop.1;caselist.cmt|" + 
                             "Sobek 2.1* network files|network.tp|" + 
                             "SobekRE network files|deftop.1|" + 
                             "Sobek case list files|CASELIST.CMT"

                };

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    return dialog.FileName;
                }

                return "";
            };
        }

        private void OnParentWindowOnPreviewKeyDown(object sender, KeyEventArgs args)
        {
            args.Handled = ViewModel.IsRunning;
        }

        public object Data { get; set; }

        public string Text { get; set; }

        public IApplication Application
        {
            get { return ViewModel.Application; }
            set { ViewModel.Application = value; }
        }

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

        public void EnsureVisible(object item)
        {
            // no elements to focus
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Image?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
