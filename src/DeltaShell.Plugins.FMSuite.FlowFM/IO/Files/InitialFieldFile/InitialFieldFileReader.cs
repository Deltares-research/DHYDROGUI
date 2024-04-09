using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Deserialization;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.Logging;
using log4net;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile
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
        private readonly IniParser iniParser;
        private readonly InitialFieldFileParser initialFieldFileParser;
        private readonly LogHandler logHandler;
        private readonly SpatialOperationFactory spatialOperationFactory;

        /// <summary>
        /// Initialize a new instance of the <see cref="InitialFieldFileReader"/> class.
        /// </summary>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="fileSystem"/> is <c>null</c>
        /// </exception>
        public InitialFieldFileReader(IFileSystem fileSystem)
        {
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.fileSystem = fileSystem;
            iniParser = new IniParser();
            logHandler = new LogHandler(Resources.reading_the_initial_field_file, log);
            var validator = new InitialFieldValidator(logHandler, new SupportedInitialFieldValidator());
            initialFieldFileParser = new InitialFieldFileParser(logHandler, validator);
            spatialOperationFactory = new SpatialOperationFactory();
        }

        /// <summary>
        /// Reads the data from an initial field file and sets the data on the model definition.
        /// </summary>
        /// <param name="filePath"> Path to the initial field file. </param>
        /// <param name="relativeParentPath">
        /// Path to which the data file references in the initial field file are relative to.
        /// In practice, can be either the MDU file or the initial field file.
        /// </param>
        /// <param name="modelDefinition">The model definition on which to set the data.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="modelDefinition"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="filePath"/> is <c>null</c> or empty.
        /// </exception>
        public void Read(string filePath, string relativeParentPath, WaterFlowFMModelDefinition modelDefinition)
        {
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNullOrWhiteSpace(relativeParentPath, nameof(relativeParentPath));

            if (!fileSystem.File.Exists(filePath))
            {
                log.Error(string.Format(Resources.Initial_field_file_does_not_exist_0_, filePath));
                return;
            }

            InitialFieldFileData fileData = ParseFile(filePath);

            foreach (InitialField initialField in fileData.InitialConditions.Concat(fileData.Parameters))
            {
                ResolveDataFilePath(relativeParentPath, initialField);
                ReadFromInitialField(modelDefinition, initialField);
            }

            logHandler.LogReport();
        }

        private InitialFieldFileData ParseFile(string filePath)
        {
            log.InfoFormat(Resources.Reading_initial_field_data_from_0_,
                           filePath);

            IniData iniData;
            using (FileSystemStream iniStream = fileSystem.File.OpenRead(filePath))
            {
                iniData = iniParser.Parse(iniStream);
            }

            return initialFieldFileParser.Parse(iniData);
        }

        private void ResolveDataFilePath(string relativeParentPath, InitialField initialField)
        {
            string parentDirectory = fileSystem.Path.GetDirectoryName(relativeParentPath);
            initialField.DataFile = fileSystem.Path.Combine(parentDirectory, initialField.DataFile);
        }

        private void ReadFromInitialField(WaterFlowFMModelDefinition modelDefinition, InitialField initialField)
        {
            if (!fileSystem.File.Exists(initialField.DataFile))
            {
                logHandler.ReportError(string.Format(Resources.Initial_field_data_file_does_not_exist_0_, initialField.DataFile));
                return;
            }

            ISpatialOperation spatialOperation = spatialOperationFactory.CreateFromInitialField(initialField);
            string spatialOperationName = quantities[initialField.Quantity];
            AddSpatialOperation(spatialOperationName, modelDefinition, spatialOperation);
        }

        private static void AddSpatialOperation(string dataItemName, WaterFlowFMModelDefinition modelDefinition, ISpatialOperation spatialOperation)
        {
            IList<ISpatialOperation> spatialOperations = modelDefinition.GetSpatialOperations(dataItemName);

            if (spatialOperations != null)
            {
                spatialOperations.Add(spatialOperation);
                return;
            }

            spatialOperations = new List<ISpatialOperation>();
            modelDefinition.SpatialOperations[dataItemName] = spatialOperations;
            spatialOperations.Add(spatialOperation);
        }
    }
}