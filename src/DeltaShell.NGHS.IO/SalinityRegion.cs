using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO
{
    public static class SalinityRegion
    {
        public const string MouthHeader = "Mouth";
        public static readonly ConfigurationSetting NodeId = new ConfigurationSetting(key: "nodeId", description: "#Node id of the node that is chosen to be the estuary mouth");
    }
}
