using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDCircleDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IniSection iniSection)
        {

            var diameter = iniSection.ReadProperty<double>(DefinitionPropertySettings.Diameter.Key);
            
            var shape = new CrossSectionStandardShapeCircle{ Diameter = diameter};
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, iniSection);

            return crossSectionDefinition;
        }
    }
}