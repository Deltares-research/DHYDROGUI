using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public partial class FMPartitionExportDialog : Form, IPartitionDialog, IView
    {
        private enum ParallelSolver
        {
            Original = 0,
            PETSC = 6,
            ParallelCG = 7,
            PARMS = 8
        }

        private int numDomains;
        private bool contiguousDomains;
        private string polFilePath;
        private string filePath;

        public FMPartitionExportDialog()
        {
            InitializeComponent();
            comboBoxSolverType.Items.AddRange(Enum.GetValues(typeof(ParallelSolver)).OfType<object>().ToArray());
            comboBoxSolverType.SelectedIndex = 1;
            metisRadioButton.CheckedChanged += RadioButtonOnCheckedChanged;
            domainsTextBox.TextChanged += DomainsTextBoxOnTextChanged;
            contiguousDomainCheckBox.Visible = false;
            contiguousDomainCheckBox.CheckStateChanged += ContiguousDomainCheckBoxOnCheckStateChanged;
            polFileSelectButton.Click += PolFileSelectButtonOnClick;
            domainsTextBox.Text = "1";
            contiguousDomainCheckBox.Checked = true;
            metisRadioButton.Checked = true;
        }

        public bool EnableSolverSelection { get; set; }

        public string Extension { get; set; }

        public int CoreCount
        {
            get
            {
                return numDomains;
            }
        }

        public DelftDialogResult ShowPartitionModal()
        {
            return ShowDialog() == DialogResult.OK ? DelftDialogResult.OK : DelftDialogResult.Cancel;
        }

        public void ConfigurePartition(object model)
        {
            var exporter = model as FMPartitionExporterBase;
            if (exporter != null)
            {
                exporter.NumDomains = numDomains;
                exporter.IsContiguous = contiguousDomains;
                exporter.PolygonFile = polFilePath;
            }

            if (exporter is FMModelPartitionExporter && EnableSolverSelection)
            {
                var selectedItem = (ParallelSolver) comboBoxSolverType.SelectedItem;
                ((FMModelPartitionExporter) exporter).SolverType = (int) selectedItem;
            }
        }

        private void PolFileSelectButtonOnClick(object sender, EventArgs eventArgs)
        {
            string selectedFilePath = new FileDialogService().SelectFile("Polygon files (*.pol)|*.pol");
            if (selectedFilePath != null)
            {
                polFileTextBox.Text = selectedFilePath;
                okButton.Enabled = ValidateInput();
            }
        }

        private void DomainsTextBoxOnTextChanged(object sender, EventArgs eventArgs)
        {
            okButton.Enabled = ValidateInput();
        }

        private void ContiguousDomainCheckBoxOnCheckStateChanged(object sender, EventArgs eventArgs)
        {
            contiguousDomains = contiguousDomainCheckBox.Checked;
        }

        private void RadioButtonOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            UpdateVisibilities();
        }

        private void UpdateVisibilities()
        {
            domainsTextBox.Enabled = metisRadioButton.Checked;
            contiguousDomainCheckBox.Enabled = metisRadioButton.Checked;
            polFileSelectButton.Enabled = polFileRadioButton.Checked;
            okButton.Enabled = ValidateInput();
        }

        private bool ValidateInput()
        {
            if (domainsTextBox.Enabled)
            {
                polFilePath = null;

                if (!int.TryParse(domainsTextBox.Text, out numDomains))
                {
                    return false;
                }

                if (numDomains < 1)
                {
                    return false;
                }

                contiguousDomains = contiguousDomainCheckBox.Checked;
            }
            else
            {
                numDomains = 0;

                polFilePath = polFileTextBox.Text;

                if (polFilePath == null)
                {
                    return false;
                }

                if (!polFilePath.EndsWith(".pol"))
                {
                    return false;
                }

                if (!File.Exists(polFilePath))
                {
                    return false;
                }
            }

            return true;
        }

        #region IConfigureDialog

        public string Title { get; set; }

        public DelftDialogResult ShowModal()
        {
            labelSolverType.Visible = EnableSolverSelection;
            comboBoxSolverType.Visible = EnableSolverSelection;
            if (ShowDialog() == DialogResult.OK)
            {
                var saveFileDialog = new SaveFileDialog
                {
                    AddExtension = false,
                    Filter = Extension
                };
                if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    filePath = saveFileDialog.FileName;
                    return DelftDialogResult.OK;
                }
            }

            return DelftDialogResult.Cancel;
        }

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        public void Configure(object item)
        {
            ConfigurePartition(item);

            var exporter = item as FMPartitionExporterBase;
            if (exporter != null)
            {
                exporter.FilePath = filePath;
            }
        }

        #endregion

        #region IView

        public object Data { get; set; }
        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion
    }
}