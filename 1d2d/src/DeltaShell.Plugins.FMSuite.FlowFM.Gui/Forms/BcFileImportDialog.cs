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
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public partial class BcFileImportDialog : Form, IConfigureDialog, IView
    {
        protected readonly IDictionary<string, FlowBoundaryQuantityType> quantities;
        protected readonly IDictionary<string,BoundaryConditionDataType> dataTypes;
        
        public BcFileImportDialog()
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

            overwriteCheckBox.Checked = true;
        }

        protected string[] FilePaths { get; set; }

        public string Title { get; set; }

        public virtual DelftDialogResult ShowModal()
        {
            openFileDialog.FileName = string.Empty;
            openFileDialog.Filter = new BcFileImporter().FileFilter;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return DelftDialogResult.Cancel;
            }
            FilePaths = openFileDialog.FileNames;
            var result = ShowDialog() == DialogResult.OK ? DelftDialogResult.OK : DelftDialogResult.Cancel;
            if (result == DelftDialogResult.OK && deleteDataCheckBox.Checked &&
                quantitiesListBox.CheckedItems.Count*dataTypesListBox.CheckedItems.Count == 0)
            {
                var extraResult = MessageBox.Show(
                    "You are deleting data without importing any boundary condition data. Do you wish to continue?",
                    "Deleting data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return extraResult == DialogResult.Yes ? DelftDialogResult.OK : DelftDialogResult.Cancel;
            }
            return result;
        }

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        public virtual void Configure(object importer)
        {
            var bcFileImporter = importer as BcFileImporter;
            if (bcFileImporter != null)
            {
                bcFileImporter.FilePaths = FilePaths;

                bcFileImporter.ExcludedQuantities =
                    quantities.Keys.Except(quantitiesListBox.CheckedItems.Cast<string>())
                              .Select(k => quantities[k])
                              .ToList();

                bcFileImporter.ExcludedDataTypes =
                    dataTypes.Keys.Except(dataTypesListBox.CheckedItems.Cast<string>())
                             .Select(k => dataTypes[k])
                             .ToList();

                bcFileImporter.OverwriteExistingData = overwriteCheckBox.Checked;
                bcFileImporter.DeleteDataBeforeImport = deleteDataCheckBox.Checked;
            }
        }

        private void ButtonOkClick(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonCancelClick(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public object Data { get; set; }
        public Image Image { get; set; }
        public void EnsureVisible(object item)
        {
            
        }

        public ViewInfo ViewInfo { get; set; }
    }
}
