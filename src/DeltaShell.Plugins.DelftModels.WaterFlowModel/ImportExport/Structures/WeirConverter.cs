using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class WeirConverter : AStructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new SimpleWeirFormula()
            };
        }

        protected override void SetStructureProperties(IStructure1D structure, IDelftIniCategory category)
        {
            var weir = structure as Weir;
            var weirFormula = weir.WeirFormula as SimpleWeirFormula;

            weir.CrestLevel = category.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = category.ReadProperty<double>(StructureRegion.CrestWidth.Key);

            weirFormula.DischargeCoefficient =
                category.ReadProperty<double>(StructureRegion.DischargeCoeff.Key);
            weirFormula.LateralContraction =
                category.ReadProperty<double>(StructureRegion.LatDisCoeff.Key);

            weir.FlowDirection =
                (FlowDirection)category.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);
        }
    }
}
