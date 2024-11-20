using DelftTools.Hydro;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public class HydroModelProjectTemplateSettings : ModelSettings
    {
        public bool UseRR { get; set; } = true;
        public bool UseFlowFM { get; set; } = true;
        public bool UseRTC { get; set; } = false;

    }
}