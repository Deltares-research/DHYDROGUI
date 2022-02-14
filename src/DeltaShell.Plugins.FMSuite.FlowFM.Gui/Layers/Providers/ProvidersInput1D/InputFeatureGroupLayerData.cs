using System;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersInput1D
{
    /// <summary>
    /// <see cref="InputFeatureGroupLayerData"/> defines
    /// a data transfer object used to create the different
    /// kind of group layers associated with the <see cref="FeatureGroupType"/>.
    /// </summary>
    internal sealed class InputFeatureGroupLayerData : IEquatable<InputFeatureGroupLayerData>
    { 
        /// <summary>
        /// Creates a new <see cref="InputFeatureGroupLayerData"/>.
        /// </summary>
        /// <param name="model">The model containing the feature group data.</param>
        /// <param name="featureGroupType">The type of feature.</param>
        public InputFeatureGroupLayerData(
            IWaterFlowFMModel model,
            FeatureType featureGroupType)
        {
            Model = model;
            FeatureGroupType = featureGroupType;
        }

        /// <summary>
        /// Gets the <see cref="IWaterFlowFMModel"/> containing the
        /// feature group data.
        /// </summary>
        public IWaterFlowFMModel Model { get; }

        /// <summary>
        /// Gets the <see cref="FeatureType"/> for which a layer
        /// should be created.
        /// </summary>
        public FeatureType FeatureGroupType { get; }

        #region IEquatable
        // Because this class is used as a child layer object,
        // and objects of this class are not cached but generated anew each time,
        // we need value equality, and as such the IEquatable interface.
        public bool Equals(InputFeatureGroupLayerData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Model, other.Model) && FeatureGroupType == other.FeatureGroupType;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is InputFeatureGroupLayerData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Model != null ? Model.GetHashCode() : 0) * 397) ^ (int)FeatureGroupType;
            }
        }
        #endregion
    }
}