using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Provides functionality for validating and sanitizing .MDU files.
    /// </summary>
    internal sealed class MduFileValidator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MduFileValidator));

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
            IReadOnlyList<WaterFlowFMProperty> fileProperties = GetFileProperties();

            foreach (WaterFlowFMProperty property in fileProperties)
            {
                CleanupFileReferencePaths(property);
                ValidateInvalidCharsInFileReferencePaths(property);
                ValidateNotExistingFileReferences(property);
            }
        }

        private IReadOnlyList<WaterFlowFMProperty> GetFileProperties()
        {
            return modelDefinition.Properties
                                  .Where(p => p.PropertyDefinition.IsFile)
                                  .ToArray();
        }

        private void CleanupFileReferencePaths(WaterFlowFMProperty property)
        {
            IReadOnlyList<string> filePaths = GetFilePropertyValues(property).ToArray();
            IReadOnlyList<string> cleanedFilePaths = filePaths.Select(CleanupFileReferencePath).ToArray();

            SetFilePropertyValues(property, cleanedFilePaths);
        }
        
        private void ValidateInvalidCharsInFileReferencePaths(WaterFlowFMProperty property)
        {
            IReadOnlyList<string> filePaths = GetFilePropertyValues(property).ToArray();
            IReadOnlyList<string> invalidFilePaths = filePaths.Where(ContainsInvalidCharacters).ToArray();

            if (invalidFilePaths.Any())
            {
                LogInvalidCharsInFileReferencePaths(property, invalidFilePaths);
                SetFilePropertyValues(property, filePaths.Except(invalidFilePaths));
            }
        }

        private void ValidateNotExistingFileReferences(WaterFlowFMProperty property)
        {
            IReadOnlyList<string> filePaths = GetFilePropertyValues(property).ToArray();
            IReadOnlyList<string> existingFilePaths = filePaths.Where(IsExistingFileReference).ToArray();
            IReadOnlyList<string> invalidFilePaths = filePaths.Except(existingFilePaths).ToArray();

            if (invalidFilePaths.Any())
            {
                LogNotExistingFileReferences(property, invalidFilePaths);
                SetFilePropertyValues(property, existingFilePaths);
            }
        }

        private IReadOnlyList<string> GetFilePropertyValues(WaterFlowFMProperty property)
        {
            string propertyValue = property.GetValueAsString();

            if (string.IsNullOrWhiteSpace(propertyValue))
            {
                return Array.Empty<string>();
            }

            return property.PropertyDefinition.IsMultipleFile
                       ? propertyValue.Split(new[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
                       : new[] { propertyValue };
        }

        private void SetFilePropertyValues(WaterFlowFMProperty property, IEnumerable<string> filePaths)
        {
            property.SetValueAsString(string.Join(" ", filePaths));
        }

        private string CleanupFileReferencePath(string path)
        {
            return path.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        
        private bool ContainsInvalidCharacters(string path)
        {
            char[] invalidPathChars = Path.GetInvalidPathChars()
                                          .Concat(new[] { '*', '?' })
                                          .ToArray();

            return path.IndexOfAny(invalidPathChars) >= 0;
        }

        private bool IsExistingFileReference(string path)
        {
            string fullPath = MduFileHelper.GetCombinedPath(mduFilePath, path);
            return FileSystem.File.Exists(fullPath);
        }

        private void LogInvalidCharsInFileReferencePaths(WaterFlowFMProperty property, IEnumerable<string> invalidFilePaths)
        {
            foreach (string path in invalidFilePaths)
            {
                string propertyName = property.PropertyDefinition.MduPropertyName;
                string modelName = modelDefinition.ModelName;

                log.ErrorFormat(Resources.MduFileReferencePathContainsInvalidCharacters, path, mduFilePath, propertyName, modelName);
            }
        }

        private void LogNotExistingFileReferences(WaterFlowFMProperty property, IEnumerable<string> invalidFilePaths)
        {
            foreach (string path in invalidFilePaths)
            {
                string propertyName = property.PropertyDefinition.MduPropertyName;
                string modelName = modelDefinition.ModelName;

                log.ErrorFormat(Resources.MduFileReferenceDoesNotExist, path, mduFilePath, propertyName, modelName);
            }
        }
    }
}