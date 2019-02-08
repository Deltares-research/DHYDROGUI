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

        protected override void SetStructurePropertiesFromCategory()
        {
            var invertedSiphon = Structure as Culvert;

            SetCommonCulvertProperties(invertedSiphon);
            SetInvertedSiphonProperties(invertedSiphon);
        }

        protected static void SetInvertedSiphonProperties(ICulvert invertedSiphon)
        {
            invertedSiphon.BendLossCoefficient = Category.ReadProperty<double>(StructureRegion.BendLossCoef.Key);
        }
    }
}
