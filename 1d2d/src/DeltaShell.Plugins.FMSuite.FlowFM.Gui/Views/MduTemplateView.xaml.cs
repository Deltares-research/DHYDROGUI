using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using Microsoft.Win32;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Views
{
    /// <summary>
    /// Interaction logic for MduTemplateView.xaml
    /// </summary>
    /// <remarks>
    /// Must be <seealso cref="UserControl"/>" because it will be presented in ProjectTemplateControl.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public sealed partial class MduTemplateView : UserControl, IProjectTemplateSettingsView
    {
        private bool disposed;

        /// <summary>
        /// MduTemplateView.
        /// Constructor initializes the importer dialog with a standard file open dialog and the <seealso cref="WaterFlowFMFileImporter"/>.
        /// </summary>
        public MduTemplateView()
        {
            InitializeComponent();
            var importer = new WaterFlowFMFileImporter(() => String.Empty);
            ViewModel.GetFilePath = () =>
            {
                var dialog = new OpenFileDialog
                {
                    AddExtension = true,
                    DefaultExt = "mdu",
                    Multiselect = false,
                    CheckPathExists = true,
                    Title = importer.Name,
                    Filter = importer.FileFilter
                };

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    return dialog.FileName;
                }

                return "";
            };
        }

        public object Data { get; set; }

        public string Text { get; set; }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public Action<object> ExecuteProjectTemplate
        {
            get
            {
                return ViewModel.ExecuteProjectTemplate;
            }
            set
            {
                ViewModel.ExecuteProjectTemplate = value;
            }
        }

        public Action Cancel
        {
            get
            {
                return ViewModel.Cancel;
            }
            set
            {
                ViewModel.Cancel = value;
            }
        }

        public void EnsureVisible(object item)
        {
            // nothing to focus
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                Image?.Dispose();
            }

            disposed = true;
        }
    }
}
