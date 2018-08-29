using System;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators
{
    public static class CrossSectionDefinitionIniCategoryGeneratorFactory
    {
        public static ICrossSectionDefinitionIniCategoryGenerator GetCrossSectionDefinitionIniCategoryGenerator(CrossSectionStandardShapeType shapeType)
        {
            switch (shapeType)
            {
                case CrossSectionStandardShapeType.Circle:
                    return new CrossSectionDefinitionIniCategoryGeneratorCircle();
                case CrossSectionStandardShapeType.Elliptical:
                    return new CrossSectionDefinitionIniCategoryGeneratorElliptical();
                case CrossSectionStandardShapeType.Rectangle:
                    return new CrossSectionDefinitionIniCategoryGeneratorRectangle();
                case CrossSectionStandardShapeType.Egg:
                    return new CrossSectionDefinitionIniCategoryGeneratorEgg();
                case CrossSectionStandardShapeType.Arch:
                    return new CrossSectionDefinitionIniCategoryGeneratorArch();
                case CrossSectionStandardShapeType.Cunette:
                    return new CrossSectionDefinitionIniCategoryGeneratorCunette();
                case CrossSectionStandardShapeType.SteelCunette:
                    return new CrossSectionDefinitionIniCategoryGeneratorSteelCunette();
                case CrossSectionStandardShapeType.Trapezium:
                    return new CrossSectionDefinitionIniCategoryGeneratorTrapezium();
                default:
                    throw new ArgumentOutOfRangeException(nameof(shapeType), shapeType, string.Empty);
            }
        }
    }
}
