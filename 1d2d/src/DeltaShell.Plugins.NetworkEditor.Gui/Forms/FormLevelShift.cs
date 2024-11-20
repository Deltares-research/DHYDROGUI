using System.Windows.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    public partial class FormLevelShift : Form
    {
        public double Shift { get; set; }

        public FormLevelShift()
        {
            InitializeComponent();
            textBoxShift.DataBindings.Add("Text", this, "Shift", false, DataSourceUpdateMode.OnPropertyChanged);
        }
    }
}
