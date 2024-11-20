using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Logging;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.InitialField;
using log4net;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField
{
    /// <summary>
    /// File reader for the initial field file (*.ini).
    /// </summary>
    public sealed class InitialFieldFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InitialFieldFileReader));

        // Dictionary containing the quantities written to the file, with their corresponding name in our domain model.
        private static readonly IDictionary<InitialFieldQuantity, string> quantities = new Dictionary<InitialFieldQuantity, string>
        {
            { InitialFieldQuantity.BedLevel, WaterFlowFMModelDefinition.BathymetryDataItemName },
            { InitialFieldQuantity.WaterLevel, WaterFlowFMModelDefinition.InitialWaterLevelDataItemName },
            { InitialFieldQuantity.FrictionCoefficient, WaterFlowFMModelDefinition.RoughnessDataItemName }
        };

        private readonly IFileSystem fileSystem;
        private readonly ILogHandler logHandler;
        private readonly InitialFieldFileContext initialFieldFileContext;
        private readonly InitialFieldFileParser initialFieldFileParser;
        private readonly InitialFieldDataValidator initialFieldDataValidator;
        private readonly SpatialOperationFactory spatialOperationFactory;

        /// <summary>
        /// Initialize a new instance of the <see cref="InitialFieldFileReader"/> class.
        /// </summary>
        /// <param name="initialFieldFileContext">Storage for the original initial field data file names.</param>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="initialFieldFileContext"/> or <paramref name="fileSystem"/> is <c>null</c>.
        /// </exception>
        public InitialFieldFileReader(InitialFieldFileContext initialFieldFileContext, IFileSystem fileSystem)
        {
            Ensure.NotNull(initialFieldFileContext, nameof(initialFieldFileContext));
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.initialFieldFileContext = initialFieldFileContext;
            this.fileSystem = fileSystem;

            logHandler = new LogHandler(Resources.reading_the_initial_field_file, log);

            initialFieldDataValidator = new InitialFieldDataValidator(logHandler, fileSystem) { FieldValidator = new InitialFieldDataConfigValidator() };
            initialFieldFileParser = new InitialFieldFileParser(logHandler);
            spatialOperationFactory = new SpatialOperationFactory(fileSystem);
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
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="modelDefinition"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="filePath"/> or <paramref name="parentFilePath"/> is <c>null</c> or empty.
        /// </exception>
        public void Read(string filePath, string parentFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNullOrWhiteSpace(parentFilePath, nameof(parentFilePath));

            if (!fileSystem.File.Exists(filePath))
            {
                log.ErrorFormat(Resources.Initial_field_file_does_not_exist_0_, filePath);
                return;
            }

            InitialFieldFileData initialFieldFileData = ParseInitialFieldFile(filePath);

            SetSpatialDataDirectory(initialFieldFileData, parentFilePath);
            ClearStoredInitialFieldDataFiles();

            foreach (InitialFieldData initialFieldData in initialFieldFileData.AllFields.Where(IsValidInitialField))
            {
                AddSpatialOperationToModel(initialFieldData, modelDefinition);
                StoreInitialFieldDataFile(initialFieldData);
            }

            logHandler.LogReport();
        }

        private void SetSpatialDataDirectory(InitialFieldFileData initialFieldFileData, string parentFilePath)
        {
            string parentDirectory = fileSystem.Path.GetDirectoryName(parentFilePath) ?? string.Empty;

            foreach (InitialFieldData initialFieldData in initialFieldFileData.AllFields)
            {
                initialFieldData.ParentDataDirectory = parentDirectory;
            }
        }

        private InitialFieldFileData ParseInitialFieldFile(string filePath)
        {
            log.InfoFormat(Resources.Reading_initial_field_data_from_0_, filePath);

            using (FileSystemStream stream = fileSystem.File.OpenRead(filePath))
            {
                return initialFieldFileParser.Parse(stream);
            }
        }

        private bool IsValidInitialField(InitialFieldData initialFieldData)
        {
            return initialFieldDataValidator.Validate(initialFieldData);
        }

        private void ClearStoredInitialFieldDataFiles()
        {
            initialFieldFileContext.ClearDataFileNames();
        }
        
        private void StoreInitialFieldDataFile(InitialFieldData initialFieldData)
        {
            initialFieldFileContext.StoreDataFileName(initialFieldData);
        }

        private void AddSpatialOperationToModel(InitialFieldData initialFieldData, WaterFlowFMModelDefinition modelDefinition)
        {
            string spatialOperationQuantity = quantities[initialFieldData.Quantity];

            ISpatialOperation spatialOperation = spatialOperationFactory.CreateFromInitialFieldData(initialFieldData);
            IList<ISpatialOperation> spatialOperations = GetSpatialOperations(spatialOperationQuantity, modelDefinition);

            initialFieldData.SpatialOperationName = spatialOperation.Name;
            initialFieldData.SpatialOperationQuantity = spatialOperationQuantity;

            spatialOperations.Add(spatialOperation);
        }

        private static IList<ISpatialOperation> GetSpatialOperations(string dataItemName, WaterFlowFMModelDefinition modelDefinition)
        {
            IList<ISpatialOperation> spatialOperations = modelDefinition.GetSpatialOperations(dataItemName);
            if (spatialOperations == null)
            {
                spatialOperations = new List<ISpatialOperation>();
                modelDefinition.SpatialOperations[dataItemName] = spatialOperations;
            }

            return spatialOperations;
        }
    }
}