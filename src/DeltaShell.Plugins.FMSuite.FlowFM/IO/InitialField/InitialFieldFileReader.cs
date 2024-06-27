using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Hydro;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Logging;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.InitialField;
using log4net;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField
{
    /// <summary>
    /// File reader for the initial field file (*.ini).
    /// </summary>
    public sealed class InitialFieldFileReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InitialFieldFileReader));

        private readonly IFileSystem fileSystem;
        private readonly InitialFieldFileContext fieldFileContext;

        private ILogHandler logHandler;
        private WaterFlowFMModelDefinition modelDefinition;

        /// <summary>
        /// Initialize a new instance of the <see cref="InitialFieldFileReader"/> class.
        /// </summary>
        /// <param name="fieldFileContext">Storage for the original initial field data file names.</param>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="fieldFileContext"/> or <paramref name="fileSystem"/> is <c>null</c>.
        /// </exception>
        public InitialFieldFileReader(InitialFieldFileContext fieldFileContext, IFileSystem fileSystem)
        {
            Ensure.NotNull(fieldFileContext, nameof(fieldFileContext));
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.fieldFileContext = fieldFileContext;
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Reads the data from an initial field file and sets the data on the model definition.
        /// </summary>
        /// <param name="filePath"> Path to the initial field file. </param>
        /// <param name="parentFilePath">
        /// Path to which the data file references in the initial field file are relative to.
        /// In practice, can be either the MDU file or the initial field file.
        /// </param>
        /// <param name="definition">The model definition on which to set the data.</param>
        /// <returns>The parsed initial field file data.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="definition"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="filePath"/> or <paramref name="parentFilePath"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown when <paramref name="filePath"/> does not exist.
        /// </exception>
        public InitialFieldFileData Read(string filePath, string parentFilePath, WaterFlowFMModelDefinition definition)
        {
            Ensure.NotNull(definition, nameof(definition));
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNullOrWhiteSpace(parentFilePath, nameof(parentFilePath));

            if (!fileSystem.File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format(Resources.Initial_field_file_does_not_exist_0_, filePath), filePath);
            }

            modelDefinition = definition;
            logHandler = new LogHandler(Resources.reading_the_initial_field_file, log);

            fieldFileContext.ClearDataFileNames();

            InitialFieldFileData initialFieldFileData = ParseInitialFieldFile(filePath);

            SetParentDataDirectory(initialFieldFileData, parentFilePath);
            SetInitialConditionGlobalQuantity(initialFieldFileData);

            IEnumerable<InitialFieldData> validFields = initialFieldFileData.AllFields.Where(IsValidInitialField);
            IEnumerable<InitialFieldData> spatialFields = validFields.Where(x => x.DataFileType != InitialFieldDataFileType.OneDField);
            
            foreach (InitialFieldData initialFieldData in spatialFields)
            {
                AddSpatialOperationToModel(initialFieldData);

                fieldFileContext.StoreDataFileName(initialFieldData);
            }

            logHandler.LogReport();

            return initialFieldFileData;
        }

        private InitialFieldFileData ParseInitialFieldFile(string filePath)
        {
            log.InfoFormat(Resources.Reading_initial_field_data_from_0_, filePath);

            using (FileSystemStream stream = fileSystem.File.OpenRead(filePath))
            {
                var initialFieldFileParser = new InitialFieldFileParser(logHandler);

                return initialFieldFileParser.Parse(stream);
            }
        }

        private void SetParentDataDirectory(InitialFieldFileData initialFieldFileData, string parentFilePath)
        {
            string parentDirectory = fileSystem.Path.GetDirectoryName(parentFilePath) ?? string.Empty;

            foreach (InitialFieldData initialFieldData in initialFieldFileData.AllFields)
            {
                initialFieldData.ParentDataDirectory = parentDirectory;
            }
        }

        private bool IsValidInitialField(InitialFieldData initialFieldData)
        {
            var fieldValidator = new InitialFieldDataConfigValidator(modelDefinition);
            var initialFieldDataValidator = new InitialFieldDataValidator(logHandler, fileSystem) { FieldValidator = fieldValidator };

            return initialFieldDataValidator.Validate(initialFieldData);
        }

        private void SetInitialConditionGlobalQuantity(InitialFieldFileData initialFieldFileData)
        {
            IReadOnlyList<InitialFieldData> fieldsTwoD = initialFieldFileData.AllFields.Where(x => x.LocationType == InitialFieldLocationType.TwoD).ToArray();

            if (fieldsTwoD.Any(x => x.Quantity == InitialFieldQuantity.WaterLevel))
            {
                modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D, ((int)InitialConditionQuantity.WaterLevel).ToString());
            }
            else if (fieldsTwoD.Any(x => x.Quantity == InitialFieldQuantity.WaterDepth))
            {
                modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D, ((int)InitialConditionQuantity.WaterDepth).ToString());
            }
        }

        private void AddSpatialOperationToModel(InitialFieldData initialFieldData)
        {
            string spatialOperationQuantity = InitialFieldFileQuantities.SupportedQuantities[initialFieldData.Quantity];

            var spatialOperationFactory = new SpatialOperationFactory(fileSystem);
            ISpatialOperation spatialOperation = spatialOperationFactory.CreateFromInitialFieldData(initialFieldData);

            initialFieldData.SpatialOperationName = spatialOperation.Name;
            initialFieldData.SpatialOperationQuantity = spatialOperationQuantity;

            IList<ISpatialOperation> spatialOperations = GetSpatialOperations(spatialOperationQuantity);
            spatialOperations.Add(spatialOperation);
        }

        private IList<ISpatialOperation> GetSpatialOperations(string dataItemName)
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