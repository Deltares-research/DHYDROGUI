using System;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;

namespace DeltaShell.NGHS.IO.FileWriters
{
    public static class DefinitionGeneratorFactory
    {
        public static DefinitionGeneratorCrossSectionDefinition GetDefinitionGeneratorCrossSection(ICrossSectionDefinition crossSectionDefinition,
                                                                                                   CrossSectionType crossSectionType)
        {
            DefinitionGeneratorCrossSectionDefinition definitionGeneratorCrossSectionDefinition = null;

            switch (crossSectionType)
            {
                case CrossSectionType.GeometryBased:
                    definitionGeneratorCrossSectionDefinition = new DefinitionGeneratorCrossSectionDefinitionXyz();
                    break;
                case CrossSectionType.YZ:
                    definitionGeneratorCrossSectionDefinition = new DefinitionGeneratorCrossSectionDefinitionYz();
                    break;
                case CrossSectionType.ZW:
                    definitionGeneratorCrossSectionDefinition = new DefinitionGeneratorCrossSectionDefinitionZw();
                    break;
                case CrossSectionType.Standard:
                    var standardCrossSectionDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
                    if (standardCrossSectionDefinition != null)
                    {
                        definitionGeneratorCrossSectionDefinition =
                            GetDefinitionGeneratorCrossSectionStandard(standardCrossSectionDefinition.ShapeType);
                    }
                    break;

                default: throw new NotSupportedException();
            }

            return definitionGeneratorCrossSectionDefinition;
        }

        private static DefinitionGeneratorCrossSectionDefinition GetDefinitionGeneratorCrossSectionStandard(CrossSectionStandardShapeType shapeType)
        {
            switch (shapeType)
            {
                case CrossSectionStandardShapeType.Round:
                    return new DefinitionGeneratorCrossSectionDefinitionCircle();
                case CrossSectionStandardShapeType.Elliptical:
                    return new DefinitionGeneratorCrossSectionDefinitionElliptical();
                case CrossSectionStandardShapeType.Rectangle:
                    return new DefinitionGeneratorCrossSectionDefinitionRectangle();
                case CrossSectionStandardShapeType.Egg:
                    return new DefinitionGeneratorCrossSectionDefinitionEgg();
                case CrossSectionStandardShapeType.Arch:
                    return new DefinitionGeneratorCrossSectionDefinitionArch();
                case CrossSectionStandardShapeType.Cunette:
                    return new DefinitionGeneratorCrossSectionDefinitionCunette();
                case CrossSectionStandardShapeType.SteelCunette:
                    return new DefinitionGeneratorCrossSectionDefinitionSteelCunette();
                case CrossSectionStandardShapeType.Trapezium:
                    return new DefinitionGeneratorCrossSectionDefinitionTrapezium();
                default:
                    return null;//new DefinitionGeneratorCrossSectionDefinitionStandard();
            }
        }
    }
}