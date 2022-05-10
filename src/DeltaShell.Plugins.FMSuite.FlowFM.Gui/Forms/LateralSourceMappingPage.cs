using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Csv;
using DelftTools.Utils.Csv.Importer;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public partial class LateralSourceMappingPage : UserControl, ICsvDataSelectionWizardPage
    {
        public LateralSourceMappingPage()
        {
            InitializeComponent();
            rbH.CheckedChanged += DataTypeRadioButtonCheckedChanged;
            rbHT.CheckedChanged += DataTypeRadioButtonCheckedChanged;
            rbQ.CheckedChanged += DataTypeRadioButtonCheckedChanged;
            rbQT.CheckedChanged += DataTypeRadioButtonCheckedChanged;
            rbQH.CheckedChanged += DataTypeRadioButtonCheckedChanged;
        }

        private bool forBoundaryConditions;
        public bool ForBoundaryConditions
        {
            get => forBoundaryConditions;
            set
            {
                forBoundaryConditions = value;
                UpdateAvailableRadioButtons();
            }
        }

        private bool batchMode;
        public bool BatchMode
        {
            get => batchMode;
            set
            {
                batchMode = value;
                UpdateAvailableRadioButtons();
            }
        }

        public IDictionary<CsvRequiredField, CsvColumnInfo> FieldToColumnMapping => csvDataSelectionControl.FieldToColumnMapping;

        public IEnumerable<CsvFilter> Filters => csvDataSelectionControl.Filters;

        /// <summary>
        /// Fills in the CsvDataSelectionControl with data from the CSV file
        /// </summary>
        /// <param name="dataTable">The DataTable with the fetched CSV data</param>
        /// <param name="requiredFields">The fields that need to be filled with data</param>
        public void SetData(DataTable dataTable, IEnumerable<CsvRequiredField> requiredFields) =>
            csvDataSelectionControl.SetData(dataTable, GetRequiredFields());

        /// <summary>
        /// Activate/Deactivate the "Finish" button
        /// </summary>
        /// <returns>The value of <see cref="CanDoNext()"/></returns>
        public bool CanFinish() => CanDoNext();

        /// <summary>
        /// Activate/Deactivate the "Next" button, but only when the CsvSelectionControl doesn't have any errors
        /// </summary>
        /// <returns></returns>
        public bool CanDoNext() => !csvDataSelectionControl.HasErrors;

        /// <summary>
        /// Activate/Deactivate the "Previous" button
        /// </summary>
        /// <returns>True</returns>
        public bool CanDoPrevious() => true;

        public event EventHandler PageUpdated;

        private void UpdateAvailableRadioButtons()
        {
            rbH.Visible = ForBoundaryConditions && BatchMode;
            rbHT.Visible = ForBoundaryConditions;
            rbQ.Visible = BatchMode;
            rbQH.Visible = true;
            rbQT.Visible = true; 
        }

        private IEnumerable<CsvRequiredField> GetRequiredFields()
        {
            if (BatchMode)
                yield return new CsvRequiredField("Feature ID", typeof(string));
            if (rbHT.Checked || rbQT.Checked)
                yield return new CsvRequiredField("Time", typeof(DateTime));
            if (rbQH.Checked || rbQT.Checked || rbQ.Checked)
                yield return new CsvRequiredField("Discharge", typeof(double));
            if (rbQH.Checked || rbH.Checked || rbHT.Checked)
                yield return new CsvRequiredField("Water Level", typeof(double));
        }

        private void DataTypeRadioButtonCheckedChanged(object sender, EventArgs eventArgs)
        {
            if (((RadioButton) sender).Checked)
                ImportDataTypeChanged?.Invoke(); 
        }

        public event Action ImportDataTypeChanged;
    }
}   