using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures
{
    /// <summary>
    /// This class is responsible for converting <see cref="IDelftIniCategory"/> objects into <see cref="Bridge"/> objects.
    /// </summary>
    /// <seealso cref="FrictionAndGroundLayerStructureConverter" />
    public class BridgeConverter : FrictionAndGroundLayerStructureConverter
    {
        protected override IStructure1D CreateNewStructure()
        {
            return new Bridge();
        }

        protected override void SetStructurePropertiesFromCategory(IList<string> warningMessages)
        {
            var bridge = Structure as IBridge;

            SetCommonBridgeProperties(bridge);
            SetStandardBridgeProperties(bridge);
            SetFrictionValuesFromCategory(bridge);
            SetGroundLayerValuesFromCategory(bridge);
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