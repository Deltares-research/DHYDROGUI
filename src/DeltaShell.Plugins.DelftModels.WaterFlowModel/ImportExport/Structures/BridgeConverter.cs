using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class BridgeConverter : AStructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Bridge();
        }

        protected override void SetStructureProperties(IStructure1D structure, IDelftIniCategory category)
        {
            var bridge = structure as IBridge;

            bridge.Length = category.ReadProperty<double>(StructureRegion.Length.Key);
            bridge.InletLossCoefficient = category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key);
            bridge.OutletLossCoefficient = category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key);
        }
    }
}