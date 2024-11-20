using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.InitialField;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField
{
    /// <summary>
    /// Interface for writing spatial operations to a data file.
    /// </summary>
    public interface ISpatialDataFileWriter
    {
        /// <summary>
        /// Write the spatial data to file in the specified directory.
        /// </summary>
        /// <param name="directory"> The target write directory. </param>
        /// <param name="switchTo">Whether the spatial operation file path be switched to the new file location.</param>
        /// <param name="data"> The initial field file data. </param>
        /// <param name="definition"> The model definition containing the spatial data. </param>
        void Write(string directory, bool switchTo, InitialFieldFileData data, WaterFlowFMModelDefinition definition);
    }
}