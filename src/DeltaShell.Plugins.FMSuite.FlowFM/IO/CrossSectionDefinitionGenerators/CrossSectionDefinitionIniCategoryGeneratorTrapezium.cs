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
            iniCategory.AddProperty(DefinitionPropertySettings.Slope.Key, $"{trapeziumShape.Slope:0.00}");
            iniCategory.AddProperty(DefinitionPropertySettings.BottomWidth.Key, $"{trapeziumShape.BottomWidthB:0.00}");
            iniCategory.AddProperty(DefinitionPropertySettings.MaximumFlowWidth.Key, $"{trapeziumShape.MaximumFlowWidth:0.00}");
        }
    }
}
