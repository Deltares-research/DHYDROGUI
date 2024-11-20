using System;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures
{
    /// <summary>
    /// <see cref="EstimatedSnappedFeatureGroupData"/> acts as a Data Transfer Object
    /// used to construct a group layer with the
    /// <see cref="EstimatedSnappedFeatureGroupLayerSubProvider"/>.
    /// </summary>
    internal sealed class EstimatedSnappedFeatureGroupData : IEquatable<EstimatedSnappedFeatureGroupData>
    {
        /// <summary>
        /// Creates a new <see cref="EstimatedSnappedFeatureGroupData"/>.
        /// </summary>
        /// <param name="model">The model to which the features belong.</param>
        public EstimatedSnappedFeatureGroupData(IWaterFlowFMModel model)
        {
            Model = model;
            SnapVersion = model.SnapVersion;
        }

        /// <summary>
        /// Gets the <see cref="IWaterFlowFMModel"/> of this
        /// <see cref="EstimatedSnappedFeatureGroupData"/>
        /// </summary>
        public IWaterFlowFMModel Model { get; }

        /// <summary>
        /// Gets the snap version at the time of creation.
        /// </summary>
        /// <remarks>
        /// Note that this might differ from the value stored in
        /// <see cref="IWaterFlowFMModel.SnapVersion"/>. This
        /// indicates that the snapped version has since changed, and
        /// this data is outdated.
        /// </remarks>
        public int SnapVersion { get; }

        #region IEquatable
        // Because this class is used as a child layer object,
        // and objects of this class are not cached but generated anew each time,
        // we need value equality, and as such the IEquatable interface.
        public bool Equals(EstimatedSnappedFeatureGroupData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Model, other.Model) && SnapVersion == other.SnapVersion;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is EstimatedSnappedFeatureGroupData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Model != null ? Model.GetHashCode() : 0) * 397) ^ SnapVersion;
            }
        }
        #endregion
    }
}