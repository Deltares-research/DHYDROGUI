using System.ComponentModel;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using System.Windows.Forms;
using DelftTools.Utils;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Roughness properties")]
    [Description("Roughness properties")]
    public class WaterFlowModel1DRoughnessProperties
    {
        private WaterFlowModel1D data;
        public WaterFlowModel1DRoughnessProperties(WaterFlowModel1D data)
        {
            this.data = data;
        }

        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_ModelSettings")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_UseReverseRoughness_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_UseReverseRoughness_Description")]
        public bool UseReverseRoughness
        {
            get { return data.UseReverseRoughness; }
            set
            {
                // Pop a dialog with confirmation for turning off reverse roughness
                if (!value)
                {
                    var result = MessageBox.Show("This will remove all reverse roughness related data from your model. Continue?", "Remove reverse roughness", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No) return;
                }

                data.UseReverseRoughness = value;
            }
        }

        [PropertyOrder(2)]
        [DynamicReadOnly]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_Categories_ModelSettings")]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_UseReverseRoughnessInCalculation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_UseReverseRoughnessInCalculation_Description")]
        public bool UseReverseRoughnessInCalculation
        {
            get { return data.UseReverseRoughnessInCalculation; }
            set { data.UseReverseRoughnessInCalculation = value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool ValidateDynamicAttributes(string propertyName)
        {
            if (propertyName.Equals("UseReverseRoughnessInCalculation"))
            {
                return !data.UseReverseRoughness;
            }

            return false;
        }

        /* Override needed to avoid displaying the whole class name in the property grid */
        public override string ToString()
        {
            return "";
        }
    }
}