using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionTrapezium : DefinitionGeneratorCrossSectionDefinitionStandardTemplate
    {
        public DefinitionGeneratorCrossSectionDefinitionTrapezium() : base(CrossSectionRegion.CrossSectionDefinitionType.Trapezium)
        {
        }
        protected override void AddCommonProperties(ICrossSectionDefinition crossSectionDefinition)
        {
            base.AddCommonProperties(crossSectionDefinition);
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

            IniCategory.AddProperty(DefinitionPropertySettings.Slope, trapeziumShape.Slope);
            IniCategory.AddProperty(DefinitionPropertySettings.MaximumFlowWidth, trapeziumShape.MaximumFlowWidth);
            IniCategory.AddProperty(DefinitionPropertySettings.BottomWidth, trapeziumShape.BottomWidthB);
        }
    }
}