using System;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="InputLayerData"/> is a data transfer object
    /// used to create the different input layers by the 1D and 2D
    /// sub layer providers.
    /// </summary>
    internal sealed class InputLayerData : IEquatable<InputLayerData>
    {
        /// <summary>
        /// Creates a new <see cref="InputLayerData"/>.
        /// </summary>
        /// <param name="model">The model containing the appropriate data.</param>
        /// <param name="dimension">The dimension of the data.</param>
        public InputLayerData(IWaterFlowFMModel model, 
                              LayerDataDimension dimension)
        { 
            Model = model;
            Dimension = dimension;
        } 

        /// <summary>
        /// Gets the <see cref="IWaterFlowFMModel"/> of this <see cref="InputLayerData"/>.
        /// </summary>
        public IWaterFlowFMModel Model { get; }

        /// <summary>
        /// Gets the dimension of this <see cref="InputLayerData"/>.
        /// </summary>
        public LayerDataDimension Dimension { get; }

        #region IEquatable
        // Because this class is used as a child layer object,
        // and objects of this class are not cached but generated anew each time,
        // we need value equality, and as such the IEquatable interface.
        public bool Equals(InputLayerData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Model, other.Model) && Dimension == other.Dimension;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is InputLayerData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Model != null ? Model.GetHashCode() : 0) * 397) ^ (int)Dimension;
            }
        }
        #endregion
    }
}