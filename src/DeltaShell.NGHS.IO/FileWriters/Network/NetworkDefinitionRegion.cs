using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public static class NetworkDefinitionRegion
    {
        public static readonly ConfigurationSetting Id = new ConfigurationSetting(key: "id", description: "Unique network id");
        public static readonly ConfigurationSetting Name = new ConfigurationSetting(key: "name", description: "Given name in the user interface");

        public const string NodeHeader = "[Node]";
        public const string IniNodeHeader = "Node";
        public static readonly ConfigurationSetting X = new ConfigurationSetting(key: "x", description: "X-coordinate of the node (m)");
        public static readonly ConfigurationSetting Y = new ConfigurationSetting(key: "y", description: "Y-coordinate of the node (m)");

        public const string BranchHeader = "[Branch]";
        public const string IniBranchHeader = "Branch";
        public static readonly ConfigurationSetting FromNode = new ConfigurationSetting(key: "fromNode", description: "Node id of node at start of branch");
        public static readonly ConfigurationSetting ToNode = new ConfigurationSetting(key: "toNode", description: "Node id of node at end of branch");
        public static readonly ConfigurationSetting BranchOrder = new ConfigurationSetting(key: "order", description: "Order number to interpolate cross sections over branches");

        public static readonly ConfigurationSetting GridPointsCount = new ConfigurationSetting(key: "gridPointsCount", description: "Number of grid points on a branch");

        public static readonly ConfigurationSetting GridPointX = new ConfigurationSetting(key: "gridPointX", description: "X-coordinates of the grid points (m)");
        public static readonly ConfigurationSetting GridPointY = new ConfigurationSetting(key: "gridPointY", description: "Y-coordinates of the grid points (m)");

        public static readonly ConfigurationSetting GridPointOffsets = new ConfigurationSetting(key: "gridPointOffsets", description: "Chainage of the grid points on the branch (m)");

        public static readonly ConfigurationSetting GridPointNames = new ConfigurationSetting(key: "gridPointIds", description: "names of the grid points");
        
        public static readonly ConfigurationSetting Geometry = new ConfigurationSetting(key: "geometry", description: "(GUI ONLY) geometry of the branch");

        
    }
}