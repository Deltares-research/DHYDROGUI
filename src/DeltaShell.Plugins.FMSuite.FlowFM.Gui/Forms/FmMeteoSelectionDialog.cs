using System;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;


namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    internal partial class FmMeteoSelectionDialog : Form
    {

        public FmMeteoSelectionDialog()
        {
            InitializeComponent();
            PrecipitationRadioButton.Checked = true;
        }

        public IFmMeteoField FmMeteoField { get; private set; }

        private void OkButtonClick(object sender, EventArgs e)
        {
            FmMeteoField = CreateMeteoField();
            if (FmMeteoField != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private IFmMeteoField CreateMeteoField()
        {
            if (PrecipitationRadioButton.Checked)
            {
                return Common.FeatureData.FmMeteoField.CreateMeteoPrecipitationSeries();
            }

            return null;
        }

        private void CancelButtonClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

    }
}
