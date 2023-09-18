using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDRectangleDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IniSection iniSection)
        {
            
            var width = iniSection.ReadProperty<double>(DefinitionPropertySettings.RectangleWidth.Key);
            var height = iniSection.ReadProperty<double>(DefinitionPropertySettings.RectangleHeight.Key);
            //var closed = iniSection.ReadProperty<int>(DefinitionPropertySettings.Closed.Key); // NO CLUE!!
            
            var shape = new CrossSectionStandardShapeRectangle {Height = height, Width = width};
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, iniSection);

            return crossSectionDefinition;
        }
    }
}