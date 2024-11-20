using System;
using System.Collections;
using System.Windows.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    public partial class ImportBranchFeatureDialog : Form
    {
        public double Tolerance { get; set; }

        public ImportBranchFeatureDialog()
        {
            InitializeComponent();
            textBox1.DataBindings.Add("Text", this, "Tolerance", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public IList DataSource
        {
            get { return selectBranchFeatureListBox.DataSource as IList; }
            set { selectBranchFeatureListBox.DataSource = value; }
        }

        public string DisplayMember
        {
            get { return selectBranchFeatureListBox.DisplayMember; }
            set { selectBranchFeatureListBox.DisplayMember = value; }
        }

        public object SelectedItem
        {
            get
            {
                return DataSource[selectBranchFeatureListBox.SelectedIndex];
            }
        }

        private void selectBranchFeatureListBox_DoubleClick(object sender, EventArgs e)
        {
            okButton.PerformClick();
        }
    }
}
