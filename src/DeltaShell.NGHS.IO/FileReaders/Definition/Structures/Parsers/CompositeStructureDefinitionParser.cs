using System;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
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
        /// <param name="category">The <see cref="IDelftIniCategory"/> to parse a structure from.</param>
        /// <param name="branch">The branch to import the bridge on.</param>
        /// <param name="structuresFileName">The structures filename.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public CompositeStructureDefinitionParser(StructureType structureType, IDelftIniCategory category, IBranch branch, string structuresFileName) 
            : base(structureType, category, branch, structuresFileName) {}

        protected override IStructure1D Parse()
        {
            return new CompositeBranchStructure
            {
                Name = Category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = Category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = Branch,
                Chainage = Branch.GetBranchSnappedChainage(Category.ReadProperty<double>(StructureRegion.Chainage.Key)),
                Tag = Category.ReadProperty<string>(StructureRegion.StructureIds.Key, true) // optional if numStructures == 0
            };
        }
    }
}