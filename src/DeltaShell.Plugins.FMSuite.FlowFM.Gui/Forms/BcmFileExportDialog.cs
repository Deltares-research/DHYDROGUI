using System;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public class BcmFileExportDialog : BcFileExportDialog
    {
        private void ExportModeComboBoxFormat(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is BcmFile.WriteMode)
            {
                e.Value = EnumDescriptionAttributeTypeConverter.GetEnumDescription((BcmFile.WriteMode)e.ListItem);
            }
        }

        public override DelftDialogResult ShowModal()
        {
            saveFileDialog.DefaultExt = BcmFile.Extension;
            saveFileDialog.Filter = new BcmFileExporter().FileFilter;
            exportModeComboBox.Items.AddRange(Enum.GetValues(typeof(BcmFile.WriteMode)).Cast<object>().ToArray());
            exportModeComboBox.SelectedIndex = 0;
            exportModeComboBox.Format += ExportModeComboBoxFormat;

            if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return DelftDialogResult.Cancel;
            }
            FilePath = saveFileDialog.FileName;
            return ShowDialog() == DialogResult.OK ? DelftDialogResult.OK : DelftDialogResult.Cancel;
        }

        public override void Configure(object model)
        {
            var bcmFileExporter = model as BcmFileExporter;
            if (bcmFileExporter != null)
            {
                bcmFileExporter.ExcludedQuantities =
                    quantities.Keys.Except(quantitiesListBox.CheckedItems.Cast<string>())
                        .Select(k => quantities[k])
                        .ToList();

                bcmFileExporter.ExcludedDataTypes =
                    dataTypes.Keys.Except(dataTypesListBox.CheckedItems.Cast<string>())
                        .Select(k => dataTypes[k])
                        .ToList();

                bcmFileExporter.WriteMode = (BcmFile.WriteMode)exportModeComboBox.SelectedItem;

                bcmFileExporter.FilePath = FilePath;
            }
        }
    }
}