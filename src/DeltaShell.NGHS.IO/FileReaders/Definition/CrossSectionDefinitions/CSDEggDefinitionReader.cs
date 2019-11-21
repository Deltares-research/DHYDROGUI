using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDEggDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IDelftIniCategory category)
        {

            var width = category.ReadProperty<double>(DefinitionPropertySettings.EggWidth.Key);

            var shape = new CrossSectionStandardShapeEgg { Width = width };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            return crossSectionDefinition;
        }
    }
    class CSDInvertedEggDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IDelftIniCategory category)
        {

            var width = category.ReadProperty<double>(DefinitionPropertySettings.EggWidth.Key);

            var shape = new CrossSectionStandardShapeInvertedEgg { Width = width };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            return crossSectionDefinition;
        }
    }
}