using DelftTools.Hydro;
using DelftTools.Hydro.Structures;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public class BranchProperties
    {
        public string Name { get; set; }
        public BranchFile.BranchType BranchType { get; set; }
        public bool IsCustomLength { get; set; }
        public SewerConnectionWaterType WaterType { get; set; }
        public SewerProfileMapping.SewerProfileMaterial Material { get; set; }
        public string SourceCompartmentName { get; set; }
        public string TargetCompartmentName { get; set; }
    }
}