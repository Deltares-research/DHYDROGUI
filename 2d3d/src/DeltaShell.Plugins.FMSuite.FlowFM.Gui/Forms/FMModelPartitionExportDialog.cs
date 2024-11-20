using DelftTools.Controls;
using DelftTools.Controls.Wpf.Services;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    /// <summary>
    /// Provides a dialog for exporting D-FlowFM model partitions.
    /// </summary>
    public sealed class FMModelPartitionExportDialog : FMPartitionExportDialogBase
    {
        private string selectedDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FMModelPartitionExportDialog"/> class.
        /// </summary>
        public FMModelPartitionExportDialog()
        {
            EnableSolverSelection = true;
        }

        /// <inheritdoc/>
        protected override DelftDialogResult ShowExportPathDialog()
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
        protected override void Configure(FMPartitionExporterBase exporter)
        {
            if (exporter is FMModelPartitionExporter modelPartitionExporter)
            {
                modelPartitionExporter.ExportDirectory = selectedDirectory;
                modelPartitionExporter.SolverType = SolverType;
            }
        }
    }
}