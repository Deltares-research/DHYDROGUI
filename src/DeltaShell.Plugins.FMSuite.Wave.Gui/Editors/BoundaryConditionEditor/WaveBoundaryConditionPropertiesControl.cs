using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Utils.Binding;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    public partial class WaveBoundaryConditionPropertiesControl : BoundaryConditionPropertiesControl
    {
        public WaveBoundaryConditionPropertiesControl()
        {
            InitializeComponent();

            uniformityBox.DataSource = EnumBindingHelper.ToList<WaveBoundaryConditionSpatialDefinitionType>();
            uniformityBox.DisplayMember = "Value";
            uniformityBox.ValueMember = "Key";
        }

        protected override IEnumerable<BoundaryConditionDataType> GetSupportedDataTypes(string variable)
        {
            return Controller.GetSupportedDataTypesForVariable(variable);
        }

        private WaveBoundaryCondition waveBoundaryCondition;

        public override IBoundaryCondition BoundaryCondition
        {
            protected get
            {
                return base.BoundaryCondition;
            }
            set
            {
                base.BoundaryCondition = value;
                waveBoundaryCondition = (WaveBoundaryCondition) BoundaryCondition;
                UpdateBinding();
            }
        }

        private void UpdateBinding()
        {
            uniformityBox.DataBindings.Clear();
            if (waveBoundaryCondition != null)
            {
                uniformityBox.DataBindings.Add(new Binding("SelectedValue", waveBoundaryCondition,
                                                           nameof(waveBoundaryCondition.SpatialDefinitionType),
                                                           false,
                                                           DataSourceUpdateMode.OnPropertyChanged));
            }
        }
    }
}