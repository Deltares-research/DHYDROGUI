using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Services;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public class BcmFileImportDialog : BcFileImportDialog
    {
        public override DelftDialogResult ShowModal()
        {
            var fileDialogService = new FileDialogService();
            var fileDialogOptions = new FileDialogOptions
            {
                FileFilter = new BcmFileImporter().FileFilter,
                RestoreDirectory = true
            };
            
            string[] filePaths = fileDialogService.ShowOpenFilesDialog(fileDialogOptions);
            if (!filePaths.Any())
            {
                return DelftDialogResult.Cancel;
            }

            FilePaths = filePaths;

            DelftDialogResult result = ShowDialog() == DialogResult.OK ? DelftDialogResult.OK : DelftDialogResult.Cancel;
            if (result == DelftDialogResult.OK && deleteDataCheckBox.Checked &&
                quantitiesListBox.CheckedItems.Count * dataTypesListBox.CheckedItems.Count == 0)
            {
                DialogResult extraResult = MessageBox.Show(
                    "You are deleting data without importing any boundary condition data. Do you wish to continue?",
                    "Deleting data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return extraResult == DialogResult.Yes ? DelftDialogResult.OK : DelftDialogResult.Cancel;
            }

            return result;
        }

        public override void Configure(object item)
        {
            var bcmFileImporter = item as BcmFileImporter;
            if (bcmFileImporter != null)
            {
                bcmFileImporter.FilePaths = FilePaths;

                bcmFileImporter.ExcludedQuantities =
                    quantities.Keys.Except(quantitiesListBox.CheckedItems.Cast<string>())
                              .Select(k => quantities[k])
                              .ToList();

                bcmFileImporter.ExcludedDataTypes =
                    dataTypes.Keys.Except(dataTypesListBox.CheckedItems.Cast<string>())
                             .Select(k => dataTypes[k])
                             .ToList();

                bcmFileImporter.OverwriteExistingData = overwriteCheckBox.Checked;
                bcmFileImporter.DeleteDataBeforeImport = deleteDataCheckBox.Checked;
            }
        }
    }
}