using DeltaShell.Plugins.FMSuite.FlowFM.WaterFlowFMModel;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    /// <summary>
    /// This class contains constants which represent all categories which can be returned during execution of
    /// <see cref="WaterFlowFMModel.GetFeatureCategory" /> method.
    /// </summary>
    public static class KnownFeatureCategories
    {
        public const string Weirs = "weirs";
        public const string GeneralStructures = "generalstructures";
        public const string Gates = "gates";
        public const string Observations = "observations";
        public const string CrossSections = "crosssections";
        public const string Pumps = "pumps";
    }
}