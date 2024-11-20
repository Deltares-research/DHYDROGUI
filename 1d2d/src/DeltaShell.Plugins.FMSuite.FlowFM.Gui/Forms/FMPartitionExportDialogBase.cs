using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Services;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    /// <summary>
    /// Provides a base class for partition export dialogs.
    /// </summary>
    public abstract partial class FMPartitionExportDialogBase : Form, IPartitionDialog, IView
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

        protected FMPartitionExportDialogBase()
        {
            InitializeComponent();
            InitializeComponentCustom();
        }

        /// <inheritdoc/>
        public string Title { get; set; }

        /// <inheritdoc/>
        public object Data { get; set; }

        /// <inheritdoc/>
        public Image Image { get; set; }

        /// <inheritdoc/>
        public ViewInfo ViewInfo { get; set; }

        /// <inheritdoc/>
        public int CoreCount => numDomains;

        /// <summary>
        /// Whether the solver selection should be visible.
        /// </summary>
        protected bool EnableSolverSelection { get; set; }

        /// <summary>
        /// Gets the selected solver type.
        /// </summary>
        protected int SolverType => (int)comboBoxSolverType.SelectedItem;

        private void InitializeComponentCustom()
        {
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
        
        private void PolFileSelectButtonOnClick(object sender, EventArgs eventArgs)
        {
            var fileDialogService = new FileDialogService();
            var fileDialogOptions = new FileDialogOptions { FileFilter = "Polygon files (*.pol)|*.pol" };

            string selectedFilePath = fileDialogService.ShowOpenFileDialog(fileDialogOptions);
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
        
        /// <inheritdoc/>
        public DelftDialogResult ShowPartitionModal()
        {
            return ShowDialog() == DialogResult.OK ? DelftDialogResult.OK : DelftDialogResult.Cancel;
        }

        /// <inheritdoc/>
        public DelftDialogResult ShowModal()
        {
            labelSolverType.Visible = EnableSolverSelection;
            comboBoxSolverType.Visible = EnableSolverSelection;

            if (ShowDialog() == DialogResult.OK)
            {
                return ShowExportPathDialog();
            }

            return DelftDialogResult.Cancel;
        }

        /// <inheritdoc/>
        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        /// <summary>
        /// Shows the dialog for selecting the partition export path.
        /// </summary>
        /// <returns>A <see cref="DelftDialogResult"/> enum value indicating the dialog result.</returns>
        protected abstract DelftDialogResult ShowExportPathDialog();

        /// <inheritdoc/>
        public void Configure(object model)
        {
            ConfigurePartition(model);
        }

        /// <inheritdoc/>
        public void ConfigurePartition(object model)
        {
            if (model is FMPartitionExporterBase exporter)
            {
                exporter.NumDomains = numDomains;
                exporter.IsContiguous = contiguousDomains;
                exporter.PolygonFile = polFilePath;

                Configure(exporter);
            }
        }

        /// <summary>
        /// Configures the given exporter before exporting the partition files.
        /// </summary>
        /// <param name="exporter">The exporter object to configure.</param>
        protected abstract void Configure(FMPartitionExporterBase exporter);

        /// <inheritdoc/>
        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }
    }
}