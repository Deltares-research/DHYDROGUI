using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators
{
    public class CrossSectionDefinitionIniCategoryGeneratorArch : ACrossSectionDefinitionIniCategoryGenerator
    {
        public override DelftIniCategory GenerateIniCategory(CrossSectionDefinitionStandard crossSectionDefinition)
        {
            var crossSectionShape = crossSectionDefinition.Shape as CrossSectionStandardShapeArch;
            if (crossSectionShape == null) throw new Exception();

            return base.GenerateIniCategory(crossSectionDefinition);
        }

        protected override void AddMeasurementsProperties(ICrossSectionStandardShape crossSectionShape)
        {
            var archShape = crossSectionShape as CrossSectionStandardShapeArch;
            iniCategory.AddProperty(DefinitionRegion.ArchCrossSectionWidth.Key, $"{archShape.Width:0.00}");
            iniCategory.AddProperty(DefinitionRegion.ArchCrossSectionHeight.Key, $"{archShape.Height:0.00}");
            iniCategory.AddProperty(DefinitionRegion.ArchHeight.Key, $"{archShape.ArcHeight:0.00}");
        }
    }
}
