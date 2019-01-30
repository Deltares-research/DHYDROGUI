using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class InvertedSiphonConverter : CulvertConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Culvert
            {
                CulvertType = CulvertType.InvertedSiphon
            };
        }

        protected override void SetStructureProperties(IStructure1D structure, IDelftIniCategory category)
        {
            var invertedSiphon = structure as Culvert;

            SetCommonCulvertProperties(invertedSiphon, category);
            SetInvertedSiphonProperties(invertedSiphon, category);
        }

        protected static void SetInvertedSiphonProperties(ICulvert invertedSiphon, IDelftIniCategory category)
        {
            invertedSiphon.BendLossCoefficient = category.ReadProperty<double>(StructureRegion.BendLossCoef.Key);
        }
    }
}
