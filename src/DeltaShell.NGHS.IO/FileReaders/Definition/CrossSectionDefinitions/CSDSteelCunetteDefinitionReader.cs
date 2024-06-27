using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDSteelCunetteDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IniSection iniSection)
        {

            var height = iniSection.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteHeight.Key);
            var radiusR = iniSection.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteR.Key);
            var radiusR1 = iniSection.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteR1.Key);
            var radiusR2 = iniSection.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteR2.Key);
            var radiusR3 = iniSection.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteR3.Key);
            
            var angleA = iniSection.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteA.Key);
            var angleA1 = iniSection.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteA1.Key);
            
            var shape = new CrossSectionStandardShapeSteelCunette {Height = height, 
                RadiusR = radiusR,
                RadiusR1 = radiusR1,
                RadiusR2 = radiusR2,
                RadiusR3 = radiusR3,
                AngleA = angleA,
                AngleA1 = angleA1
            };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, iniSection);
            return crossSectionDefinition;
        }
    }
}