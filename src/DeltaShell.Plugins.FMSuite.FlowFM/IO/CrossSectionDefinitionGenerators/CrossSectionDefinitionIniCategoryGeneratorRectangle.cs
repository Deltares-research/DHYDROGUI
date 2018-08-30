using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators
{
    public class CrossSectionDefinitionIniCategoryGeneratorRectangle : ACrossSectionDefinitionIniCategoryGenerator
    {
        public override DelftIniCategory GenerateIniCategory(CrossSectionDefinitionStandard crossSectionDefinition)
        {
            var crossSectionStandardShape = crossSectionDefinition.Shape as CrossSectionStandardShapeRectangle;
            if (crossSectionStandardShape == null) throw new Exception();

            return base.GenerateIniCategory(crossSectionDefinition);
        }

        protected override void AddMeasurementsProperties(ICrossSectionStandardShape crossSectionShape)
        {
            var rectangleShape = crossSectionShape as CrossSectionStandardShapeRectangle;
            iniCategory.AddProperty(DefinitionPropertySettings.RectangleWidth.Key, $"{rectangleShape.Width:0.00}");
            iniCategory.AddProperty(DefinitionPropertySettings.RectangleHeight.Key, $"{rectangleShape.Height:0.00}");
        }
    }
}
