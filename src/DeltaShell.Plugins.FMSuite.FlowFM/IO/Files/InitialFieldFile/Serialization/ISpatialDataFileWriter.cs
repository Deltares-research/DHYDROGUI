using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Serialization
{
    /// <summary>
    /// Interface for writing spatial operations to a data file.
    /// </summary>
    public interface ISpatialDataFileWriter
    {
        /// <summary>
        /// Write the spatial data to file in the specified directory.
        /// </summary>
        /// <param name="targetDirectory"> The target write directory. </param>
        /// <param name="initialFieldFileData"> The initial field file data. </param>
        /// <param name="modelDefinition"> The model definition containing the spatial data. </param>
        void Write(string targetDirectory, InitialFieldFileData initialFieldFileData, WaterFlowFMModelDefinition modelDefinition);
    }
}