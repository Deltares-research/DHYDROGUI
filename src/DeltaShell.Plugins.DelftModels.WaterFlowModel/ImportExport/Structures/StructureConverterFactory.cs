using DeltaShell.NGHS.IO.FileWriters.Structure;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public static class StructureConverterFactory
    {
        public static IStructureConverter GetStructureConverter(string type)
        {

            IStructureConverter converter = null;

            switch (type)
            {
                case StructureRegion.StructureTypeName.Pump:
                    converter = new PumpConverter();
                    break;
                case StructureRegion.StructureTypeName.Weir:
                    converter = new WeirConverter();
                    break;
                case StructureRegion.StructureTypeName.UniversalWeir:
                    converter = new UniversalWeirConverter();
                    break;
                case StructureRegion.StructureTypeName.RiverWeir:
                    converter = new RiverWeirConverter();
                    break;
                case StructureRegion.StructureTypeName.AdvancedWeir:
                    converter = new AdvancedWeirConverter();
                    break;
                case StructureRegion.StructureTypeName.Orifice:
                    converter = new OrificeConverter();
                    break;
                case StructureRegion.StructureTypeName.GeneralStructure:
                    converter = new GeneralStructureConverter();
                    break;
                case StructureRegion.StructureTypeName.ExtraResistanceStructure:
                    converter = new ExtraResistanceConverter();
                    break;
                case StructureRegion.StructureTypeName.Culvert:
                    converter = new CulvertConverter();
                    break;
            }

            return converter;

        }
    }

}

