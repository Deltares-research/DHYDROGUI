using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionRectangle : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionRectangle() : base(CrossSectionRegion.CrossSectionDefinitionType.Rectangle)
        {
            GenerateProfileProperties = false;
        }

        protected override bool HasCorrectCrossSectionShape(CrossSectionDefinitionStandard standardDefinition)
        {
            var rectangleShape = standardDefinition.Shape as CrossSectionStandardShapeRectangle;
            return rectangleShape != null;
        }

        protected override void AddShapeMeasurementProperties(ICrossSectionStandardShape shape)
        {
            var rectangleShape = shape as CrossSectionStandardShapeRectangle;
            if (rectangleShape == null) return;

            IniCategory.AddProperty(DefinitionPropertySettings.RectangleWidth, rectangleShape.Width);
            IniCategory.AddProperty(DefinitionPropertySettings.RectangleHeight, rectangleShape.Height);
        }
    }
}