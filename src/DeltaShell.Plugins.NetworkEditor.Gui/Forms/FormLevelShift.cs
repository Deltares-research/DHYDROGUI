using System.Windows.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    public partial class FormLevelShift : Form
    {
        public FormLevelShift()
        {
            InitializeComponent();
            textBoxShift.DataBindings.Add("Text", this, "Shift", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public double Shift { get; set; }
    }
}