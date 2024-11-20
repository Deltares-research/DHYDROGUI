using System;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public class BcmFileExportDialog : BcFileExportDialog
    {
        public override DelftDialogResult ShowModal()
        {
            saveFileDialog.DefaultExt = BcmFile.Extension;
            saveFileDialog.Filter = new BcmFileExporter().FileFilter;
            exportModeComboBox.Items.AddRange(Enum.GetValues(typeof(BcFile.WriteMode)).Cast<object>().ToArray());
            exportModeComboBox.SelectedIndex = 0;
            exportModeComboBox.Format += ExportModeComboBoxFormat;

            if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return DelftDialogResult.Cancel;
            }

            FilePath = saveFileDialog.FileName;
            return ShowDialog() == DialogResult.OK ? DelftDialogResult.OK : DelftDialogResult.Cancel;
        }

        public override void Configure(object item)
        {
            if (item is BcmFileExporter bcmFileExporter)
            {
                bcmFileExporter.ExcludedQuantities =
                    quantities.Keys.Except(quantitiesListBox.CheckedItems.Cast<string>())
                              .Select(k => quantities[k])
                              .ToList();

                bcmFileExporter.ExcludedDataTypes =
                    dataTypes.Keys.Except(dataTypesListBox.CheckedItems.Cast<string>())
                             .Select(k => dataTypes[k])
                             .ToList();

                bcmFileExporter.WriteMode = (BcFile.WriteMode) exportModeComboBox.SelectedItem;

                bcmFileExporter.FilePath = FilePath;
            }
        }

        private void ExportModeComboBoxFormat(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is BcFile.WriteMode mode)
            {
                e.Value = mode.GetDescription();
            }
        }
    }
}