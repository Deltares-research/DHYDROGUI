using DeltaShell.Plugins.FMSuite.FlowFM.Model;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    /// <summary>
    /// This class contains constants which represent all categories which can be returned during execution of
    /// <see cref="WaterFlowFMModel.GetFeatureCategory"/> method.
    /// </summary>
    public static class KnownFeatureCategories
    {
        public const string Weirs = "weirs";
        public const string GeneralStructures = "generalstructures";
        public const string Gates = "gates";
        public const string ObservationPoints = "observations";
        public const string ObservationCrossSections = "crosssections";
        public const string Pumps = "pumps";
    }
}