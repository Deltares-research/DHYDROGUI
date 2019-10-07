using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    internal class DefinitionGeneratorCrossSectionDefinitionCunette : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionCunette()
            : base(CrossSectionRegion.CrossSectionDefinitionType.Cunette)
        {
        }

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            var standardDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
            if (standardDefinition == null) return IniCategory;
            if (standardDefinition.ShapeType != CrossSectionStandardShapeType.Cunette) return IniCategory;
            AddCommonRegionElements(crossSectionDefinition);
            var shapeCunette = standardDefinition.Shape as CrossSectionStandardShapeCunette;
            if (shapeCunette == null) return IniCategory;
            IniCategory.AddProperty(DefinitionRegion.CunetteWidth.Key, shapeCunette.Width, DefinitionRegion.CunetteWidth.Description, DefinitionRegion.CunetteWidth.Format);
            GenerateTabulatedProfile(ConverStandardToZw(standardDefinition));
            return IniCategory;
        }
    }
}