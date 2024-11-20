using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using DelftTools.Hydro;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    public partial class ComputationalGridDialog : Form
    {
        private bool useOpacity = true;

        public ComputationalGridDialog()
        {
            HydroNetworks = new List<IHydroNetwork>();
            AllBranches = true;
            OverwriteSegments = true;
            UseFixedLength = true;
            FixedLength = 100.0;
            GridAtCrossSection = false;
            GridAtLateralSource = false;
            GridAtStructure = true;
            StructureDistance = 0.5;
            MinimumCellLength = 0.5;
            AllowSelectionCheck = false;
            InitializeComponent();
        }

        public IList<IHydroNetwork> HydroNetworks { get; set; }

        public int SelectedNetwork { get; set; }

        public bool AllBranches { get; set; }

        public bool AllowSelectionCheck { get; set; }

        public bool OverwriteSegments { get; set; }

        public bool UseFixedLength { get; set; }

        public double FixedLength { get; set; }

        public bool GridAtCrossSection { get; set; }

        public bool GridAtLateralSource { get; set; }

        public bool GridAtStructure { get; set; }

        public double StructureDistance { get; set; }

        public double MinimumCellLength { get; set; }

        public bool NewDiscretization { get; private set; }

        public IDiscretization UpdateDiscretization { get; set; }

        public IDiscretization SourceDiscretization { get; private set; }
        
        public bool Erase { get; set; }

        /// <summary>
        /// Make dialog transparent when moving and when losing focus
        /// </summary>
        public bool UseOpacity
        {
            get { return useOpacity; }
            set { useOpacity = value; }
        }

        private void CalculationGridWizard_Load(object sender, EventArgs e)
        {
            SelectedNetwork = 0;

            // databinding does not work as expected for radio buttons
            radioSelectedBranches.Checked = !AllBranches;
            radioOverwrite.Checked = OverwriteSegments;

            checkBoxPreferred.DataBindings.Add(new Binding("Checked", this, "UseFixedLength"));

            // data binding has the annoying side effect that it will reset the textbox to its original 
            // (binded) value when it encounters an error.
            textBoxPreferredLength.Text = FixedLength.ToString();
            textBoxPreferredLength.Enabled = checkBoxPreferred.Checked;
            textBoxStructureDistance.Text = StructureDistance.ToString();
            textBoxMinimumDistance.Text = MinimumCellLength.ToString();

            checkBoxCrossSection.DataBindings.Add(new Binding("Checked", this, "GridAtCrossSection"));
            checkBoxLaterals.DataBindings.Add(new Binding("Checked", this, "GridAtLateralSource"));

            checkBoxStructure.DataBindings.Add(new Binding("Checked", this, "GridAtStructure"));
            checkBoxStructure.Enabled = true; // structures should always be at gridcell boundary
            checkBoxNone.DataBindings.Add(new Binding("Checked", this, "Erase"));

            groupBoxSelection.Enabled = AllowSelectionCheck;
        }

        private void CalculationGridWizard_Move(object sender, EventArgs e)
        {
            if (!useOpacity) return;
            Opacity = Math.Max(0.4, Opacity - 0.01);
        }

        private void CalculationGridWizard_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (!useOpacity) return;
            Opacity = 0.99;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if ((textBoxPreferredLength.Enabled) && (errorProvider1.GetError(textBoxPreferredLength).Length > 0))
                return;
            if ((textBoxStructureDistance.Enabled) && (errorProvider1.GetError(textBoxStructureDistance).Length > 0))
                return;
            if ((textBoxMinimumDistance.Enabled) && (errorProvider1.GetError(textBoxMinimumDistance).Length > 0))
                return;
            AllBranches = radioAllBranches.Checked;
            OverwriteSegments = radioOverwrite.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void checkBoxNone_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxStructure.Enabled = !checkBoxNone.Checked;
            checkBoxCrossSection.Enabled = !checkBoxNone.Checked;
            checkBoxCrossSection.Enabled = !checkBoxNone.Checked;
            checkBoxPreferred.Enabled = !checkBoxNone.Checked;
            if (checkBoxPreferred.Checked)
            {
                textBoxPreferredLength.Enabled = !checkBoxNone.Checked;
                if (!textBoxPreferredLength.Enabled)
                    errorProvider1.Clear();
            }
            
        }

        private void textBoxFixed_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double result;
            if (CheckDoubleTextBox(checkBoxPreferred, textBoxPreferredLength, out result))
            {
                FixedLength = result;
            }
        }

        private bool CheckDoubleTextBox(CheckBox checkBox, TextBox textBox, out double result)
        {
            result = 0;
            if ((checkBox.Enabled) && (checkBox.Checked))
            {
                double.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                if (result <= 0.0)
                {
                    errorProvider1.SetError(textBox, "Please enter a positive number.");
                }
                else
                {
                    errorProvider1.SetError(textBox, "");
                    return true;
                }
            }
            else
            {
                errorProvider1.SetError(textBox, "");
            }
            return false;
        }

        private void checkBoxFixed_CheckedChanged(object sender, EventArgs e)
        {
            textBoxPreferredLength.Enabled = checkBoxPreferred.Checked;
        }

        private void tbStructureBefore_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double result;
            if (CheckDoubleTextBox(checkBoxStructure, textBoxStructureDistance, out result))
            {
                StructureDistance = result;
            }
        }

        private void tbStructureAfter_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double result;
            if (CheckDoubleTextBox(checkBoxStructure, textBoxMinimumDistance, out result))
            {
                MinimumCellLength = result;
            }
        }

        private void checkBoxStructure_CheckedChanged(object sender, EventArgs e)
        {
            textBoxStructureDistance.Enabled = checkBoxStructure.Checked;
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
    }
}