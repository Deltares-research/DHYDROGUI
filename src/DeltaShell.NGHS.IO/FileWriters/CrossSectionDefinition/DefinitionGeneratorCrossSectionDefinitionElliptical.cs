using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionElliptical : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionElliptical()
            : base(CrossSectionRegion.CrossSectionDefinitionType.Elliptical)
        {
        }

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            var standardDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
            if (standardDefinition == null) return IniCategory;
            if (standardDefinition.ShapeType != CrossSectionStandardShapeType.Elliptical) return IniCategory;
            AddCommonRegionElements(crossSectionDefinition);
            var shapeElliptical = standardDefinition.Shape as CrossSectionStandardShapeElliptical;
            if (shapeElliptical == null) return IniCategory;
            IniCategory.AddProperty(DefinitionRegion.EllipseWidth.Key, shapeElliptical.Width, DefinitionRegion.EllipseWidth.Description, DefinitionRegion.EllipseWidth.Format);
            IniCategory.AddProperty(DefinitionRegion.EllipseHeight.Key, shapeElliptical.Height, DefinitionRegion.EllipseHeight.Description, DefinitionRegion.EllipseHeight.Format);
            GenerateTabulatedProfile(ConverStandardToZw(standardDefinition));
            return IniCategory;
        }
    }
}