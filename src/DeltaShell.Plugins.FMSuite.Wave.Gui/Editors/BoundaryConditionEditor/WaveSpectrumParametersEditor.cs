using System;
using System.Windows.Forms;
using DelftTools.Utils.Binding;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    public partial class WaveSpectralParametersEditor : UserControl
    {
        public WaveSpectralParametersEditor()
        {
            InitializeComponent();

            shapeTypeBox.DataSource = EnumBindingHelper.ToList<WaveSpectrumShapeType>();
            shapeTypeBox.DisplayMember = "Value";
            shapeTypeBox.ValueMember = "Key";
            shapeTypeBox.SelectedValueChanged += ShapeTypeBoxOnSelectedValueChanged;
            periodTypeBox.DataSource = EnumBindingHelper.ToList<WavePeriodType>();
            periodTypeBox.DisplayMember = "Value";
            periodTypeBox.ValueMember = "Key";
            spreadingTypeBox.DataSource = EnumBindingHelper.ToList<WaveDirectionalSpreadingType>();
            spreadingTypeBox.DisplayMember = "Value";
            spreadingTypeBox.ValueMember = "Key";

            UpdateInputFields();
        }

        private void ShapeTypeBoxOnSelectedValueChanged(object sender, EventArgs eventArgs)
        {
            UpdateInputFields();
        }

        private void UpdateInputFields()
        {
            peakEnhBox.Enabled = (WaveSpectrumShapeType) shapeTypeBox.SelectedValue == WaveSpectrumShapeType.Jonswap;
            peakEnhLabel.Enabled = peakEnhBox.Enabled;
            gaussSpreadBox.Enabled = (WaveSpectrumShapeType) shapeTypeBox.SelectedValue == WaveSpectrumShapeType.Gauss;
            gaussSpreadLabel.Enabled = gaussSpreadBox.Enabled;
        }

        private WaveBoundaryCondition data;
        public WaveBoundaryCondition Data
        {
            get => data;
            set
            {
                UnbindControls();
                data = value;

                if (data != null)
                {
                    BindControls();
                }
            }
        }

        private void BindControls()
        {
            // workaround for .net 4 issue
            // https://connect.microsoft.com/VisualStudio/feedback/details/683913/binding-to-a-nested-property-does-not-work-in-net-4#
            var bindingSource = new BindingSource(data, ""); 
            
            shapeTypeBox.DataBindings.Add(new Binding("SelectedValue", bindingSource, "SpectralData.ShapeType", false, DataSourceUpdateMode.OnPropertyChanged));
            periodTypeBox.DataBindings.Add(new Binding("SelectedValue", bindingSource, "SpectralData.PeriodType", false, DataSourceUpdateMode.OnPropertyChanged));
            spreadingTypeBox.DataBindings.Add(new Binding("SelectedValue", bindingSource, "DirectionalSpreadingType", false, DataSourceUpdateMode.OnPropertyChanged));
            peakEnhBox.DataBindings.Add(new Binding("Text", bindingSource, "SpectralData.PeakEnhancementFactor", false, DataSourceUpdateMode.OnPropertyChanged));
            gaussSpreadBox.DataBindings.Add(new Binding("Text", bindingSource, "SpectralData.GaussianSpreadingValue", false, DataSourceUpdateMode.OnPropertyChanged));
        }

        private void UnbindControls()
        {
            shapeTypeBox.DataBindings.Clear();
            periodTypeBox.DataBindings.Clear();
            spreadingTypeBox.DataBindings.Clear();
            peakEnhBox.DataBindings.Clear();
            gaussSpreadBox.DataBindings.Clear();
        }
    }
}
