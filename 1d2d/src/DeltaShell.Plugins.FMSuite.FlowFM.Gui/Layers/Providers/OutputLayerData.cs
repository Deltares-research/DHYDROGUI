using System;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="OutputLayerData"/> is a data transfer object
    /// used to create the output layer containing the 1D and 2D data.
    /// </summary>
    internal sealed class OutputLayerData : IEquatable<OutputLayerData>
    {
        /// <summary>
        /// Creates a new <see cref="OutputLayerData"/>.
        /// </summary>
        /// <param name="model">The model containing the appropriate data.</param>
        public OutputLayerData(IWaterFlowFMModel model)
        { 
            Model = model; 
        }

        /// <summary>
        /// Gets the <see cref="IWaterFlowFMModel"/> of this <see cref="OutputLayerData"/>.
        /// </summary>
        public IWaterFlowFMModel Model { get; }


        #region IEquatable
        // Because this class is used as a child layer object,
        // and objects of this class are not cached but generated anew each time,
        // we need value equality, and as such the IEquatable interface.

        public bool Equals(OutputLayerData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Model, other.Model);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is OutputLayerData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Model != null ? Model.GetHashCode() : 0);
        }
        #endregion
    }
}