using System.Windows.Forms;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    /// <summary>
    /// Provides a dialog for exporting grid file partitions.
    /// </summary>
    public sealed class FMGridPartitionExportDialog : FMPartitionExportDialogBase
    {
        private string filePath;
        
        /// <summary>
        /// Gets or sets the file name filter which determines the choices that appear in the grid file dialog.
        /// </summary>
        public string FileFilter { get; set; }

        /// <inheritdoc/>
        protected override DelftDialogResult ShowExportPathDialog()
        {
            var saveFileDialog = new SaveFileDialog
            {
                AddExtension = false,
                Filter = FileFilter
            };
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                filePath = saveFileDialog.FileName;
                return DelftDialogResult.OK;
            }

            return DelftDialogResult.Cancel;
        }

        /// <inheritdoc/>
        protected override void Configure(FMPartitionExporterBase exporter)
        {
            if (exporter is FMGridPartitionExporter gridPartitionExporter)
            {
                gridPartitionExporter.FilePath = filePath;
            }
        }
    }
}