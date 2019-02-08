using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class SiphonConverter : InvertedSiphonConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Culvert
            {
                CulvertType = CulvertType.Siphon
            };
        }

        protected override void SetStructurePropertiesFromCategory()
        {
            var siphon = Structure as Culvert;

            SetCommonCulvertProperties(siphon);
            SetInvertedSiphonProperties(siphon);
            SetSiphonProperties(siphon);
        }

        private static void SetSiphonProperties(ICulvert siphon)
        {
            siphon.SiphonOnLevel = Category.ReadProperty<double>(StructureRegion.TurnOnLevel.Key);
            siphon.SiphonOffLevel = Category.ReadProperty<double>(StructureRegion.TurnOffLevel.Key);
        }
    }
}
