using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDTrapeziumDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IDelftIniCategory category)
        {

            var slope = category.ReadProperty<double>(DefinitionPropertySettings.Slope.Key);
            var bottomWidth = category.ReadProperty<double>(DefinitionPropertySettings.BottomWidth.Key);
            var maximumFlowWidth = category.ReadProperty<double>(DefinitionPropertySettings.MaximumFlowWidth.Key);

            var shape = new CrossSectionStandardShapeTrapezium { Slope = slope, BottomWidthB = bottomWidth, MaximumFlowWidth = maximumFlowWidth };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);
            return crossSectionDefinition;
        }
    }
}