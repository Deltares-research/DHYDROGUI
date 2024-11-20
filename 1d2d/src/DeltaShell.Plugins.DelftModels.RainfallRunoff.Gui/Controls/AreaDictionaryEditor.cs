using System.Windows.Forms;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public partial class AreaDictionaryEditor : UserControl
    {
        public AreaDictionaryEditor()
        {
            InitializeComponent();
        }

        public Panel ItemPanel
        {
            get { return itemPanel; }
        }

        public ErrorProvider ErrorProvider
        {
            get { return errorProvider; }
        }

        public TextBox TotalAreaText
        {
            get { return totalAreaTxt; }
        }

        public string UnitLabel
        {
            get { return unitLabel.Text; }
            set { unitLabel.Text = value; }
        }

        public string TotalAreaLabel
        {
            get { return totalAreaLabel.Text; }
            set { totalAreaLabel.Text = value; }
        }

        public int EmptyHeight
        {
            get { return bottomPanel.Height; }
        }
    }
}