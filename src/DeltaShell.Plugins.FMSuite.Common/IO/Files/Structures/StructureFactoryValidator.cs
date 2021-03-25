using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures
{
    public static class StructureFactoryValidator
    {
        public static readonly StructureType[] SupportedTypes =
        {
            StructureType.Pump,
            StructureType.Weir,
            StructureType.Gate,
            StructureType.GeneralStructure
        };

        /// <summary>
        /// Checks if the given structure is valid or not.
        /// </summary>
        /// <param name="structureDataAccessObject"> The structure data access object to be checked. </param>
        /// <returns> Error message, or empty if ok. </returns>
        public static string Validate(StructureDAO structureDataAccessObject)
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
        private static bool ValidateGeneralStructureProperties(StructureDAO structureDataAccessObject,
                                                               out string name,
                                                               out string errorMessage)
        {
            name = "";
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

                if (string.IsNullOrEmpty(polylineProperty.GetValueAsString()))
                {
                    errorMessage = $"Structure '{name}' does not have a filename specified for property '{KnownStructureProperties.PolylineFile}'.";
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

            errorMessage = "";
            return false;
        }
    }
}