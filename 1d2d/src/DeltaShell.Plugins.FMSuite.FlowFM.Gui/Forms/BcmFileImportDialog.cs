using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public class BcmFileImportDialog : BcFileImportDialog
    {
        public override DelftDialogResult ShowModal()
        {
            openFileDialog.FileName = string.Empty;
            openFileDialog.DefaultExt = BcmFile.Extension;
            openFileDialog.Filter = new BcmFileImporter().FileFilter;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return DelftDialogResult.Cancel;
            }
            FilePaths = openFileDialog.FileNames;
            var result = ShowDialog() == DialogResult.OK ? DelftDialogResult.OK : DelftDialogResult.Cancel;
            if (result == DelftDialogResult.OK && deleteDataCheckBox.Checked &&
                quantitiesListBox.CheckedItems.Count * dataTypesListBox.CheckedItems.Count == 0)
            {
                var extraResult = MessageBox.Show(
                    "You are deleting data without importing any boundary condition data. Do you wish to continue?",
                    "Deleting data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return extraResult == DialogResult.Yes ? DelftDialogResult.OK : DelftDialogResult.Cancel;
            }
            return result;
        }

        public override void Configure(object importer)
        {
            var bcmFileImporter = importer as BcmFileImporter;
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