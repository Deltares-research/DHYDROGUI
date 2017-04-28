using DelftTools.Functions;
using DelftTools.Hydro;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    /// <summary>
    /// This class is used to store all roughness data per sectionType and branch but
    /// without the need of using a fully define HydroNetwork and RoughnessSection.
    /// </summary>
    public class RoughessCsvBranchData
    {
        public string BranchName { get; set; }
        public string SectionType { get; set; }
        public double Chainage { get; set; }
        public RoughnessType RoughnessType { get; set; }
        public IFunction ConstantRoughness { get; set; }
        public IFunction RoughnessFunctionOfH { get; set; }
        public IFunction RoughnessFunctionOfQ { get; set; }
    }
}