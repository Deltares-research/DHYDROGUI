using System.Windows.Forms;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    public partial class FormPasteBranchFeature : Form
    {
        public IBranch Branch { get; set; }
        public double Chainage { get; set; }
        public double Shift { get; set; }
        public string Title { get; set; }

        public FormPasteBranchFeature()
        {
            InitializeComponent();
            textBoxShift.DataBindings.Add("Text", this, "Shift", false, DataSourceUpdateMode.OnPropertyChanged);
            textBoxChainage.DataBindings.Add("Text", this, "Chainage", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void FormPasteCrossSection_Load(object sender, System.EventArgs e)
        {
            labelPaste.Text = Title;
            labelChainage.Text = string.Format("New chainage [0-{0:f2}] m.", Branch.Length);
        }

        private void textBoxChainage_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double value;
            bool valid = double.TryParse(textBoxChainage.Text, out value);
            if ((valid) && (value >= 0) && (value <= Branch.Length))
            {
                errorProvider1.Clear();
                return;
            }
            errorProvider1.SetError(textBoxChainage, string.Format("Chainage must be in range [0-{0:f2}] m.", Branch.Length));
            e.Cancel = true;
        }
    }
}
