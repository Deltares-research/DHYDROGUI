using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures
{
    public static class StructureFactoryValidator
    {
        /// <summary>
        /// Checks if the given structure is valid or not.
        /// </summary>
        /// <param name="structure"> The structure to be checked. </param>
        /// <returns> Error message, or empty if ok. </returns>
        public static string Validate(Structure2D structure)
        {
            if (structure == null)
            {
                return "";
            }

            string errorMessage, name;
            if (ValidateGeneralStructureProperties(structure, out name, out errorMessage))
            {
                return errorMessage;
            }

            if (ValidateSpecificStructureProperties(structure, name, out errorMessage))
            {
                return errorMessage;
            }

            return "";
        }

        private static bool ValidateSpecificStructureProperties(Structure2D structure, string name,
                                                                out string errorMessage)
        {
            string errorMessage1;
            if (structure.StructureType == StructureType.Pump &&
                ValidateGeneralPumpProperties(structure, name, out errorMessage1))
            {
                errorMessage = errorMessage1;
                return true;
            }

            errorMessage = "";
            return false;
        }

        private static bool ValidateGeneralPumpProperties(Structure2D structure, string name,
                                                          out string errorMessage)
        {
            ModelProperty property = structure.GetProperty(KnownStructureProperties.NrOfReductionFactors);
            if (property != null)
            {
                var numberOfLevels = FMParser.FromString<int>(property.GetValueAsString());
                if (numberOfLevels == 1)
                {
                    if (structure.GetProperty(KnownStructureProperties.ReductionFactor) == null)
                    {
                        errorMessage =
                            string.Format(
                                "Structure '{0}' with constant reduction factor does not have factor defined.", name);
                        return true;
                    }
                }

                if (numberOfLevels > 1)
                {
                    if (structure.GetProperty(KnownStructureProperties.Head) == null)
                    {
                        errorMessage =
                            string.Format(
                                "Structure '{0}' with multiple reduction factors does not have reference levels defined.",
                                name);
                        return true;
                    }

                    if (structure.GetProperty(KnownStructureProperties.ReductionFactor) == null)
                    {
                        errorMessage =
                            string.Format(
                                "Structure '{0}' with multiple reduction factors does not have factors defined.", name);
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
        /// <param name="structure"> Structure to be validated. </param>
        /// <param name="name"> Name of the structure </param>
        /// <param name="errorMessage"> Error message output. </param>
        /// <returns> True if <paramref name="errorMessage" /> is set; False otherwise. </returns>
        private static bool ValidateGeneralStructureProperties(Structure2D structure,
                                                               out string name,
                                                               out string errorMessage)
        {
            name = "";
            ModelProperty idProperty = structure.GetProperty(KnownStructureProperties.Name);
            if (idProperty == null || string.IsNullOrEmpty(name = idProperty.GetValueAsString()))
            {
                errorMessage = "Id of structure must be specified.";
                return true;
            }

            #region Type property

            StructureType structureType = structure.StructureType;
            if (SupportedTypes.All(t => t != structureType))
            {
                if (structure.InvalidStructureType == null)
                {
                    errorMessage = string.Format("Structure '{0}' cannot have null as type.", name);
                }
                else
                {
                    errorMessage = string.Format("Structure '{0}' has unsupported type ({1}) specified.", name,
                                                 structure.InvalidStructureType);
                }

                return true;
            }

            ModelProperty typeProperty = structure.GetProperty(KnownStructureProperties.Type);
            string typeAsString;
            if (typeProperty == null || string.IsNullOrEmpty(typeAsString = typeProperty.GetValueAsString()))
            {
                errorMessage = string.Format("Structure '{0}' does not have a type specified.", name);
                return true;
            }

            var structureTypeFromString =
                (StructureType) typeof(StructureType).GetEnumValueFromDescription(typeAsString);
            if (structureTypeFromString != structureType)
            {
                errorMessage = string.Format("Structure '{0}' has conflicting types: '{1}' and '{2}' are stated.",
                                             name, structureType.GetDescription(), typeAsString);
                return true;
            }

            #endregion

            #region Geometry related properties

            ModelProperty xProperty = structure.GetProperty(KnownStructureProperties.X);
            ModelProperty yProperty = structure.GetProperty(KnownStructureProperties.Y);
            ModelProperty polylineProperty = structure.GetProperty(KnownStructureProperties.PolylineFile);
            if (xProperty == null && yProperty == null)
            {
                if (polylineProperty == null)
                {
                    errorMessage = string.Format("Structure '{0}' must have geometry specified.", name);
                    return true;
                }

                if (string.IsNullOrEmpty(polylineProperty.GetValueAsString()))
                {
                    errorMessage = string.Format(
                        "Structure '{0}' does not have a filename specified for property '{1}'.",
                        name, KnownStructureProperties.PolylineFile);
                    return true;
                }
            }
            else
            {
                if (xProperty == null)
                {
                    errorMessage = string.Format("Structure '{0}' has property '{1}' specified, but '{2}' is missing.",
                                                 name, KnownStructureProperties.Y, KnownStructureProperties.X);
                    return true;
                }

                if (yProperty == null)
                {
                    errorMessage = string.Format("Structure '{0}' has property '{1}' specified, but '{2}' is missing.",
                                                 name, KnownStructureProperties.X, KnownStructureProperties.Y);
                    return true;
                }

                if (polylineProperty != null)
                {
                    errorMessage = string.Format("Structure '{0}' cannot have point geometry and polyline geometry.",
                                                 name);
                    return true;
                }
            }

            #endregion

            errorMessage = "";
            return false;
        }

        public static readonly StructureType[] SupportedTypes = new[]
        {
            StructureType.Pump,
            StructureType.Weir,
            StructureType.Gate,
            StructureType.GeneralStructure
        };

        /// <summary>
        /// Throws <see cref="FormatException" /> is case structure does not match expected type.
        /// </summary>
        /// <exception cref="FormatException"> </exception>
        public static void ThrowIfInvalidType(Structure2D structure, IEnumerable<StructureType> expectedTypes)
        {
            StructureType[] enumerable = expectedTypes as StructureType[] ?? expectedTypes.ToArray();
            StructureType structureType = structure.StructureType;
            if (enumerable.All(type => type != structureType))
            {
                bool isSingularItem = enumerable.Length > 1;
                throw new FormatException(string.Format("Structure specification for {0}, but should {1}: {2}",
                                                        structureType,
                                                        isSingularItem ? "be type" : "be any of the following",
                                                        string.Join(", ", enumerable)));
            }
        }
    }
}