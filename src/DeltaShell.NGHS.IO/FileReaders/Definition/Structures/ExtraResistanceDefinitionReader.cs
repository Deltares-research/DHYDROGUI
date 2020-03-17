using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures
{
    class ExtraResistanceDefinitionReader : IStructureDefinitionReader
    {
        public IStructure1D ReadDefinition(IDelftIniCategory category,
            IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch)
        {
            var extraResistance = new ExtraResistance
            {
                Name = category.ReadProperty<string>(StructureRegion.Id.Key),
                LongName = category.ReadProperty<string>(StructureRegion.Name.Key, true),
                Branch = branch,
                Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(category.ReadProperty<double>(StructureRegion.Chainage.Key)),
            };

            var levels = category.ReadProperty<string>(StructureRegion.Levels.Key).ToDoubleArray();
            var ksi = category.ReadProperty<string>(StructureRegion.Ksi.Key).ToDoubleArray();

            extraResistance.FrictionTable = extraResistance.FrictionTable.CreateFunctionFromArrays(levels, ksi);

            return extraResistance;
        }
    }
}