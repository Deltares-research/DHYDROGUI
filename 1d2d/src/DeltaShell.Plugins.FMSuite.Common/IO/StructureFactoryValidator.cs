using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.Structure;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public static class StructureFactoryValidator
    {
        /// <summary>
        /// Checks if the given structure is valid or not.
        /// </summary>
        /// <param name="structure">The structure to be checked.</param>
        /// <returns>Error message, or empty if ok.</returns>
        public static string Validate(Structure2D structure)
        {
            if (structure == null) return "";

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
            if (structure.Structure2DType == Structure2DType.Pump &&
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
            var property = structure.GetProperty(KnownStructureProperties.NrOfReductionFactors);
            if (property != null)
            {
                var numberOfLevels = DataTypeValueParser.FromString<int>(property.GetValueAsString());
                if (numberOfLevels == 1 && structure.GetProperty(KnownStructureProperties.ReductionFactor) == null)
                {
                    errorMessage = $"Structure '{name}' with constant reduction factor does not have factor defined.";
                    return true;
                }
                if (numberOfLevels > 1)
                {
                    if (structure.GetProperty(KnownStructureProperties.Head) == null)
                    {
                        errorMessage = $"Structure '{name}' with multiple reduction factors does not have reference levels defined.";
                        return true;
                    }
                    if (structure.GetProperty(KnownStructureProperties.ReductionFactor) == null)
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
        /// <param name="structure">Structure to be validated.</param>
        /// <param name="name">Name of the structure</param>
        /// <param name="errorMessage">Error message output.</param>
        /// <returns>True if <paramref name="errorMessage"/> is set; False otherwise.</returns>
        private static bool ValidateGeneralStructureProperties(Structure2D structure, 
                                                               out string name,
                                                               out string errorMessage)
        {
            name = "";
            var idProperty = structure.GetProperty(KnownStructureProperties.Name);
            if (idProperty == null || String.IsNullOrEmpty(name = idProperty.GetValueAsString()))
            {
                errorMessage = "Id of structure must be specified.";
                return true;
            }

            #region Type property

            var structureType = structure.Structure2DType;
            if (SupportedTypes.All(t => t != structureType))
            {
                if(structure.InvalidStructureType == null) errorMessage = String.Format("Structure '{0}' cannot have null as type.", name);
                else errorMessage = String.Format("Structure '{0}' has unsupported type ({1}) specified.", name, structure.InvalidStructureType);
                return true;
            }
            var typeProperty = structure.GetProperty(KnownStructureProperties.Type);
            string typeAsString;
            if (typeProperty == null || String.IsNullOrEmpty(typeAsString = typeProperty.GetValueAsString()))
            {
                errorMessage = String.Format("Structure '{0}' does not have a type specified.", name);
                return true;
            }
            
            if ((Structure2DType)typeof(Structure2DType).GetEnumValueFromDescription(typeAsString) != structureType)
            {
                errorMessage = String.Format("Structure '{0}' has conflicting types: '{1}' and '{2}' are stated.",
                                    name, structureType.GetDescription(), typeAsString);
                return true;
            }

            #endregion

            #region Geometry related properties

            var xProperty = structure.GetProperty(StructureRegion.XCoordinates.Key);
            var yProperty = structure.GetProperty(StructureRegion.YCoordinates.Key);
            var polylineProperty = structure.GetProperty(KnownStructureProperties.PolylineFile);
            if (xProperty == null && yProperty == null)
            {
                if (polylineProperty == null)
                {
                    errorMessage = string.Format("Structure '{0}' must have geometry specified.", name);
                    return true;
                }
                if (String.IsNullOrEmpty(polylineProperty.GetValueAsString()))
                {
                    errorMessage = string.Format("Structure '{0}' does not have a filename specified for property '{1}'.", 
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
                    errorMessage = string.Format("Structure '{0}' cannot have point geometry and polyline geometry.", name);
                    return true;
                }
            }

            #endregion

            errorMessage = "";
            return false;
        }

        public static readonly Structure2DType[] SupportedTypes = new[]
        {
            Structure2DType.Pump,
            Structure2DType.Weir,
            Structure2DType.Gate,
            Structure2DType.GeneralStructure,
            Structure2DType.LeveeBreach 
        };

        /// <summary>
        /// Throws <see cref="FormatException"/> is case structure does not match expected type.
        /// </summary>
        /// <exception cref="FormatException"></exception>
        public static void ThrowIfInvalidType(Structure2D structure, IEnumerable<Structure2DType> expectedTypes)
        {
            var enumerable = expectedTypes as Structure2DType[] ?? expectedTypes.ToArray();
            var structureType = structure.Structure2DType;
            if (enumerable.All(type => type != structureType))
            {
                var isSingularItem = enumerable.Length > 1;
                throw new FormatException(String.Format("Structure specification for {0}, but should {1}: {2}",
                    structureType, isSingularItem ? "be type" : "be any of the following", String.Join(", ", enumerable)));
            }
        }
    }
}