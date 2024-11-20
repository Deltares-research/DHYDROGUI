using System;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures
{
    /// <summary>
    /// <see cref="OutputSnappedFeatureData"/> is a data transfer object
    /// used to define the data for a single output snapped feature,
    /// utilized in the <see cref="OutputSnappedFeatureLayerSubProvider"/>.
    /// </summary>
    internal sealed class OutputSnappedFeatureData : IEquatable<OutputSnappedFeatureData>
    {
        /// <summary>
        /// Creates a new <see cref="OutputSnappedFeatureData"/> with the
        /// given parameters.
        /// </summary>
        /// <param name="model">The model to which the feature belongs.</param>
        /// <param name="layerName">The layer name.</param>
        /// <param name="snappedFeatureDataPath">The path to the feature's shp file.</param>
        public OutputSnappedFeatureData(IWaterFlowFMModel model,
                                        string layerName,
                                        string snappedFeatureDataPath)
        {
            Model = model;
            LayerName = layerName;
            SnappedFeatureDataPath = snappedFeatureDataPath;
        }

        /// <summary>
        /// Gets the <see cref="IWaterFlowFMModel"/> to which this feature belongs.
        /// </summary>
        public IWaterFlowFMModel Model { get; }

        /// <summary>
        /// Gets the layer name of the corresponding layer.
        /// </summary>
        public string LayerName { get; }

        /// <summary>
        /// Gets the path to the .shp file that contains the relevant snapped feature.
        /// </summary>
        public string SnappedFeatureDataPath { get; }

        #region IEquatable
        // Because this class is used as a child layer object,
        // and objects of this class are not cached but generated anew each time,
        // we need value equality, and as such the IEquatable interface.
        public bool Equals(OutputSnappedFeatureData other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Model, other.Model) && LayerName == other.LayerName && SnappedFeatureDataPath == other.SnappedFeatureDataPath;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is OutputSnappedFeatureData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Model != null ? Model.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LayerName != null ? LayerName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SnappedFeatureDataPath != null ? SnappedFeatureDataPath.GetHashCode() : 0);
                return hashCode;
            }
        }
        #endregion
    }
}