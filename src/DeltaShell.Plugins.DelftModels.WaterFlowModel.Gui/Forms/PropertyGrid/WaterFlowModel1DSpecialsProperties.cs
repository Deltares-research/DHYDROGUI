using System.ComponentModel;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid {

    [DisplayName("Specials properties")]
    [Description("Specials properties")]
    public class WaterFlowModel1DSpecialsProperties
    {
        private WaterFlowModel1D data;

        public WaterFlowModel1DSpecialsProperties(WaterFlowModel1D data)
        {
            this.data = data;
        }


        [PropertyOrder(1)]
        [ResourcesCategory(typeof(Resources), "WaterFlowModel1DProperties_DesignFactorDlg_DisplayName")]
        [ResourcesDescription(typeof(Resources), "WaterFlowModel1DProperties_DesignFactorDlg_Description")]
        public double? DesignFactorDlg
        {
            get { return data.DesignFactorDlg; }
            set { data.DesignFactorDlg = value; }
        }
        public override string ToString()
        {
            return "";
        }
    }
}