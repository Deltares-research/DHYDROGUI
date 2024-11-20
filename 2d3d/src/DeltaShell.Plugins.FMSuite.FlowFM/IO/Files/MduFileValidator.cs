using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.FMSuite.Common.IO.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    /// <summary>
    /// Provides functionality for validating and sanitizing .MDU files.
    /// </summary>
    internal sealed class MduFileValidator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MduFileValidator));

        /// <summary>
        /// MDU properties that represent an output file; should not be validated for existence.
        /// </summary>
        private static readonly string[] outputFileProperties = 
        {
            KnownProperties.HisFile,
            KnownProperties.MapFile
        };

        private readonly string mduFilePath;
        private readonly WaterFlowFMModelDefinition modelDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="MduFileValidator"/> class.
        /// </summary>
        /// <param name="mduFilePath">The path to the .MDU file.</param>
        /// <param name="modelDefinition">The model definition containing the parsed .MDU file data.</param>
        public MduFileValidator(string mduFilePath, WaterFlowFMModelDefinition modelDefinition)
        {
            this.mduFilePath = mduFilePath;
            this.modelDefinition = modelDefinition;
        }

        /// <summary>
        /// Provides access to the file system.
        /// </summary>
        public IFileSystem FileSystem { get; set; } = new FileSystem();

        /// <summary>
        /// Validates and sanitizes the .MDU file data. Logs validation issues.
        /// </summary>
        public void Validate()
        {
            foreach (WaterFlowFMProperty property in modelDefinition.FileProperties)
            {
                CleanupFileReferencePaths(property);
                ValidateInvalidCharsInFileReferencePaths(property);

                if (!IsOutputFileProperty(property))
                {
                    ValidateNotExistingFileReferences(property);
                }
            }
        }

        private void CleanupFileReferencePaths(WaterFlowFMProperty property)
        {
            IReadOnlyList<string> filePaths = property.GetFileLocationValues().ToArray();
            IReadOnlyList<string> cleanedFilePaths = filePaths.Select(CleanupFileReferencePath).ToArray();

            property.SetValueFromStrings(cleanedFilePaths);
        }

        private void ValidateInvalidCharsInFileReferencePaths(WaterFlowFMProperty property)
        {
            var validator = new FilePathCharactersValidator(mduFilePath, FileSystem);
            ValidateFileReferences(property, validator);
        }

        private void ValidateNotExistingFileReferences(WaterFlowFMProperty property)
        {
            var validator = new FilePathExistenceValidator(mduFilePath, mduFilePath, FileSystem);
            ValidateFileReferences(property, validator);
        }

        private static void ValidateFileReferences(WaterFlowFMProperty property, IValidator<FilePathInfo> validator)
        {
            IReadOnlyList<string> filePaths = property.GetFileLocationValues().ToArray();
            IList<string> invalidFilePaths = new List<string>();

            foreach (string filePath in filePaths)
            {
                var filePathInfo = new FilePathInfo(filePath, property.PropertyDefinition.FilePropertyKey, property.LineNumber);
                ValidationResult result = validator.Validate(filePathInfo);

                if (!result.Valid)
                {
                    invalidFilePaths.Add(filePath);
                    log.Error(result.Message);
                }
            }

            if (invalidFilePaths.Any())
            {
                property.SetValueFromStrings(filePaths.Except(invalidFilePaths));
            }
        }

        private string CleanupFileReferencePath(string path)
        {
            return path.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private bool IsOutputFileProperty(WaterFlowFMProperty property)
        {
            return outputFileProperties.Contains(
                property.PropertyDefinition.MduPropertyName, StringComparer.OrdinalIgnoreCase);
        }
    }
}