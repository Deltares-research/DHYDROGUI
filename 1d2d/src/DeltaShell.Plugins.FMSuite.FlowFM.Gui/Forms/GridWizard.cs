using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public partial class GridWizard : Form
    {
        public GridWizard()
        {
            InitializeComponent();
        }

        public double SupportPointDistance
        {
            get { return Double.Parse(tbSupportPointDistance.Text.Replace(',', '.'), CultureInfo.InvariantCulture); }
        }

        public double MinimumSupportPointDistance
        {
            get { return Double.Parse(tbMinimumSupportPointDistance.Text.Replace(',', '.'), CultureInfo.InvariantCulture); }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void tbSupportPointDistance_Validating(object sender, CancelEventArgs e)
        {
            var entry = tbSupportPointDistance.Text;
            if (String.IsNullOrEmpty(entry))
            {
                e.Cancel = true;
                tbSupportPointDistance.Select(0, tbSupportPointDistance.Text.Length);
                errorProvider1.SetError(tbSupportPointDistance, "specify value");
            }
            var value = 0.0d;
            var result = Double.TryParse(entry, out value);
            if (!result || value < 0)
            {
                e.Cancel = true;
                tbSupportPointDistance.Select(0, tbSupportPointDistance.Text.Length);
                errorProvider1.SetError(tbSupportPointDistance, "invalid value");
            }
            else
            {
                errorProvider1.Clear();
            }
        }

        private void tbMinimumSupportPointDistance_Validating(object sender, CancelEventArgs e)
        {
            var entry = tbMinimumSupportPointDistance.Text;
            if (String.IsNullOrEmpty(entry))
            {
                e.Cancel = true;
                tbMinimumSupportPointDistance.Select(0, tbMinimumSupportPointDistance.Text.Length);
                errorProvider1.SetError(tbMinimumSupportPointDistance, "specify value");
            }
            var value = 0.0d;
            var result = Double.TryParse(entry, out value);
            if (!result || value < 0)
            {
                e.Cancel = true;
                tbSupportPointDistance.Select(0, tbMinimumSupportPointDistance.Text.Length);
                errorProvider1.SetError(tbMinimumSupportPointDistance, "invalid value");
            }
            else
            {
                errorProvider1.Clear();
            }
        }
    }
}

