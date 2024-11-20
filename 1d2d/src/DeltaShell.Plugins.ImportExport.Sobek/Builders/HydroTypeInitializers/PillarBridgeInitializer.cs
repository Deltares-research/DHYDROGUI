using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using BridgeType = DelftTools.Hydro.Structures.BridgeType;

namespace DeltaShell.Plugins.ImportExport.Sobek.Builders.HydroTypeInitializers
{
    public class PillarBridgeInitializer : IBridgeInitializer
    {

        public void Initialize(SobekBridge sobekBridge, IBridge bridge)
        {
            Ensure.NotNull(sobekBridge, nameof(sobekBridge));
            Ensure.NotNull(bridge, nameof(IBridge));

            bridge.PillarWidth = sobekBridge.TotalPillarWidth;
            bridge.ShapeFactor = sobekBridge.FormFactor;
            bridge.BridgeType = BridgeType.Pillar;
        }

    }
}