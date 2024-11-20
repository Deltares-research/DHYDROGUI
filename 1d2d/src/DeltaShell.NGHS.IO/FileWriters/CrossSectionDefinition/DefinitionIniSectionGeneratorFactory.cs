using System;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public static class DefinitionIniSectionGeneratorFactory
    {
        public static DefinitionGeneratorCrossSectionDefinition GetCrossSectionDefinitionIniSectionGenerator(ICrossSectionDefinition crossSectionDefinition)
        {
            var standardCrossSectionDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
            return standardCrossSectionDefinition != null ? GetCrossSectionDefinitionIniSectionGenerator(standardCrossSectionDefinition.ShapeType) : null;
        }

        public static DefinitionGeneratorCrossSectionDefinitionStandard GetCrossSectionDefinitionIniSectionGenerator(CrossSectionStandardShapeType shapeType)
        {
            switch (shapeType)
            {
                case CrossSectionStandardShapeType.Circle:
                    return new DefinitionGeneratorCrossSectionDefinitionCircle();
                case CrossSectionStandardShapeType.Elliptical:
                    return new DefinitionGeneratorCrossSectionDefinitionElliptical();
                case CrossSectionStandardShapeType.Rectangle:
                    return new DefinitionGeneratorCrossSectionDefinitionRectangle();
                case CrossSectionStandardShapeType.InvertedEgg:
                    return new DefinitionGeneratorCrossSectionDefinitionInvertedEgg();
                case CrossSectionStandardShapeType.Egg:
                    return new DefinitionGeneratorCrossSectionDefinitionEgg();
                case CrossSectionStandardShapeType.Arch:
                    return new DefinitionGeneratorCrossSectionDefinitionArch();
                case CrossSectionStandardShapeType.UShape:
                    return new DefinitionGeneratorCrossSectionDefinitionUShape();
                case CrossSectionStandardShapeType.Cunette:
                    return new DefinitionGeneratorCrossSectionDefinitionCunette();
                case CrossSectionStandardShapeType.SteelCunette:
                    return new DefinitionGeneratorCrossSectionDefinitionSteelCunette();
                case CrossSectionStandardShapeType.Trapezium:
                    return new DefinitionGeneratorCrossSectionDefinitionTrapezium();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
