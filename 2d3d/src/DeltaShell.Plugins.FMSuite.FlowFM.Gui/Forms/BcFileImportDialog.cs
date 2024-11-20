using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Services;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public partial class BcFileImportDialog : Form, IConfigureDialog, IView
    {
        protected readonly IDictionary<string, FlowBoundaryQuantityType> quantities;
        protected readonly IDictionary<string, BoundaryConditionDataType> dataTypes;

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

        public string Title { get; set; }

        public object Data { get; set; }
        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public virtual DelftDialogResult ShowModal()
        {
            var fileDialogService = new FileDialogService();
            var fileDialogOptions = new FileDialogOptions
            {
                FileFilter = new BcFileImporter().FileFilter,
                RestoreDirectory = true
            };
            
            string[] filePaths = fileDialogService.ShowOpenFilesDialog(fileDialogOptions).ToArray();
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

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        public virtual void Configure(object item)
        {
            var bcFileImporter = item as BcFileImporter;
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

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        protected string[] FilePaths { get; set; }

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