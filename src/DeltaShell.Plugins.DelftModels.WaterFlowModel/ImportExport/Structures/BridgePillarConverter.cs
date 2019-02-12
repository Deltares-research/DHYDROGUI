using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    /// <summary>
    /// This class is responsible for converting <see cref="IDelftIniCategory"/> objects into Bridge Pillars.
    /// Bridge Pillars are a special case of <see cref="Bridge"/> objects, where its <see cref="BridgeType"/> is equal to
    /// <see cref="BridgeType.Pillar"/>.
    /// </summary>
    /// <seealso cref="BridgeConverter" />
    public class BridgePillarConverter : BridgeConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Bridge
            {
                IsPillar = true
            };
        }

        protected override void SetStructurePropertiesFromCategory(IList<string> warningMessages)
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