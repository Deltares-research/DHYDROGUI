using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionRectangle : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionRectangle() : base(CrossSectionRegion.CrossSectionDefinitionType.Rectangle, useTabulatedProfile:false)
        {
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

            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.RectangleWidth, rectangleShape.Width);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.RectangleHeight, rectangleShape.Height);
        }
    }
}