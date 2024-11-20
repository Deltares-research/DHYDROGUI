using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Services;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    /// <summary>
    /// Provides a dialog for exporting D-FlowFM models.
    /// </summary>
    public sealed class FMModelExportDialog : Form, IConfigureDialog, IView
    {
        private string selectedDirectory;
        
        /// <inheritdoc/>
        public string Title { get; set; }

        /// <inheritdoc/>
        public object Data { get; set; }

        /// <inheritdoc/>
        public Image Image { get; set; }

        /// <inheritdoc/>
        public ViewInfo ViewInfo { get; set; }

        /// <inheritdoc/>
        public DelftDialogResult ShowModal()
        {
            var folderDialogService = new FolderDialogService();
            var folderDialogOptions = new FolderDialogOptions();

            string selectedPath = folderDialogService.ShowSelectFolderDialog(folderDialogOptions);
            
            if (string.IsNullOrEmpty(selectedPath))
            {
                return DelftDialogResult.Cancel;
            }

            selectedDirectory = selectedPath;
            return DelftDialogResult.OK;
        }

        /// <inheritdoc/>
        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        /// <inheritdoc/>
        public void Configure(object item)
        {
            if (item is FMModelFileExporter exporter)
            {
                exporter.ExportDirectory = selectedDirectory;
            }
        }

        /// <inheritdoc/>
        public void EnsureVisible(object item)
        {
        }
    }
}