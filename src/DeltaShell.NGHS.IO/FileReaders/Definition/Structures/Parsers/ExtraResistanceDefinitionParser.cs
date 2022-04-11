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
    /// Parser for extra resistances.
    /// </summary>
    public class ExtraResistanceDefinitionParser : StructureParserBase
    {
        /// <summary>
        /// Initializes a new <see cref="ExtraResistanceDefinitionParser"/>.
        /// </summary>
        /// <param name="structureType">The structure type.</param>
        /// <param name="category">The <see cref="IDelftIniCategory"/> to parse a structure from.</param>
        /// <param name="branch">The branch to import the bridge on.</param>
        /// <param name="structuresFilename">The structures filename.</param>
        /// <exception cref="ArgumentNullException">When any argument is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when an invalid <paramref name="structureType"/> is provided.
        /// </exception>
        public ExtraResistanceDefinitionParser(StructureType structureType,
                                               IDelftIniCategory category, 
                                               IBranch branch, 
                                               string structuresFilename) 
            : base(structureType, category, branch, structuresFilename) {}
        
        protected override IStructure1D Parse()
        {
            var extraResistance = new ExtraResistance
            {
                Name = Category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = Category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = Branch,
                Chainage = Branch.GetBranchSnappedChainage(Category.ReadProperty<double>(StructureRegion.Chainage.Key)),
            };

            var levels = Category.ReadProperty<string>(StructureRegion.Levels.Key).ToDoubleArray();
            var ksi = Category.ReadProperty<string>(StructureRegion.Ksi.Key).ToDoubleArray();

            extraResistance.FrictionTable = extraResistance.FrictionTable.CreateFunctionFromArrays(levels, ksi);

            return extraResistance;
        }
    }
}