using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures
{
    /// <summary>
    /// Provider for <see cref="IStructureParser"/>.
    /// </summary>
    public static class StructureParserProvider
    {
        /// <summary>
        /// Gets the <see cref="IStructureParser"/> for a specific structure.
        /// </summary>
        /// <param name="structureType">The structure type.</param>
        /// <param name="category">The <see cref="IDelftIniCategory"/> with the structure data.</param>
        /// <param name="crossSectionDefinitions">A collection of cross-section definitions.</param>
        /// <param name="branch">The branch the structure should be imported on.</param>
        /// <param name="structuresFilePath">The structures file path.</param>
        /// <param name="referenceDateTime">The reference date of the model being loaded.</param>
        /// <returns>A specific structure parser.</returns>
        /// <exception cref="FileReadingException">
        /// Thrown when there is no parser for the specified <paramref name="structureType"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public static IStructureParser GetStructureParser(StructureType structureType,
                                                          IDelftIniCategory category, 
                                                          ICollection<ICrossSectionDefinition> crossSectionDefinitions, 
                                                          IBranch branch,
                                                          string structuresFilePath,
                                                          DateTime referenceDateTime)
        {
            Ensure.IsDefined(structureType, nameof(structureType));
            Ensure.NotNull(category, nameof(category));
            Ensure.NotNull(crossSectionDefinitions, nameof(crossSectionDefinitions));
            Ensure.NotNull(branch, nameof(branch));
            Ensure.NotNull(structuresFilePath, nameof(structuresFilePath));

            string structuresFilename = Path.GetFileName(structuresFilePath);
            
            switch (structureType)
            {
                case StructureType.Bridge:
                    return new BridgeDefinitionParser(structureType, category, crossSectionDefinitions, branch, structuresFilename);
                case StructureType.Culvert:
                    return new CulvertDefinitionParser(new TimFile(),
                                                       structureType, 
                                                       category, 
                                                       crossSectionDefinitions, 
                                                       branch, 
                                                       structuresFilePath, 
                                                       referenceDateTime);
                case StructureType.Pump:
                    return new PumpDefinitionParser(new TimFile(),
                                                    structureType, 
                                                    category, 
                                                    branch, 
                                                    structuresFilePath, 
                                                    referenceDateTime);
                case StructureType.Weir:
                case StructureType.UniversalWeir:
                case StructureType.GeneralStructure:
                    return new WeirDefinitionParser(new TimFile(),
                                                    structureType, 
                                                    category, 
                                                    branch, 
                                                    structuresFilePath, 
                                                    referenceDateTime);
                case StructureType.Orifice:
                    return new OrificeDefinitionParser(new TimFile(),
                                                       structureType, 
                                                       category, 
                                                       branch, 
                                                       structuresFilePath, 
                                                       referenceDateTime);
                case StructureType.CompositeBranchStructure:
                    return new CompositeStructureDefinitionParser(structureType, category, branch, structuresFilename);
                default:
                    throw new FileReadingException(string.Format(Resources.StructureParserProvider_No_parser_available, 
                                                                 structureType, Environment.NewLine));
            }
        }
    }
}