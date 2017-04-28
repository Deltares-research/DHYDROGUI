using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Csv;
using DelftTools.Utils.Csv.Importer;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms
{
    public partial class FlowSeriesCsvMappingPage : UserControl, ICsvDataSelectionWizardPage
    {

        public FlowSeriesCsvMappingPage()
        {
            InitializeComponent();
            rbH.CheckedChanged += DataTypeRadioButtonCheckedChanged;
            rbHT.CheckedChanged += DataTypeRadioButtonCheckedChanged;
            rbQ.CheckedChanged += DataTypeRadioButtonCheckedChanged;
            rbQT.CheckedChanged += DataTypeRadioButtonCheckedChanged;
            rbQH.CheckedChanged += DataTypeRadioButtonCheckedChanged;
        }

        private bool _forBoundaryConditions;
        public bool ForBoundaryConditions
        {
            get { return _forBoundaryConditions; }
            set
            {
                _forBoundaryConditions = value;
                updateAvailableRadioButtons();
            }
        }

        private bool _batchMode;
        public bool BatchMode
        {
            get { return _batchMode; }
            set
            {
                _batchMode = value;
                updateAvailableRadioButtons();
            }
        }

        public bool CanFinish()
        {
            return CanDoNext();
        }

        public bool CanDoNext()
        {
            return !csvDataSelectionControl.HasErrors;
        }

        public bool CanDoPrevious()
        {
            return true;
        }

        public IDictionary<CsvRequiredField, CsvColumnInfo> FieldToColumnMapping
        {
            get { return csvDataSelectionControl.FieldToColumnMapping; }
        }

        public IEnumerable<CsvFilter> Filters
        {
            get { return csvDataSelectionControl.Filters; }
        }

        public void SetData(DataTable dataTable, IEnumerable<CsvRequiredField> requiredFields)
        {
            csvDataSelectionControl.SetData(dataTable, GetRequiredFields());
        }

        private void updateAvailableRadioButtons()
        {
            rbH.Visible = true;
            rbHT.Visible = true;
            rbQ.Visible = true;
            rbQH.Visible = true;
            rbQT.Visible = true; 
            
            if (!ForBoundaryConditions)
            {
                // These options are not valid for lateral sources. 
                rbH.Visible = false;
                rbHT.Visible = false;
            }
            if (!BatchMode)
            {
                // If importing for one BC or LS, importing only one constant value does not make sense. 
                rbQ.Visible = false;
                rbH.Visible = false; 
            }
        }


        private IEnumerable<CsvRequiredField> GetRequiredFields()
        {
            if (BatchMode)
                yield return new CsvRequiredField("Feature ID", typeof(string));
            if (rbHT.Checked || rbQT.Checked)
                yield return new CsvRequiredField("Time", typeof(DateTime));
            if (rbQH.Checked || rbQT.Checked || rbQ.Checked)
                yield return new CsvRequiredField("Discharge", typeof(double));
            if (rbH.Checked || rbHT.Checked || rbQH.Checked)
                yield return new CsvRequiredField("Water Level", typeof(double));
        }

        private void DataTypeRadioButtonCheckedChanged(object sender, EventArgs eventArgs)
        {
            if (ImportDataTypeChanged != null && ((RadioButton) sender).Checked)
                ImportDataTypeChanged(); 
        }

        public event Action ImportDataTypeChanged;

    }

}
