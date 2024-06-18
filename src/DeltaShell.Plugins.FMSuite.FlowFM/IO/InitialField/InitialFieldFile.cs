using System.IO.Abstractions;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.InitialField;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField
{
    /// <summary>
    /// Provides methods for reading and writing initial field files (*.ini).
    /// </summary>
    public sealed class InitialFieldFile
    {
        private readonly InitialFieldFileReader initialFieldFileReader;
        private readonly InitialFieldFileWriter initialFieldFileWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitialFieldFile"/> class.
        /// </summary>
        public InitialFieldFile()
        {
            var fileSystem = new FileSystem();
            var context = new InitialFieldFileContext();
            var spatialDataWriter = new SpatialDataFileWriter();

            initialFieldFileReader = new InitialFieldFileReader(context, fileSystem);
            initialFieldFileWriter = new InitialFieldFileWriter(context, spatialDataWriter, fileSystem);
        }

        /// <summary>
        /// Reads the data from an initial field file and sets the data on the model definition.
        /// </summary>
        /// <param name="filePath"> Path to the initial field file. </param>
        /// <param name="parentFilePath">
        /// Path to which the data file references in the initial field file are relative to.
        /// In practice, can be either the MDU file or the initial field file.
        /// </param>
        /// <param name="modelDefinition">The model definition on which to set the data.</param>
        /// <returns>The parsed initial field file data.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="modelDefinition"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="filePath"/> or <paramref name="parentFilePath"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown when <paramref name="filePath"/> does not exist.
        /// </exception>
        public InitialFieldFileData Read(string filePath, string parentFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNullOrWhiteSpace(parentFilePath, nameof(parentFilePath));

            return initialFieldFileReader.Read(filePath, parentFilePath, modelDefinition);
        }

        /// <summary>
        /// Evaluates whether an initial field file should be written.
        /// The initial field file should only be written if there is at least one spatial operation or a channel.
        /// </summary>
        /// <param name="modelDefinition">The model definition of which to write the data.</param>
        /// <param name="hydroNetwork">The network of which to write the data.</param>
        /// <returns><c>true</c> if the initial field file should be written; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="modelDefinition"/> or <paramref name="hydroNetwork"/> is <c>null</c>.
        /// </exception>
        public bool ShouldWrite(WaterFlowFMModelDefinition modelDefinition, IHydroNetwork hydroNetwork)
        {
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));
            Ensure.NotNull(hydroNetwork, nameof(hydroNetwork));

            return initialFieldFileWriter.ShouldWrite(modelDefinition, hydroNetwork);
        }

        /// <summary>
        /// Writes the spatial operation data from the model definition to an initial field file.
        /// </summary>
        /// <param name="filePath"> Path to the initial field file. If the file already exists, it is overwritten. </param>
        /// <param name="parentFilePath">
        /// Path to which the data file references in the initial field file are relative to.
        /// In practice, can be either the MDU file or the initial field file.
        /// </param>
        /// <param name="switchTo">Whether the path of the referenced files should be switched to the new file location.</param>
        /// <param name="modelDefinition">The model definition from which to write the spatial operation data.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="modelDefinition"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="filePath"/> or <paramref name="parentFilePath"/> is <c>null</c> or empty.
        /// </exception>
        public void Write(string filePath, string parentFilePath, bool switchTo, WaterFlowFMModelDefinition modelDefinition)
        {
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNullOrWhiteSpace(parentFilePath, nameof(parentFilePath));

            initialFieldFileWriter.Write(filePath, parentFilePath, switchTo, modelDefinition);
        }
    }
}