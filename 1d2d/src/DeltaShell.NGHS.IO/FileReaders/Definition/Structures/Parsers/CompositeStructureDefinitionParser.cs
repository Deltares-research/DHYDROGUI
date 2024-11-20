using System;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Parser for composite structures.
    /// </summary>
    public class CompositeStructureDefinitionParser : StructureParserBase
    {
        /// <summary>
        /// Initializes a new <see cref="CompositeStructureDefinitionParser"/>.
        /// </summary>
        /// <param name="structureType">The structure type.</param>
        /// <param name="iniSection">The <see cref="IniSection"/> to parse a structure from.</param>
        /// <param name="branch">The branch to import the bridge on.</param>
        /// <param name="structuresFileName">The structures filename.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public CompositeStructureDefinitionParser(StructureType structureType, IniSection iniSection, IBranch branch, string structuresFileName) 
            : base(structureType, iniSection, branch, structuresFileName) {}

        protected override IStructure1D Parse()
        {
            return new CompositeBranchStructure
            {
                Name = IniSection.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = IniSection.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = Branch,
                Chainage = Branch.GetBranchSnappedChainage(IniSection.ReadProperty<double>(StructureRegion.Chainage.Key)),
                Tag = IniSection.ReadProperty<string>(StructureRegion.StructureIds.Key, true) // optional if numStructures == 0
            };
        }
    }
}