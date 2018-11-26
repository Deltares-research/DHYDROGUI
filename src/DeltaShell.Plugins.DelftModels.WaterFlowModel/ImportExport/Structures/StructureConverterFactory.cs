using DeltaShell.NGHS.IO.FileWriters.Structure;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public static class StructureConverterFactory
    {
        public static IStructureConverter GetSpecificConverter(string type)
        {

            IStructureConverter converter = null;

            switch (type)
            {
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
            }

            return converter;

        }
    }

}

