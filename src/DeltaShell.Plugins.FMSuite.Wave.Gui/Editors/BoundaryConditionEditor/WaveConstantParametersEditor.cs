using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    public partial class WaveConstantParametersEditor : UserControl
    {
        public WaveConstantParametersEditor()
        {
            InitializeComponent();
        }

        private WaveBoundaryParameters data;
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
                waveHeightBox.DataBindings.Add(new Binding("Text", data, "Height", false, DataSourceUpdateMode.OnPropertyChanged));
                wavePeriodBox.DataBindings.Add(new Binding("Text", data, "Period", false, DataSourceUpdateMode.OnPropertyChanged));
                waveDirectionBox.DataBindings.Add(new Binding("Text", data, "Direction", false, DataSourceUpdateMode.OnPropertyChanged));
                waveSpreadingBox.DataBindings.Add(new Binding("Text", data, "Spreading", false, DataSourceUpdateMode.OnPropertyChanged));
            }
        }
    }
}
