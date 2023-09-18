using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini;
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
        /// <param name="iniSection">The <see cref="IniSection"/> with the structure data.</param>
        /// <param name="crossSectionDefinitions">A collection of cross-section definitions.</param>
        /// <param name="branch">The branch the structure should be imported on.</param>
        /// <param name="structuresFilePath">The structures file path.</param>
        /// <param name="referenceDateTime">The reference date of the model being loaded.</param>
        /// <param name="timeSeriesFileReader">TimeSeries FileReader which determines how time series are read.</param>
        /// <returns>A specific structure parser.</returns>
        /// <exception cref="FileReadingException">
        /// Thrown when there is no parser for the specified <paramref name="structureType"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public static IStructureParser GetStructureParser(StructureType structureType,
                                                          IniSection iniSection, 
                                                          ICollection<ICrossSectionDefinition> crossSectionDefinitions, 
                                                          IBranch branch,
                                                          string structuresFilePath,
                                                          DateTime referenceDateTime,
                                                          ITimeSeriesFileReader timeSeriesFileReader)
        {
            Ensure.IsDefined(structureType, nameof(structureType));
            Ensure.NotNull(iniSection, nameof(iniSection));
            Ensure.NotNull(crossSectionDefinitions, nameof(crossSectionDefinitions));
            Ensure.NotNull(branch, nameof(branch));
            Ensure.NotNull(structuresFilePath, nameof(structuresFilePath));
            Ensure.NotNull(timeSeriesFileReader, nameof(timeSeriesFileReader));

            string structuresFilename = Path.GetFileName(structuresFilePath);

            switch (structureType)
            {
                case StructureType.Bridge:
                    return new BridgeDefinitionParser(structureType, iniSection, crossSectionDefinitions, branch, structuresFilename);
                case StructureType.Culvert:
                    return new CulvertDefinitionParser(timeSeriesFileReader,
                                                       structureType, 
                                                       iniSection, 
                                                       crossSectionDefinitions, 
                                                       branch, 
                                                       structuresFilePath, 
                                                       referenceDateTime);
                case StructureType.Pump:
                    return new PumpDefinitionParser(timeSeriesFileReader,
                                                    structureType, 
                                                    iniSection, 
                                                    branch, 
                                                    structuresFilePath, 
                                                    referenceDateTime);
                case StructureType.Weir:
                case StructureType.UniversalWeir:
                case StructureType.GeneralStructure:
                    return new WeirDefinitionParser(timeSeriesFileReader,
                                                    structureType, 
                                                    iniSection, 
                                                    branch, 
                                                    structuresFilePath, 
                                                    referenceDateTime);
                case StructureType.Orifice:
                    return new OrificeDefinitionParser(timeSeriesFileReader,
                                                       structureType, 
                                                       iniSection, 
                                                       branch, 
                                                       structuresFilePath, 
                                                       referenceDateTime);
                case StructureType.CompositeBranchStructure:
                    return new CompositeStructureDefinitionParser(structureType, iniSection, branch, structuresFilename);
                default:
                    throw new FileReadingException(string.Format(Resources.StructureParserProvider_No_parser_available, 
                                                                 structureType, Environment.NewLine));
            }
        }
    }
}