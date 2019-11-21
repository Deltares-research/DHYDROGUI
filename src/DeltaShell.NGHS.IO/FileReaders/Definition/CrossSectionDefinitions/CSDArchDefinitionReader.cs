using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDArchDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IDelftIniCategory category)
        {

            var archHeight = category.ReadProperty<double>(DefinitionPropertySettings.ArchHeight.Key);
            var height = category.ReadProperty<double>(DefinitionPropertySettings.ArchCrossSectionHeight.Key);
            var width = category.ReadProperty<double>(DefinitionPropertySettings.ArchCrossSectionWidth.Key);

            var shape = new CrossSectionStandardShapeArch { ArcHeight = archHeight, Width = width, Height = height};
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            return crossSectionDefinition;
        }
    }
    class CSDUShapeDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IDelftIniCategory category)
        {

            var archHeight = category.ReadProperty<double>(DefinitionPropertySettings.ArchHeight.Key);
            var height = category.ReadProperty<double>(DefinitionPropertySettings.ArchCrossSectionHeight.Key);
            var width = category.ReadProperty<double>(DefinitionPropertySettings.ArchCrossSectionWidth.Key);

            var shape = new CrossSectionStandardShapeUShape { ArcHeight = archHeight, Width = width, Height = height};
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            return crossSectionDefinition;
        }
    }
}