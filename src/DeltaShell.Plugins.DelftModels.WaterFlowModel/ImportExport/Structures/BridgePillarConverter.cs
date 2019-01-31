using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    public class BridgePillarConverter : BridgeConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Bridge
            {
                IsPillar = true
            };
        }

        protected override void SetStructureProperties()
        {
            var bridge = Structure as IBridge;
            SetCommonBridgeProperties(bridge);
            SetBridgePillarProperties(bridge);
        }

        private static void SetBridgePillarProperties(IBridge bridge)
        {
            bridge.PillarWidth = Category.ReadProperty<double>(StructureRegion.PillarWidth.Key);
            bridge.ShapeFactor = Category.ReadProperty<double>(StructureRegion.FormFactor.Key);
        }
    }
}