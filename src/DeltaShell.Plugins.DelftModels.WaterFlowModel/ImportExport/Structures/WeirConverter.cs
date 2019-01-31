using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class WeirConverter : StructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new SimpleWeirFormula()
            };
        }

        protected override void SetStructureProperties()
        {
            var weir = Structure as Weir;
            var weirFormula = weir.WeirFormula as SimpleWeirFormula;

            weir.CrestLevel = Category.ReadProperty<double>(StructureRegion.CrestLevel.Key);
            weir.CrestWidth = Category.ReadProperty<double>(StructureRegion.CrestWidth.Key);

            weirFormula.DischargeCoefficient =
                Category.ReadProperty<double>(StructureRegion.DischargeCoeff.Key);
            weirFormula.LateralContraction =
                Category.ReadProperty<double>(StructureRegion.LatDisCoeff.Key);

            weir.FlowDirection =
                (FlowDirection)Category.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);
        }
    }
}
