using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    internal class DefinitionGeneratorCrossSectionDefinitionRectangle : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionRectangle()
            : base(CrossSectionRegion.CrossSectionDefinitionType.Rectangle)
        {
        }

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            var standardDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
            if (standardDefinition == null) return IniCategory;
            if (standardDefinition.ShapeType != CrossSectionStandardShapeType.Rectangle) return IniCategory;
            AddCommonRegionElements(crossSectionDefinition);
            var shapeRectangle = standardDefinition.Shape as CrossSectionStandardShapeRectangle;
            if (shapeRectangle == null) return IniCategory;
            IniCategory.AddProperty(DefinitionRegion.RectangleWidth.Key, shapeRectangle.Width, DefinitionRegion.RectangleWidth.Description, DefinitionRegion.RectangleWidth.Format);
            IniCategory.AddProperty(DefinitionRegion.RectangleHeight.Key, shapeRectangle.Height, DefinitionRegion.RectangleHeight.Description, DefinitionRegion.RectangleHeight.Format);
            IniCategory.AddProperty(DefinitionRegion.Closed.Key, 1, DefinitionRegion.RectangleHeight.Description); //for closed write the default value of 1
            return IniCategory;
        }
    }
}