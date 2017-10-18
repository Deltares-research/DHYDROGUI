using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;
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
        [DynamicVisible]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_SalinityNode_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_SalinityNode_Description")]
        [TypeConverter(typeof(SelectSalinityEstuaryMouthNodeIdTypeConverter))]
        public string SalinityEstuaryMouthNodeId
        {
            get { return data.SalinityEstuaryMouthNodeId; }
            set { data.SalinityEstuaryMouthNodeId = value; }
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

            return false;
        }

        [DynamicVisibleValidationMethod]
        public bool ValidateDynamicVisibleAttributes(string propertyName)
        {
            if (propertyName == nameof(this.SalinityEstuaryMouthNodeId))
            {
                return DispersionFormulationType == DispersionFormulationType.KuijperVanRijnPrismatic;
            }

            return false;
        }

        public IEnumerable<string> GetSalinityEstuaryMouthNodeIds()
        {
            return data.Network.HydroNodes
                .Where(n => n.IsValidSalinityEstuaryMouthNodeId())
                .Select(c => c.Name);
        }

        /* Override needed to avoid displaying the whole class name in the property grid */
        public override string ToString()
        {
            return "";
        }
    }
}