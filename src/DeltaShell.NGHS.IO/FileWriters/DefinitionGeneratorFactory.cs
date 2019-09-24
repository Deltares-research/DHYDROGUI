using System;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;

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

        public static IDefinitionGeneratorStructure GetDefinitionGeneratorStructure(StructureType structureType, CompoundStructureInfo compoundStructureInfo)
        {
            switch (structureType)
            {
                case StructureType.Pump:
                    return new DefinitionGeneratorStructurePump(compoundStructureInfo);
                case StructureType.Weir:
                    return new DefinitionGeneratorStructureWeir(compoundStructureInfo);
                case StructureType.UniversalWeir:
                    return new DefinitionGeneratorStructureUniversalWeir(compoundStructureInfo);
                case StructureType.RiverWeir:
                    return new DefinitionGeneratorStructureRiverWeir(compoundStructureInfo);
                case StructureType.AdvancedWeir:
                    return new DefinitionGeneratorStructureAdvancedWeir(compoundStructureInfo);
                case StructureType.Orifice:
                    return new DefinitionGeneratorStructureOrifice(compoundStructureInfo);
                case StructureType.GeneralStructure:
                    return new DefinitionGeneratorStructureGeneralStructure(compoundStructureInfo);
                case StructureType.Culvert:
                    return new DefinitionGeneratorStructureCulvert(compoundStructureInfo);
                case StructureType.InvertedSiphon:
                    return new DefinitionGeneratorStructureInvertedSiphon(compoundStructureInfo);
                case StructureType.Siphon:
                    return new DefinitionGeneratorStructureSiphon(compoundStructureInfo);
                case StructureType.Bridge:
                    return new DefinitionGeneratorStructureBridgeStandard(compoundStructureInfo);
                case StructureType.BridgePillar:
                    return new DefinitionGeneratorStructureBridgePillar(compoundStructureInfo);
                case StructureType.ExtraResistance:
                    return new DefinitionGeneratorStructureExtraResistance(compoundStructureInfo);
                default:
                    return null;
            }
        }
    }
}