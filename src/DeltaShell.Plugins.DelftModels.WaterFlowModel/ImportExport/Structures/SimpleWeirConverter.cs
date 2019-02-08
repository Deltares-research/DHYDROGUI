using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class SimpleWeirConverter : StructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Weir
            {
                WeirFormula = new SimpleWeirFormula()
            };
        }

        protected override void SetStructurePropertiesFromCategory()
        {
            if (!(Structure is IWeir weir)) return;
            if (!(weir.WeirFormula is SimpleWeirFormula weirFormula)) return;

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
