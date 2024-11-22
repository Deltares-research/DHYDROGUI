using System;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.FileReaders.Definition;
using DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public static class DefinitionGeneratorFactory
    {
        public static IDefinitionReader<ICrossSectionDefinition> GetDefinitionReaderCrossSection(string type, string template = "")
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
                case CrossSectionRegion.CrossSectionDefinitionType.Circle:
                    return new CSDCircleDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Zw_Template:
                    return GetStandardDefinitionReaderCrossSection(template);
                default :
                    return null;
            }
        }

        private static IDefinitionReader<ICrossSectionDefinition> GetStandardDefinitionReaderCrossSection(string template)
        {
            switch (template)
            {
                case CrossSectionRegion.CrossSectionDefinitionType.Elliptical:
                    return new CSDEllipseDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Egg:
                    return new CSDEggDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.InvertedEgg:
                    return new CSDInvertedEggDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Arch:
                    return new CSDArchDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.UShape:
                    return new CSDUShapeDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Mouth:
                    return new CSDCunetteDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.SteelMouth:
                    return new CSDSteelCunetteDefinitionReader();
                case CrossSectionRegion.CrossSectionDefinitionType.Trapezium:
                    return new CSDTrapeziumDefinitionReader();
                default:
                    return new CSDZWDefinitionReader();
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
                    definitionGeneratorCrossSectionDefinition = DefinitionIniSectionGeneratorFactory.GetCrossSectionDefinitionIniSectionGenerator(crossSectionDefinition);
                    break;
            }
            return definitionGeneratorCrossSectionDefinition;
        }

        public static IDefinitionGeneratorLocation GetDefinitionGeneratorLocation(IBranchFeature branchFeature, bool obsCrs = false)
        {
            if (branchFeature is ICrossSection)
            {
                var branchFeatureBranch = branchFeature.Branch;
                if (branchFeatureBranch is Channel)
                {
                    return new DefinitionGeneratorCrossSectionLocationForChannel(CrossSectionRegion.IniHeader);
                }
                if(branchFeatureBranch is ISewerConnection)
                {
                    return new DefinitionGeneratorCrossSectionLocationForSewerConnection(CrossSectionRegion.IniHeader);
                }
            }
            if (branchFeature is IObservationPoint)
            {
                return new DefinitionGeneratorLocation(obsCrs? ObservationPointRegion.IniHeaderCrs : ObservationPointRegion.IniHeader);
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
                    return new DefinitionGeneratorStructurePump2D();
                case Structure2DType.Weir:
                    return new DefinitionGeneratorStructureWeir2D();
                case Structure2DType.GeneralStructure:
                    return new DefinitionGeneratorStructureGeneralStructure2D();
                case Structure2DType.Gate:
                    return new DefinitionGeneratorStructureGate2D();
                case Structure2DType.LeveeBreach:
                    return new DefinitionGeneratorStructureLeveeBreach2D(referenceDateTime);
                default:
                    return null;
            }
        }

        public static IDefinitionGeneratorStructure GetDefinitionGeneratorStructure(StructureType structureType)
        {
            switch (structureType)
            {
                case StructureType.Pump:
                    return new DefinitionGeneratorStructurePump(new StructureBcFileNameGenerator());
                case StructureType.Weir:
                    return new DefinitionGeneratorStructureWeir(new StructureBcFileNameGenerator());
                case StructureType.UniversalWeir:
                    return new DefinitionGeneratorStructureUniversalWeir(new StructureBcFileNameGenerator());
                case StructureType.RiverWeir:
                    return new DefinitionGeneratorStructureRiverWeir();
                case StructureType.AdvancedWeir:
                    return new DefinitionGeneratorStructureAdvancedWeir();
                case StructureType.Orifice:
                    return new DefinitionGeneratorStructureOrifice(new StructureBcFileNameGenerator());
                case StructureType.GeneralStructure:
                    return new DefinitionGeneratorStructureGeneralStructure(new StructureBcFileNameGenerator());
                case StructureType.Culvert:
                    return new DefinitionGeneratorStructureCulvert(new StructureBcFileNameGenerator());
                case StructureType.InvertedSiphon:
                    return new DefinitionGeneratorStructureInvertedSiphon(new StructureBcFileNameGenerator());
                case StructureType.Bridge:
                    return new DefinitionGeneratorStructureBridgeStandard();
                case StructureType.BridgePillar:
                    return new DefinitionGeneratorStructureBridgePillar();
                default:
                    return null;
            }
        }
    }
}