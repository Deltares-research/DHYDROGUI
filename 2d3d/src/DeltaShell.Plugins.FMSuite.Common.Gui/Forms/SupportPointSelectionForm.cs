using System;
using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    public partial class SupportPointSelectionForm : Form
    {
        public SupportPointSelectionForm()
        {
            InitializeComponent();
        }

        public SupportPointMode SupportPointOperationMode { get; private set; }

        private SupportPointMode Mode
        {
            get
            {
                if (radioButtonSelectedSP.Checked)
                {
                    return SupportPointMode.SelectedPoint;
                }

                if (radioButtonActiveSP.Checked)
                {
                    return SupportPointMode.ActivePoints;
                }

                if (radioButtonInactiveSP.Checked)
                {
                    return SupportPointMode.InactivePoints;
                }

                if (radioButtonAllSP.Checked)
                {
                    return SupportPointMode.AllPoints;
                }

                return SupportPointMode.NoPoints;
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            SupportPointOperationMode = Mode;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            SupportPointOperationMode = SupportPointMode.NoPoints;
            Close();
        }
    }
}