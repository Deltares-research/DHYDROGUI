using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    /// <summary>
    /// a simple class that represents a single for a csv file that is used to store 
    /// roughness settings.
    /// </summary>
    public class RoughnessCsvRecord
    {
        public string BranchName { get; set; }
        public double Chainage { get; set; }
        public RoughnessType RoughnessType { get; set; }
        public string SectionType { get; set; }
        public RoughnessFunction RoughnessFunction { get; set; }
        public InterpolationType InterpolationType { get; set; }
        public bool NegativeIsPositive { get; set; }
        public double PositiveConstant {get; set; }
        public double PositiveQ {get; set; }
        public double PositiveQRoughness {get; set; }
        public double PositiveH {get; set; }
        public double PositiveHRoughness {get; set; }

        public double NegativeConstant {get; set; }
        public double NegativeQ { get; set; }
        public double NegativeQRoughness { get; set; }
        public double NegativeH { get; set; }
        public double NegativeHRoughness { get; set; }
    }
}