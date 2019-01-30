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

            SetCommonBridgeProperties(bridge, category);
            SetStandardBridgeProperties(category, bridge);
        }

        private static void SetCommonBridgeProperties(IBridge bridge, IDelftIniCategory category)
        {
            bridge.BottomLevel = category.ReadProperty<double>(StructureRegion.BedLevel.Key);
            bridge.FlowDirection = (FlowDirection) category.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);
        }

        private static void SetStandardBridgeProperties(IDelftIniCategory category, IBridge bridge)
        {
            bridge.Length = category.ReadProperty<double>(StructureRegion.Length.Key);
            bridge.InletLossCoefficient = category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key);
            bridge.OutletLossCoefficient = category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key);
        }
    }
}