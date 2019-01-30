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

        protected override void SetStructureProperties(IStructure1D structure, IDelftIniCategory category)
        {
            var bridge = structure as IBridge;
            SetCommonBridgeProperties(bridge, category);
            SetBridgePillarProperties(bridge, category);
        }

        private static void SetBridgePillarProperties(IBridge bridge, IDelftIniCategory category)
        {
            bridge.PillarWidth = category.ReadProperty<double>(StructureRegion.PillarWidth.Key);
            bridge.ShapeFactor = category.ReadProperty<double>(StructureRegion.FormFactor.Key);
        }
    }
}