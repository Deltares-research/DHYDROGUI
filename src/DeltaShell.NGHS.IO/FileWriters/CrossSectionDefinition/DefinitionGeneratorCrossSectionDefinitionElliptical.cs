using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionElliptical : DefinitionGeneratorCrossSectionDefinitionStandardTemplate
    {
        public DefinitionGeneratorCrossSectionDefinitionElliptical() : base(CrossSectionRegion.CrossSectionDefinitionType.Elliptical)
        {
        }
        
        protected override bool HasCorrectCrossSectionShape(CrossSectionDefinitionStandard standardDefinition)
        {
            var ellipticalShape = standardDefinition.Shape as CrossSectionStandardShapeElliptical;
            return ellipticalShape != null;
        }

        protected override void AddShapeMeasurementProperties(ICrossSectionStandardShape shape)
        {
            var ellipticalShape = shape as CrossSectionStandardShapeElliptical;
            if (ellipticalShape == null) return;

            IniCategory.AddProperty(DefinitionPropertySettings.EllipseWidth, ellipticalShape.Width);
            IniCategory.AddProperty(DefinitionPropertySettings.EllipseHeight, ellipticalShape.Height);
        }
    }
}