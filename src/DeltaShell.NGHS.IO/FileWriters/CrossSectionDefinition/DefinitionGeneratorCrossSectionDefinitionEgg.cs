using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class
        DefinitionGeneratorCrossSectionDefinitionEgg : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionEgg()
            : base(CrossSectionRegion.CrossSectionDefinitionType.Egg)
        {
        }

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            var standardDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
            if (standardDefinition == null) return IniCategory;
            if (standardDefinition.ShapeType != CrossSectionStandardShapeType.Egg) return IniCategory;
            AddCommonRegionElements(crossSectionDefinition);
            var shapeEgg = standardDefinition.Shape as CrossSectionStandardShapeEgg;
            if (shapeEgg == null) return IniCategory;
            IniCategory.AddProperty(DefinitionRegion.EggWidth.Key, shapeEgg.Width, DefinitionRegion.EggWidth.Description, DefinitionRegion.EggWidth.Format);
            GenerateTabulatedProfile(ConverStandardToZw(standardDefinition));
            return IniCategory;
        }
    }
}