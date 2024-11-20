using System.ComponentModel;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    public enum WaterQualityProcessType
    {
        [Description("Sobek process definition")]
        Sobek = 1,

        [Description("Custom process definition")]
        Custom
    }
}