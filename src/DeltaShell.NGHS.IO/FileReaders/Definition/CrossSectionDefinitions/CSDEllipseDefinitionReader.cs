using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDEllipseDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IDelftIniCategory category)
        {

            var width = category.ReadProperty<double>(DefinitionPropertySettings.EllipseWidth.Key);
            var height = category.ReadProperty<double>(DefinitionPropertySettings.EllipseHeight.Key);
            
            var shape = new CrossSectionStandardShapeElliptical{ Height = height, Width = width  };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);
            
            return crossSectionDefinition;
        }
    }
}