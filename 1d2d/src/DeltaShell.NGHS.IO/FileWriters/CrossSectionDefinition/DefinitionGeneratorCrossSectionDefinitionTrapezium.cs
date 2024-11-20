using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionTrapezium : DefinitionGeneratorCrossSectionDefinitionStandardTemplate
    {
        public DefinitionGeneratorCrossSectionDefinitionTrapezium() : base(CrossSectionRegion.CrossSectionDefinitionType.Trapezium)
        {
        }
        
        protected override bool HasCorrectCrossSectionShape(CrossSectionDefinitionStandard standardDefinition)
        {
            var trapeziumShape = standardDefinition.Shape as CrossSectionStandardShapeTrapezium;
            return trapeziumShape != null;
        }

        protected override void AddShapeMeasurementProperties(ICrossSectionStandardShape shape)
        {
            var trapeziumShape = shape as CrossSectionStandardShapeTrapezium;
            if (trapeziumShape == null) return;

            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.Slope, trapeziumShape.Slope);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.MaximumFlowWidth, trapeziumShape.MaximumFlowWidth);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.BottomWidth, trapeziumShape.BottomWidthB);
        }
    }
}