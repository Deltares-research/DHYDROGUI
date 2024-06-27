using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures
{
    /// <summary>
    /// Class for parsing structure definitions.
    /// </summary>
    public static class StructureDefinitionParser
    {
        /// <summary>
        /// Reads a structure from a <see cref="IniSection"/>.
        /// </summary>
        /// <param name="iniSection">The <see cref="IniSection"/> to read the structure from.</param>
        /// <param name="crossSectionDefinitions">A collection of cross-section definitions.</param>
        /// <param name="branch">The branch to import the structure to.</param>
        /// <param name="type">The type of the structure.</param>
        /// <param name="structuresFilePath">The structures file path.</param>
        /// <param name="referenceDateTime">The reference date of the model being loaded.</param>
        /// <param name="timeSeriesFileReader">TimeSeries FileReader which determines how time series are read.</param>
        /// <returns>The parsed structure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="FileReadingException">Thrown when an unknown structure type is provided.</exception>
        public static IStructure1D ReadStructure(this IniSection iniSection, 
                                                 ICollection<ICrossSectionDefinition> crossSectionDefinitions, 
                                                 IBranch branch, 
                                                 string type,
                                                 string structuresFilePath,
                                                 DateTime referenceDateTime,
                                                 ITimeSeriesFileReader timeSeriesFileReader)
        {
            Ensure.NotNull(iniSection, nameof(iniSection));
            Ensure.NotNull(crossSectionDefinitions, nameof(crossSectionDefinitions));
            Ensure.NotNull(branch, nameof(branch));
            Ensure.NotNull(type, nameof(type));
            Ensure.NotNull(structuresFilePath, nameof(structuresFilePath));
            Ensure.NotNull(timeSeriesFileReader, nameof(timeSeriesFileReader));
            
            if (!Enum.TryParse(type, true, out StructureType structureType))
            {
                if (string.Equals(type, "compound", StringComparison.InvariantCultureIgnoreCase))
                {
                    structureType = StructureType.CompositeBranchStructure;
                }
                else
                {
                    throw new FileReadingException(string.Format(Resources.StructureDefinitionParser_Could_not_parse_structure_type, type));
                }
            }

            IStructureParser structureParser = StructureParserProvider.GetStructureParser(structureType, 
                                                                                          iniSection, 
                                                                                          crossSectionDefinitions, 
                                                                                          branch,
                                                                                          structuresFilePath,
                                                                                          referenceDateTime,
                                                                                          timeSeriesFileReader);
            IStructure1D structure = structureParser.ParseStructure();

            return structure;
        }
    }
}