using System.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Extensions
{
    /// <summary>
    /// <see cref="SnappedFeaturesRelatedExtensions"/> defines extension
    /// methods related to the snapped features.
    /// </summary>
    public static class SnappedFeaturesRelatedExtensions
    {
        /// <summary>
        /// Checks whether this <see cref="IWaterFlowFMModel"/> has snapped output
        /// features.
        /// </summary>
        /// <param name="model">The model to verify.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="model"/> has snapped output features;
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool HasSnappedOutputFeatures(this IWaterFlowFMModel model) =>
            model != null &&
            model.WriteSnappedFeatures &&
            Directory.Exists(model.OutputSnappedFeaturesPath);
    }
}