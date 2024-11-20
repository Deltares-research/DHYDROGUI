using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Common.DepthLayers
{
    public enum DepthLayerType
    {
        [Description("Single")]
        Single,
        [Description("Z")]
        Z,
        [Description("Sigma")]
        Sigma
    }
}