using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public partial class BcFileExportDialog : Form, IConfigureDialog, IView
    {
        protected readonly IDictionary<string, FlowBoundaryQuantityType> quantities;
        protected readonly IDictionary<string, BoundaryConditionDataType> dataTypes;

        public BcFileExportDialog()
        {
            InitializeComponent();

            quantities =
                FlowBoundaryConditionEditorController.SupportedFlowQuantities.ToDictionary(
                    FlowBoundaryCondition.GetDescription, q => q);
            quantitiesListBox.Items.AddRange(quantities.Keys.OfType<object>().ToArray());
            for (var i = 0; i < quantitiesListBox.Items.Count; ++i)
            {
                quantitiesListBox.SetItemChecked(i, true);
            }

            dataTypes =
                new FlowBoundaryConditionEditorController().AllSupportedDataTypes.ToDictionary(
                    d => d.GetDescription(), d => d);
            dataTypesListBox.Items.AddRange(dataTypes.Keys.OfType<object>().ToArray());
            for (var i = 0; i < dataTypesListBox.Items.Count; ++i)
            {
                dataTypesListBox.SetItemChecked(i, true);
            }
        }

        public string Title { get; set; }

        public object Data { get; set; }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public virtual DelftDialogResult ShowModal()
        {
            saveFileDialog.DefaultExt = BcFile.Extension;
            saveFileDialog.Filter = new BcFileExporter().FileFilter;

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

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        public virtual void Configure(object item)
        {
            if (item is BcFileExporter bcFileExporter)
            {
                bcFileExporter.ExcludedQuantities =
                    quantities.Keys.Except(quantitiesListBox.CheckedItems.Cast<string>())
                              .Select(k => quantities[k])
                              .ToList();

                bcFileExporter.ExcludedDataTypes =
                    dataTypes.Keys.Except(dataTypesListBox.CheckedItems.Cast<string>())
                             .Select(k => dataTypes[k])
                             .ToList();

                bcFileExporter.WriteMode = (BcFile.WriteMode) exportModeComboBox.SelectedItem;

                bcFileExporter.FilePath = FilePath;
            }
        }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        protected string FilePath { get; set; }

        private void ExportModeComboBoxFormat(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is BcFile.WriteMode mode)
            {
                e.Value = mode.GetDescription();
            }
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}