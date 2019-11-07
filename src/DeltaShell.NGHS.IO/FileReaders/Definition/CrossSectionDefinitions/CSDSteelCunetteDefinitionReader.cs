using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDSteelCunetteDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IDelftIniCategory category)
        {

            var height = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteHeight.Key);
            var radiusR = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteR.Key);
            var radiusR1 = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteR1.Key);
            var radiusR2 = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteR2.Key);
            var radiusR3 = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteR3.Key);
            
            var angleA = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteA.Key);
            var angleA1 = category.ReadProperty<double>(DefinitionPropertySettings.SteelCunetteA1.Key);
            
            var shape = new CrossSectionStandardShapeSteelCunette {Height = height, 
                RadiusR = radiusR,
                RadiusR1 = radiusR1,
                RadiusR2 = radiusR2,
                RadiusR3 = radiusR3,
                AngleA = angleA,
                AngleA1 = angleA1
            };
            var crossSectionDefinition = new CrossSectionDefinitionStandard(shape);
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);
            return crossSectionDefinition;
        }
    }
}