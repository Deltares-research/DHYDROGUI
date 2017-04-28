using System.ComponentModel;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Advanced options")]
    [Description("Advanced options")]
    public class WaterFlowModel1DAdvancedOptions
    { 
        private WaterFlowModel1D data;

        public WaterFlowModel1DAdvancedOptions(WaterFlowModel1D data)
        {
            this.data = data;
        }

        [PropertyOrder(1)]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_Density_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_Density_Description")]
        public DensityType DensityType
        {
            get { return data.DensityType; }
            set { if (value != null)  data.DensityType = value; }
        }

        [PropertyOrder(2)]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_Latitude_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_Latitude_Description")]
        public double Latitude
        {
            get { return data.Latitude; }
            set { data.Latitude = value; }
        }

        [PropertyOrder(3)]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_Longitude_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_Longitude_Description")]

        public double Longitude
        {
            get { return data.Longitude; }
            set { data.Longitude = value; }
        }

        /* Override needed to avoid displaying the whole class name in the property grid */
        public override string ToString()
        {
            return "";
        }
    }
}