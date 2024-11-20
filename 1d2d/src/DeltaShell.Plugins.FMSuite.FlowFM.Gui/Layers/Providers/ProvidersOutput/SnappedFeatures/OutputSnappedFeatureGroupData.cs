using System;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures
{
    /// <summary>
    /// <see cref="OutputSnappedFeatureGroupData"/> is the data transfer object
    /// used by the <see cref="OutputGroupLayerSubProvider"/> to create a
    /// group layer for a set of snapped features.
    /// </summary>
    internal sealed class OutputSnappedFeatureGroupData : IEquatable<OutputSnappedFeatureGroupData>
    {
        /// <summary>
        /// Creates a new <see cref="OutputSnappedFeatureGroupData"/> with
        /// the given model.
        /// </summary>
        /// <param name="model">The model containing the snapped features.</param>
        public OutputSnappedFeatureGroupData(IWaterFlowFMModel model)
        {
            Model = model;
        }

        /// <summary>
        /// Gets the <see cref="IWaterFlowFMModel"/> containing the snapped features.
        /// </summary>
        public IWaterFlowFMModel Model { get; }

        #region IEquatable
        // Because this class is used as a child layer object,
        // and objects of this class are not cached but generated anew each time,
        // we need value equality, and as such the IEquatable interface.
        public bool Equals(OutputSnappedFeatureGroupData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(this, other) || Equals(Model, other.Model);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is OutputSnappedFeatureGroupData other && Equals(other);
        }

        public override int GetHashCode() => Model != null ? Model.GetHashCode() : 0;
        #endregion
    }
}