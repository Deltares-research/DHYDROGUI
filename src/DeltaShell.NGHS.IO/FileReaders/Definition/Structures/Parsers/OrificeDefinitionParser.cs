using System;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders.TimeSeriesReaders;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Parser for orifices.
    /// </summary>
    public class OrificeDefinitionParser : WeirDefinitionParser
    {
        protected override Weir CreateStructure() =>
            new Orifice(true);

        /// <summary>
        /// Initializes a new <see cref="OrificeDefinitionParser"/>.
        /// </summary>
        /// <param name="fileReader">The file reader</param>
        /// <param name="structureType">The structure type.</param>
        /// <param name="iniSection">The <see cref="IniSection"/> to parse a structure from.</param>
        /// <param name="branch">The branch to import the bridge on.</param>
        /// <param name="structuresFilePath">The structures filename.</param>
        /// <param name="referenceDateTime">The reference time date.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public OrificeDefinitionParser(ITimeSeriesFileReader fileReader,
                                       StructureType structureType,
                                       IniSection iniSection,
                                       IBranch branch,
                                       string structuresFilePath,
                                       DateTime referenceDateTime) :
            base(fileReader, 
                 structureType, 
                 iniSection, 
                 branch, 
                 structuresFilePath, 
                 referenceDateTime) { }
    }
}