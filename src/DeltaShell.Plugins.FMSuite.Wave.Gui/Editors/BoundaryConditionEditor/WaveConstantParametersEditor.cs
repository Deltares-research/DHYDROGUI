using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    public partial class WaveConstantParametersEditor : UserControl
    {
        private WaveBoundaryParameters data;

        public WaveConstantParametersEditor()
        {
            InitializeComponent();
        }

        public WaveBoundaryParameters Data
        {
            get => data;
            set
            {
                data = value;

                UpdateBindings();
                Visible = data != null;
            }
        }

        private void UpdateBindings()
        {
            waveHeightBox.DataBindings.Clear();
            wavePeriodBox.DataBindings.Clear();
            waveDirectionBox.DataBindings.Clear();
            waveSpreadingBox.DataBindings.Clear();

            if (data != null)
            {
                waveHeightBox.DataBindings.Add(new Binding("Text", data, "Height"));
                wavePeriodBox.DataBindings.Add(new Binding("Text", data, "Period"));
                waveDirectionBox.DataBindings.Add(new Binding("Text", data, "Direction"));
                waveSpreadingBox.DataBindings.Add(new Binding("Text", data, "Spreading"));
            }
        }
    }
}