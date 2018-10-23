using System;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.FileReaders.Definition;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public static class DefinitionGeneratorFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DefinitionGeneratorFactory));

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

        public static DefinitionGeneratorCrossSectionDefinition GetDefinitionGeneratorCrossSection(ICrossSectionDefinition crossSectionDefinition)
        {
            DefinitionGeneratorCrossSectionDefinition definitionGeneratorCrossSectionDefinition = null;

            var crossSectionType = crossSectionDefinition.CrossSectionType;
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
                    definitionGeneratorCrossSectionDefinition = DefinitionIniCategoryGeneratorFactory.GetCrossSectionDefinitionIniCategoryGenerator(crossSectionDefinition);
                    break;
            }
            return definitionGeneratorCrossSectionDefinition;
        }
      

        public static IDefinitionGeneratorLocation GetDefinitionGeneratorLocation(IBranchFeature branchFeature)
        {
            if (branchFeature is ICrossSection)
            {
                var branchFeatureBranch = branchFeature.Branch;
                if (branchFeatureBranch is Channel)
                {
                    return new DefinitionGeneratorCrossSectionLocationForChannel(CrossSectionRegion.IniHeader);
                }
                if(branchFeatureBranch is Pipe)
                {
                    return new DefinitionGeneratorCrossSectionLocationForPipe(CrossSectionRegion.IniHeader);
                }
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

        public static DefinitionGeneratorStructure2D GetDefinitionGeneratorStructure(Structure2DType structureType, DateTime? referenceDateTime)
        {
            switch (structureType)
            {
                case Structure2DType.Pump:
                    return new DefinitionGeneratorStructurePump2D(null);
                case Structure2DType.Weir:
                    return new DefinitionGeneratorStructureWeir2D(null);
                case Structure2DType.GeneralStructure:
                    return new DefinitionGeneratorStructureGeneralStructure2D(null);
                case Structure2DType.Gate:
                    return new DefinitionGeneratorStructureGate2D(null);
                case Structure2DType.LeveeBreach:
                    return new DefinitionGeneratorStructureLeveeBreach2D(referenceDateTime);
                default:
                    return null;
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