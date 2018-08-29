using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators
{
    public class CrossSectionDefinitionIniCategoryGeneratorCunette : ACrossSectionDefinitionIniCategoryGenerator
    {
        public override DelftIniCategory GenerateIniCategory(CrossSectionDefinitionStandard crossSectionDefinition)
        {
            var crossSectionStandardShape = crossSectionDefinition.Shape as CrossSectionStandardShapeCunette;
            if (crossSectionStandardShape == null) throw new Exception();

            return base.GenerateIniCategory(crossSectionDefinition);
        }

        protected override void AddMeasurementsProperties(ICrossSectionStandardShape crossSectionShape)
        {
            var cunetteShape = crossSectionShape as CrossSectionStandardShapeCunette;
            iniCategory.AddProperty(DefinitionRegion.CunetteWidth.Key, $"{cunetteShape.Width:0.00}");
        }
    }
}
