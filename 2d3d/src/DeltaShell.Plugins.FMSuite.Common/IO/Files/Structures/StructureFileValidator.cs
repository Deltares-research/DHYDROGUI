using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.FMSuite.Common.IO.Validation;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures
{
    /// <summary>
    /// Validator for data from the structures file.
    /// </summary>
    public sealed class StructureFileValidator
    {
        public static readonly StructureType[] SupportedTypes =
        {
            StructureType.Pump,
            StructureType.Weir,
            StructureType.Gate,
            StructureType.GeneralStructure
        };

        private readonly FilePathValidator filePathValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="StructureFileValidator"/> class.
        /// </summary>
        /// <param name="structureFilePath"> The structure file path. </param>
        /// <param name="referenceFilePath"> The reference file path. </param>
        /// <param name="fileSystem">Provides access to the file system. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="fileSystem"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="structureFilePath"/> or <paramref name="referenceFilePath"/> is <c>null</c> or white space.
        /// </exception>
        public StructureFileValidator(string structureFilePath, string referenceFilePath, IFileSystem fileSystem)
        {
            Ensure.NotNullOrWhiteSpace(structureFilePath, nameof(structureFilePath));
            Ensure.NotNullOrWhiteSpace(referenceFilePath, nameof(referenceFilePath));
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            filePathValidator = FilePathValidator.CreateDefault(referenceFilePath, structureFilePath, fileSystem);
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="StructureFileValidator"/> class.
        /// </summary>
        /// <param name="structureFilePath"> The structure file path. </param>
        /// <param name="referenceFilePath"> The reference file path. </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="structureFilePath"/> or <paramref name="referenceFilePath"/> is <c>null</c> or white space.
        /// </exception>
        public StructureFileValidator(string structureFilePath, string referenceFilePath)
            : this(structureFilePath, referenceFilePath, new FileSystem())
        {
        }

        /// <summary>
        /// Checks if the given structure is valid or not.
        /// </summary>
        /// <param name="structureDataAccessObject"> The structure data access object to be checked. </param>
        /// <returns> Error message, or empty if ok. </returns>
        public string Validate(StructureDAO structureDataAccessObject)
        {
            if (structureDataAccessObject == null)
            {
                return "";
            }

            if (ValidateGeneralStructureProperties(structureDataAccessObject, out string name, out string errorMessage))
            {
                return errorMessage;
            }

            if (ValidateSpecificStructureProperties(structureDataAccessObject, name, out errorMessage))
            {
                return errorMessage;
            }

            return "";
        }

        /// <summary>
        /// Throws <see cref="FormatException"/> is case structure does not match expected type.
        /// </summary>
        /// <exception cref="FormatException"> </exception>
        public static void ThrowIfInvalidType(StructureDAO structureDataAccessObject, IEnumerable<StructureType> expectedTypes)
        {
            StructureType[] enumerable = expectedTypes as StructureType[] ?? expectedTypes.ToArray();
            StructureType structureType = structureDataAccessObject.StructureType;
            if (enumerable.All(type => type != structureType))
            {
                bool isSingularItem = enumerable.Length > 1;
                throw new FormatException($"Structure specification for {structureType}, but should {(isSingularItem ? "be type" : "be any of the following")}: {string.Join(", ", enumerable)}");
            }
        }

        private static bool ValidateSpecificStructureProperties(StructureDAO structureDataAccessObject, string name,
                                                                out string errorMessage)
        {
            if (structureDataAccessObject.StructureType == StructureType.Pump &&
                ValidateGeneralPumpProperties(structureDataAccessObject, name, out string errorMessage1))
            {
                errorMessage = errorMessage1;
                return true;
            }

            errorMessage = "";
            return false;
        }

        private static bool ValidateGeneralPumpProperties(StructureDAO structureDataAccessObject, string name,
                                                          out string errorMessage)
        {
            ModelProperty property = structureDataAccessObject.GetProperty(KnownStructureProperties.NrOfReductionFactors);
            if (property != null)
            {
                var numberOfLevels = FMParser.FromString<int>(property.GetValueAsString());
                if (numberOfLevels == 1 && structureDataAccessObject.GetProperty(KnownStructureProperties.ReductionFactor) == null)
                {
                    errorMessage = $"Structure '{name}' with constant reduction factor does not have factor defined.";
                    return true;
                }

                if (numberOfLevels > 1)
                {
                    if (structureDataAccessObject.GetProperty(KnownStructureProperties.Head) == null)
                    {
                        errorMessage = $"Structure '{name}' with multiple reduction factors does not have reference levels defined.";
                        return true;
                    }

                    if (structureDataAccessObject.GetProperty(KnownStructureProperties.ReductionFactor) == null)
                    {
                        errorMessage = $"Structure '{name}' with multiple reduction factors does not have factors defined.";
                        return true;
                    }
                }
            }

            errorMessage = "";
            return false;
        }

        /// <summary>
        /// Validates general structure properties.
        /// </summary>
        /// <param name="structureDataAccessObject"> Structure data access object to be validated. </param>
        /// <param name="name"> Name of the structure </param>
        /// <param name="errorMessage"> Error message output. </param>
        /// <returns> True if <paramref name="errorMessage"/> is set; False otherwise. </returns>
        private bool ValidateGeneralStructureProperties(StructureDAO structureDataAccessObject,
                                                        out string name,
                                                        out string errorMessage)
        {
            name = "";
            errorMessage = "";

            ModelProperty idProperty = structureDataAccessObject.GetProperty(KnownStructureProperties.Name);
            if (idProperty == null || string.IsNullOrEmpty(name = idProperty.GetValueAsString()))
            {
                errorMessage = "Id of structure must be specified.";
                return true;
            }

            #region Type property

            StructureType structureType = structureDataAccessObject.StructureType;
            if (SupportedTypes.All(t => t != structureType))
            {
                if (structureDataAccessObject.InvalidStructureType == null)
                {
                    errorMessage = $"Structure '{name}' cannot have null as type.";
                }
                else
                {
                    errorMessage = $"Structure '{name}' has unsupported type ({structureDataAccessObject.InvalidStructureType}) specified.";
                }

                return true;
            }

            ModelProperty typeProperty = structureDataAccessObject.GetProperty(KnownStructureProperties.Type);
            string typeAsString;
            if (typeProperty == null || string.IsNullOrEmpty(typeAsString = typeProperty.GetValueAsString()))
            {
                errorMessage = $"Structure '{name}' does not have a type specified.";
                return true;
            }

            var structureTypeFromString = (StructureType) typeof(StructureType).GetEnumValueFromDescription(typeAsString);
            if (structureTypeFromString != structureType)
            {
                errorMessage = $"Structure '{name}' has conflicting types: '{structureType.GetDescription()}' and '{typeAsString}' are stated.";
                return true;
            }

            #endregion

            #region Geometry related properties

            ModelProperty xProperty = structureDataAccessObject.GetProperty(KnownStructureProperties.X);
            ModelProperty yProperty = structureDataAccessObject.GetProperty(KnownStructureProperties.Y);
            ModelProperty polylineProperty = structureDataAccessObject.GetProperty(KnownStructureProperties.PolylineFile);
            if (xProperty == null && yProperty == null)
            {
                if (polylineProperty == null)
                {
                    errorMessage = $"Structure '{name}' must have geometry specified.";
                    return true;
                }

                string polyLineFileName = polylineProperty.GetValueAsString();
                if (string.IsNullOrEmpty(polyLineFileName))
                {
                    errorMessage = $"Structure '{name}' does not have a filename specified for property '{KnownStructureProperties.PolylineFile}'.";
                    return true;
                }

                if (ValidateFileReference(polyLineFileName, polylineProperty, ref errorMessage))
                {
                    return true;
                }
            }
            else
            {
                if (xProperty == null)
                {
                    errorMessage = $"Structure '{name}' has property '{KnownStructureProperties.Y}' specified, but '{KnownStructureProperties.X}' is missing.";
                    return true;
                }

                if (yProperty == null)
                {
                    errorMessage = $"Structure '{name}' has property '{KnownStructureProperties.X}' specified, but '{KnownStructureProperties.Y}' is missing.";
                    return true;
                }

                if (polylineProperty != null)
                {
                    errorMessage = $"Structure '{name}' cannot have point geometry and polyline geometry.";
                    return true;
                }
            }

            #endregion

            if (ValidateTimeSeriesFileReferences(structureDataAccessObject, ref errorMessage))
            {
                return true;
            }

            return false;
        }

        private bool ValidateTimeSeriesFileReferences(StructureDAO structureDataAccessObject, ref string errorMessage)
        {
            foreach (ModelProperty modelProperty in structureDataAccessObject.Properties)
            {
                string timeSeriesFileName = GetTimeSeriesFileName(modelProperty);
                if (timeSeriesFileName == null)
                {
                    continue;
                }

                if (ValidateFileReference(timeSeriesFileName, modelProperty, ref errorMessage))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetTimeSeriesFileName(ModelProperty modelProperty)
        {
            if (modelProperty.Value is Steerable steerable && steerable.Mode == SteerableMode.TimeSeries)
            {
                return steerable.TimeSeriesFilename;
            }

            return null;
        }

        private bool ValidateFileReference(string fileReference, ModelProperty property, ref string errorMessage)
        {
            var filePathInfo = new FilePathInfo(fileReference, property.PropertyDefinition.FilePropertyKey, property.LineNumber);

            ValidationResult result = filePathValidator.Validate(filePathInfo);
            if (!result.Valid)
            {
                errorMessage = result.Message;
                return true;
            }

            return false;
        }
    }
}