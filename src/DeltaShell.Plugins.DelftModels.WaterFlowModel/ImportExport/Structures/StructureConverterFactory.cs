using DeltaShell.NGHS.IO.FileWriters.Structure;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public static class StructureConverterFactory
    {
        public static IStructureConverter GetStructureConverter(string type)
        {
            switch (type)
            {
                case StructureRegion.StructureTypeName.Pump:
                    return new PumpConverter();
                case StructureRegion.StructureTypeName.Weir:
                    return new WeirConverter();
                case StructureRegion.StructureTypeName.UniversalWeir:
                    return new UniversalWeirConverter();
                case StructureRegion.StructureTypeName.RiverWeir:
                    return new RiverWeirConverter();
                case StructureRegion.StructureTypeName.AdvancedWeir:
                    return new AdvancedWeirConverter();
                case StructureRegion.StructureTypeName.Orifice:
                    return new OrificeConverter();
                case StructureRegion.StructureTypeName.GeneralStructure:
                    return new GeneralStructureConverter();
                case StructureRegion.StructureTypeName.ExtraResistanceStructure:
                    return new ExtraResistanceConverter();
                case StructureRegion.StructureTypeName.Culvert:
                    return new CulvertConverter();
                case StructureRegion.StructureTypeName.InvertedSiphon:
                    return new InvertedSiphonConverter();
                case StructureRegion.StructureTypeName.Siphon:
                    return new SiphonConverter();
                case StructureRegion.StructureTypeName.Bridge:
                    return new BridgeConverter();
                case StructureRegion.StructureTypeName.BridgePillar:
                    return new BridgePillarConverter();
                default:
                    return null;
            }
        }
    }
}

