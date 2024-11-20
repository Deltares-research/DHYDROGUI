using System;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput2D.SnappedFeatures
{
    /// <summary>
    /// <see cref="EstimatedSnappedFeatureData"/> acts as a Data Transfer Object
    /// used to construct a single snapped feature layer with the
    /// <see cref="EstimatedSnappedFeatureLayerSubProvider"/>.
    /// </summary>
    internal sealed class EstimatedSnappedFeatureData : IEquatable<EstimatedSnappedFeatureData>
    {
        /// <summary>
        /// Creates a new <see cref="EstimatedSnappedFeatureData"/>.
        /// </summary>
        /// <param name="model">The model to which the features belong.</param>
        /// <param name="featureType">The type of feature of the layer</param>
        public EstimatedSnappedFeatureData(IWaterFlowFMModel model, 
                                           EstimatedSnappedFeatureType featureType)
        {
            Model = model;
            FeatureType = featureType;
        }

        /// <summary>
        /// Gets the <see cref="IWaterFlowFMModel"/> of this
        /// <see cref="EstimatedSnappedFeatureData"/>
        /// </summary>
        public IWaterFlowFMModel Model { get; }

        /// <summary>
        /// Gets the type of feature of this <see cref="EstimatedSnappedFeatureData"/>.
        /// </summary>
        public EstimatedSnappedFeatureType FeatureType { get; }

        #region IEquatable
        // Because this class is used as a child layer object,
        // and objects of this class are not cached but generated anew each time,
        // we need value equality, and as such the IEquatable interface.
        public bool Equals(EstimatedSnappedFeatureData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Model, other.Model) && FeatureType == other.FeatureType;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is EstimatedSnappedFeatureData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Model != null ? Model.GetHashCode() : 0) * 397) ^ (int)FeatureType;
            }
        }
        #endregion
    }
}