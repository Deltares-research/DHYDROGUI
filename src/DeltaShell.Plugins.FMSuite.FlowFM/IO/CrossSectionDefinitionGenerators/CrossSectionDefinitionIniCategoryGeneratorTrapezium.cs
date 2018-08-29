using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators
{
    public class CrossSectionDefinitionIniCategoryGeneratorTrapezium : ACrossSectionDefinitionIniCategoryGenerator
    {
        public override DelftIniCategory GenerateIniCategory(CrossSectionDefinitionStandard crossSectionDefinition)
        {
            var crossSectionShape = crossSectionDefinition.Shape as CrossSectionStandardShapeTrapezium;
            if (crossSectionShape == null) throw new Exception();

            return base.GenerateIniCategory(crossSectionDefinition);
        }

        protected override void AddMeasurementsProperties(ICrossSectionStandardShape crossSectionShape)
        {
            var trapeziumShape = crossSectionShape as CrossSectionStandardShapeTrapezium;
            iniCategory.AddProperty(DefinitionRegion.Slope.Key, $"{trapeziumShape.Slope:0.00}");
            iniCategory.AddProperty(DefinitionRegion.BottomWidth.Key, $"{trapeziumShape.BottomWidthB:0.00}");
            iniCategory.AddProperty(DefinitionRegion.MaximumFlowWidth.Key, $"{trapeziumShape.MaximumFlowWidth:0.00}");
        }
    }
}
