using System;
using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public partial class GenerateEmbankmentsDialog : Form
    {
        public GenerateEmbankmentsDialog()
        {
            InitializeComponent();
        }

        #region Public return values

        public double ConstantDistance
        {
            get
            {
                double constantDistance;
                return Double.TryParse(constantDistanceTextBox.Text, out constantDistance) ? constantDistance : double.NaN;
            }
        }

        public bool CrossSectionBased
        {
            get { return radioButtonCrossSectionBased.Checked; }
        }

        public bool GenerateLeftEmbankments
        {
            get { return checkBoxGenerateLeftEmbankments.Checked; }
        }

        public bool GenerateRightEmbankments
        {
            get { return checkBoxGenerateRightEmbankments.Checked; }
        }

        public bool MergeAutomatically
        {
            get { return checkBoxAutomaticMerge.Checked; }
        }

        #endregion

        #region Event handlers

        private void OkButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        private void RadioButtonConstantDistanceCheckedChanged(object sender, EventArgs e)
        {
            constantDistanceTextBox.Enabled = radioButtonConstantDistance.Checked;
        }
    }
}
