using System.Windows.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    public partial class SimpleProgress : Form
    {
        public SimpleProgress()
        {
            InitializeComponent();
            Canceled = false;
        }

        public int Progress
        {
            get { return ProgressBar.Value; }
            set
            {
                ProgressBar.Value = value;
                Application.DoEvents();
            }
        }

        public bool Canceled { get; set; }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            Canceled = true;
        }
    }
}

