using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionCircle : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionCircle() : base(CrossSectionRegion.CrossSectionDefinitionType.Circle, useTabulatedProfile:false)
        {
        }

        
        protected override bool HasCorrectCrossSectionShape(CrossSectionDefinitionStandard standardDefinition)
        {
            var shapeCircle = standardDefinition.Shape as CrossSectionStandardShapeCircle;
            return shapeCircle != null;
        }

        protected override void AddShapeMeasurementProperties(ICrossSectionStandardShape shape)
        {
            var circleShape = shape as CrossSectionStandardShapeCircle;
            if (circleShape == null) return;

            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.Diameter, circleShape.Diameter);
        }
    }
}