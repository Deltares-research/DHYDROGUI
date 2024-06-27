using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDTrapeziumDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IniSection iniSection)
        {

            var slope = iniSection.ReadProperty<double>(DefinitionPropertySettings.Slope.Key);
            var bottomWidth = iniSection.ReadProperty<double>(DefinitionPropertySettings.BottomWidth.Key);
            var maximumFlowWidth = iniSection.ReadProperty<double>(DefinitionPropertySettings.MaximumFlowWidth.Key);

            var shape = new CrossSectionStandardShapeTrapezium { Slope = slope, BottomWidthB = bottomWidth, MaximumFlowWidth = maximumFlowWidth };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, iniSection);
            return crossSectionDefinition;
        }
    }
}