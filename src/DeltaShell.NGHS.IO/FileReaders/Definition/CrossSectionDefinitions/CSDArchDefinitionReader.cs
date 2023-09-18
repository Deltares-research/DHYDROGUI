using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDArchDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IniSection iniSection)
        {

            var archHeight = iniSection.ReadProperty<double>(DefinitionPropertySettings.ArchHeight.Key);
            var height = iniSection.ReadProperty<double>(DefinitionPropertySettings.ArchCrossSectionHeight.Key);
            var width = iniSection.ReadProperty<double>(DefinitionPropertySettings.ArchCrossSectionWidth.Key);

            var shape = new CrossSectionStandardShapeArch { ArcHeight = archHeight, Width = width, Height = height};
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, iniSection);

            return crossSectionDefinition;
        }
    }
    class CSDUShapeDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IniSection iniSection)
        {

            var archHeight = iniSection.ReadProperty<double>(DefinitionPropertySettings.ArchHeight.Key);
            var height = iniSection.ReadProperty<double>(DefinitionPropertySettings.ArchCrossSectionHeight.Key);
            var width = iniSection.ReadProperty<double>(DefinitionPropertySettings.ArchCrossSectionWidth.Key);

            var shape = new CrossSectionStandardShapeUShape { ArcHeight = archHeight, Width = width, Height = height};
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, iniSection);

            return crossSectionDefinition;
        }
    }
}