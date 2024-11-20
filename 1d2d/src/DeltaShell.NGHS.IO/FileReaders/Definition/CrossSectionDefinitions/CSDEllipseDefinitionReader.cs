using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDEllipseDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IniSection iniSection)
        {

            var width = iniSection.ReadProperty<double>(DefinitionPropertySettings.EllipseWidth.Key);
            var height = iniSection.ReadProperty<double>(DefinitionPropertySettings.EllipseHeight.Key);
            
            var shape = new CrossSectionStandardShapeElliptical{ Height = height, Width = width  };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, iniSection);
            
            return crossSectionDefinition;
        }
    }
}