using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Salinity properties")]
    [Description("Salinity properties")]
    public class WaterFlowModel1DSalinityProperties
    {
        private WaterFlowModel1D data;

        public WaterFlowModel1DSalinityProperties(WaterFlowModel1D data)
        {
            this.data = data;
        }

        [PropertyOrder(1)]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_UseSalt_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_UseSalt_Description")]
        public bool UseSalt
        {
            get { return data.UseSalt; }
            set
            {
                // Pop a dialog with confirmation for turning off salt
                if (!value)
                {
                    var result = MessageBox.Show("This will remove all salt related data from your model. Continue?", "Remove salt", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No) return;
                }

                data.UseSalt = value;
            }
        }

        [PropertyOrder(2)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_UseSaltInCalculation_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_UseSaltInCalculation_Description")]
        public bool UseSaltInCalculation
        {
            get { return data.UseSaltInCalculation; }
            set { data.UseSaltInCalculation = value; }
        }

        [PropertyOrder(3)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_DispersionFormulationsType_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_DispersionFormulationsType_Description")]

        public DispersionFormulationType DispersionFormulationType
        {
            get { return data.DispersionFormulationType; }
            set { data.DispersionFormulationType = value; }
        }

        [PropertyOrder(4)]
        [DynamicReadOnly]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_SalinityPath_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_SalinityPath_Description")]
        [Editor(typeof(PathEditor), typeof(UITypeEditor))]
        public string SalinityPath
        {
            get { return data.SalinityPath; }
            set { if (value != null) data.SalinityPath = value; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool ValidateDynamicAttributes(string propertyName)
        {
            if (propertyName.Equals("UseSaltInCalculation"))
            {
                return !data.UseSalt;
            }

            if (propertyName.Equals("DispersionFormulationType"))
            {
                return data.DispersionCoverage == null || !data.UseSalt;
            }

            if (propertyName.Equals("SalinityPath"))
            {
                return data.DispersionFormulationType == DispersionFormulationType.Constant;
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