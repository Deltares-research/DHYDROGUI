using System.IO;
using System.IO.Abstractions;
using DelftTools.Hydro;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO;
using Deltares.Infrastructure.Logging;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.InitialField;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField
{
    /// <summary>
    /// File writer for the initial field file (*.ini).
    /// </summary>
    public sealed class InitialFieldFileWriter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InitialFieldFileWriter));

        private readonly IFileSystem fileSystem;
        private readonly InitialFieldFileContext initialFieldFileContext;
        private readonly ISpatialDataFileWriter spatialDataFileWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitialFieldFileWriter"/> class.
        /// </summary>
        /// <param name="initialFieldFileContext">Storage for the original initial field data file names.</param>
        /// <param name="spatialDataFileWriter">The writer for spatial data.</param>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="initialFieldFileContext"/>, <paramref name="spatialDataFileWriter"/> or
        /// <paramref name="fileSystem"/> is <c>null</c>.
        /// </exception>
        public InitialFieldFileWriter(InitialFieldFileContext initialFieldFileContext, ISpatialDataFileWriter spatialDataFileWriter, IFileSystem fileSystem)
        {
            Ensure.NotNull(fileSystem, nameof(fileSystem));
            Ensure.NotNull(initialFieldFileContext, nameof(initialFieldFileContext));
            Ensure.NotNull(spatialDataFileWriter, nameof(spatialDataFileWriter));

            this.fileSystem = fileSystem;
            this.initialFieldFileContext = initialFieldFileContext;
            this.spatialDataFileWriter = spatialDataFileWriter;
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
            
            return !IsHydroNetworkEmpty(hydroNetwork) || ContainsSupportedSpatialOperations(modelDefinition);
        }

        private static bool IsHydroNetworkEmpty(IHydroNetwork hydroNetwork)
            => hydroNetwork.IsEdgesEmpty || hydroNetwork.IsVerticesEmpty;
        
        private static bool ContainsSupportedSpatialOperations(WaterFlowFMModelDefinition modelDefinition)
            => InitialFieldFileQuantities.ContainsSupportedSpatialOperations(modelDefinition);

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

            var logHandler = new LogHandler(Resources.writing_the_initial_field_file, log);

            InitialFieldFileData initialFieldFileData = CreateInitialFieldFileData(modelDefinition);

            CreateInitialFieldFileDirectory(filePath);

            RestoreInitialFieldDataFileNames(initialFieldFileData);

            WriteInitialFieldFile(filePath, initialFieldFileData);
            WriteSpatialDataFiles(parentFilePath, switchTo, initialFieldFileData, modelDefinition);

            logHandler.LogReport();
        }

        private InitialFieldFileData CreateInitialFieldFileData(WaterFlowFMModelDefinition modelDefinition)
        {
            var initialFieldFileDataFactory = new InitialFieldFileDataFactory();
            return initialFieldFileDataFactory.CreateFromModelDefinition(modelDefinition);
        }

        private void CreateInitialFieldFileDirectory(string filePath)
        {
            string directory = fileSystem.Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory))
            {
                fileSystem.CreateDirectoryIfNotExists(directory);
            }
        }

        private void RestoreInitialFieldDataFileNames(InitialFieldFileData initialFieldFileData)
        {
            foreach (InitialFieldData initialFieldData in initialFieldFileData.AllFields)
            {
                initialFieldFileContext.RestoreDataFileName(initialFieldData);
            }
        }

        private void WriteInitialFieldFile(string targetFile, InitialFieldFileData initialFieldFileData)
        {
            log.InfoFormat(Resources.Writing_initial_field_data_to_0_, targetFile);

            using (Stream iniStream = fileSystem.File.Open(targetFile, FileMode.Create))
            {
                var initialFieldFileFormatter = new InitialFieldFileFormatter();

                initialFieldFileFormatter.Format(initialFieldFileData, iniStream);
            }
        }

        private void WriteSpatialDataFiles(string parentFilePath, bool switchTo, InitialFieldFileData initialFieldFileData, WaterFlowFMModelDefinition modelDefinition)
        {
            string spatialDataDirectory = fileSystem.Path.GetDirectoryName(parentFilePath);
            spatialDataFileWriter.Write(spatialDataDirectory, switchTo, initialFieldFileData, modelDefinition);
        }
    }
}