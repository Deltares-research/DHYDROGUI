using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDRectangleDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IDelftIniCategory category)
        {
            
            var width = category.ReadProperty<double>(DefinitionPropertySettings.RectangleWidth.Key);
            var height = category.ReadProperty<double>(DefinitionPropertySettings.RectangleHeight.Key);
            //var closed = category.ReadProperty<int>(DefinitionPropertySettings.Closed.Key); // NO CLUE!!
            
            var shape = new CrossSectionStandardShapeRectangle {Height = height, Width = width};
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            return crossSectionDefinition;
        }
    }
}