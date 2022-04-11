using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// <param name="structuresFilename">The structures filename.</param>
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
                                                          string structuresFilename)
        {
            Ensure.IsDefined(structureType, nameof(structureType));
            Ensure.NotNull(category, nameof(category));
            Ensure.NotNull(crossSectionDefinitions, nameof(crossSectionDefinitions));
            Ensure.NotNull(branch, nameof(branch));
            Ensure.NotNull(structuresFilename, nameof(structuresFilename));
            
            switch (structureType)
            {
                case StructureType.Bridge:
                    return new BridgeDefinitionParser(structureType, category, crossSectionDefinitions, branch, structuresFilename);
                case StructureType.Culvert:
                    return new CulvertDefinitionParser(structureType, category, crossSectionDefinitions, branch, structuresFilename);
                case StructureType.ExtraResistance:
                    return new ExtraResistanceDefinitionParser(structureType, category, branch, structuresFilename);
                case StructureType.Pump:
                    return new PumpDefinitionParser(structureType, category, branch, structuresFilename);
                case StructureType.Weir:
                case StructureType.UniversalWeir:
                case StructureType.GeneralStructure:
                    return new WeirDefinitionParser(structureType, category, branch, structuresFilename);
                case StructureType.Orifice:
                    return new OrificeDefinitionParser(structureType, category, branch, structuresFilename);
                case StructureType.CompositeBranchStructure:
                    return new CompositeStructureDefinitionParser(structureType, category, branch, structuresFilename);
                default:
                    throw new FileReadingException(string.Format(Resources.StructureParserProvider_No_parser_available, 
                                                                 structureType, Environment.NewLine));
            }
        }
    }
}