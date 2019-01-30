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

        protected override void SetStructureProperties(IStructure1D structure, IDelftIniCategory category)
        {
            var siphon = structure as Culvert;

            SetCommonCulvertProperties(siphon, category);
            SetInvertedSiphonProperties(siphon, category);
            SetSiphonProperties(siphon, category);
        }

        private static void SetSiphonProperties(ICulvert siphon, IDelftIniCategory category)
        {
            siphon.SiphonOnLevel = category.ReadProperty<double>(StructureRegion.TurnOnLevel.Key);
            siphon.SiphonOffLevel = category.ReadProperty<double>(StructureRegion.TurnOffLevel.Key);
        }
    }
}
