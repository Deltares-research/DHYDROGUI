using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class BridgeConverter : FrictionAndGroundLayerStructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Bridge();
        }

        protected override void SetStructurePropertiesFromCategory()
        {
            var bridge = Structure as IBridge;

            SetCommonBridgeProperties(bridge);
            SetStandardBridgeProperties(bridge);
            SetFrictionValues(bridge);
            SetGroundLayerValues(bridge);
        }

        protected static void SetCommonBridgeProperties(IBridge bridge)
        {
            bridge.BottomLevel = Category.ReadProperty<double>(StructureRegion.BedLevel.Key);
            bridge.FlowDirection = (FlowDirection) Category.ReadProperty<int>(StructureRegion.AllowedFlowDir.Key);
        }

        private static void SetStandardBridgeProperties(IBridge bridge)
        {
            bridge.Length = Category.ReadProperty<double>(StructureRegion.Length.Key);
            bridge.InletLossCoefficient = Category.ReadProperty<double>(StructureRegion.InletLossCoeff.Key);
            bridge.OutletLossCoefficient = Category.ReadProperty<double>(StructureRegion.OutletLossCoeff.Key);
        }
    }
}