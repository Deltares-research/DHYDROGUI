using System.ComponentModel;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    [DisplayName("Sediment properties")]
    [Description("Sediment properties")]
    public class WaterFlowModel1DSedimentProperties
    {
        private readonly WaterFlowModel1D data;

        public WaterFlowModel1DSedimentProperties(WaterFlowModel1D data)
        {
            this.data = data;
        }
        
        [PropertyOrder(1)]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_D50_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_D50_Description")]
        public double D50
        {
            get { return data.D50; }
            set { data.D50 = value; }
        }

        [PropertyOrder(2)]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_D90_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_D90_Description")]
        public double D90
        {
            get { return data.D90; }
            set { data.D90 = value; }
        }

        [PropertyOrder(3)]
        [ResourcesDisplayName(typeof(Resources), "WaterFlowModel1DProperties_DepthUsedForSediment_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_DepthUsedForSediment_Description")]
        public double DepthUsedForSediment
        {
            get { return data.DepthUsedForSediment; }
            set { data.DepthUsedForSediment = value; }
        }

        // Override needed to avoid displaying the whole class name in the property grid
        public override string ToString()
        {
            return "";
        }
    }
}