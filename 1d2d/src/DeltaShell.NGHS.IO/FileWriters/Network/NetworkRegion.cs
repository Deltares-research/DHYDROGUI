using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public static class NetworkRegion
    {
        //branch properties
        public const string BranchIniHeader = "Branch";
        public static readonly ConfigurationSetting BranchId = new ConfigurationSetting(key: "name", description: "Unique branch id");
        public static readonly ConfigurationSetting BranchName = new ConfigurationSetting(key: "description", description: "Long name in the user interface");
        public static readonly ConfigurationSetting BranchType = new ConfigurationSetting(key: "branchType", description: "Channel = 0, SewerConnection = 1, Pipe = 2");
        public static readonly ConfigurationSetting BranchWaterType = new ConfigurationSetting(key: "waterType", description: "0 = None, 1 = StormWater, 2 = DryWater, 3 = Combined");
        public static readonly ConfigurationSetting BranchMaterial = new ConfigurationSetting(key: "material", description: "0 = Unknown, 1 = Concrete, 2 = CastIron, 3 = StoneWare, 4 = Hdpe, 5 = Masonry, 6 = SheetMetal, 7 = Polyester, 8 = Polyvinylchlorid, 9 = Steel");
        public static readonly ConfigurationSetting IsLengthCustom = new ConfigurationSetting(key: "isLengthCustom", description: "branch length specified by user");
        public static readonly ConfigurationSetting SourceCompartmentName = new ConfigurationSetting(key: "sourceCompartmentName", description: "Source compartment name this sewer connection is beginning");
        public static readonly ConfigurationSetting TargetCompartmentName = new ConfigurationSetting(key: "targetCompartmentName", description: "Target compartment name this sewer connection is ending");

    }
    
}
