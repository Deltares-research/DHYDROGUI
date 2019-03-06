using System;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileReaders.Definition;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters
{
    public static class DefinitionGeneratorFactory
    {
        public static IDefinitionReader GetDefinitionReaderCrossSection(string type)
        {
            switch (type)
            {
                case CrossSectionRegion.CrossSectionDefinitionType.Yz:
                    return new CSDYZDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Xyz:
                    return new CSDXYZDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Zw:
                    return new CSDZWDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Rectangle:
                    return new CSDRectangleDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Elliptical:
                    return new CSDEllipseDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Circle:
                    return new CSDCircleDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Egg:
                    return new CSDEggDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Arch:
                    return new CSDArchDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Cunette:
                    return new CSDCunetteDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.SteelCunette:
                    return new CSDSteelCunetteDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Trapezium:
                    return new CSDTrapeziumDefinitionReader();
                default :
                    return null;
            }
        }

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
        
        public static IDefinitionGeneratorLocation GetDefinitionGeneratorLocation(IBranchFeature branchFeature)
        {
            if (branchFeature is ICrossSection)
            {
                return new DefinitionGeneratorCrossSectionLocation(CrossSectionRegion.IniHeader);
            }
            if (branchFeature is IObservationPoint)
            {
                return new DefinitionGeneratorLocation(ObservationPointRegion.IniHeader);
            }
            if (branchFeature is ILateralSource)
            {
                return new DefinitionGeneratorLateralSourceLocation(BoundaryRegion.LateralDischargeHeader);
            }
            return null;

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

        public static IDefinitionReader GetDefinitionReaderStructure(StructureType structureType)
        {
            switch (structureType)
            {
                case StructureType.Unknown:
                    break;
                case StructureType.Bridge:
                    break;
                case StructureType.BridgePillar:
                    break;
                case StructureType.CompositeBranchStructure:
                    break;
                case StructureType.Culvert:
                    break;
                case StructureType.InvertedSiphon:
                    break;
                case StructureType.Siphon:
                    break;
                case StructureType.ExtraResistance:
                    break;
                case StructureType.Gate:
                    break;
                case StructureType.Pump:
                    break;
                case StructureType.Weir:
                    return new WeirDefinitionReader();
                case StructureType.UniversalWeir:
                    break;
                case StructureType.RiverWeir:
                    break;
                case StructureType.AdvancedWeir:
                    break;
                case StructureType.Orifice:
                    break;
                case StructureType.GeneralStructure:
                    break;
                default:
                    return null;
            }
            return null;

        }
    }
}