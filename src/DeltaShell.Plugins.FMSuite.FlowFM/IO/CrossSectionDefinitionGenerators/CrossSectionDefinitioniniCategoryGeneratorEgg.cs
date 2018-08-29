using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators
{
    public class CrossSectionDefinitionIniCategoryGeneratorEgg : ACrossSectionDefinitionIniCategoryGenerator
    {
        public override DelftIniCategory GenerateIniCategory(CrossSectionDefinitionStandard crossSectionDefinition)
        {
            var crossSectionStandardShape = crossSectionDefinition.Shape as CrossSectionStandardShapeEgg;
            if (crossSectionStandardShape == null) throw new Exception();

            return base.GenerateIniCategory(crossSectionDefinition);
        }

        protected override void AddMeasurementsProperties(ICrossSectionStandardShape crossSectionShape)
        {
            var eggShape = crossSectionShape as CrossSectionStandardShapeEgg;
            iniCategory.AddProperty(DefinitionRegion.EggWidth.Key, $"{eggShape.Width:0.00}");
        }
    }
}
