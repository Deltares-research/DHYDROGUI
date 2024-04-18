using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.ImportExport.Sobek.Properties;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;
using BridgeType = DelftTools.Hydro.Structures.BridgeType;

namespace DeltaShell.Plugins.ImportExport.Sobek.Builders.HydroTypeInitializers
{
    public class DefaultBridgeInitializer : IBridgeInitializer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DefaultBridgeInitializer));
        public Dictionary<string, SobekCrossSectionDefinition> SobekCrossSectionDefinitions { get; }
        
        public DefaultBridgeInitializer(Dictionary<string, SobekCrossSectionDefinition> sobekCrossSectionDefinitions)
        {
            SobekCrossSectionDefinitions = sobekCrossSectionDefinitions;
        }

        public void Initialize(SobekBridge sobekBridge, IBridge bridge)
        {
            Ensure.NotNull(sobekBridge, nameof(sobekBridge));
            Ensure.NotNull(bridge, nameof(IBridge));

            if (sobekBridge.CrossSectionId != null &&
                SobekCrossSectionDefinitions != null &&
                SobekCrossSectionDefinitions.TryGetValue(sobekBridge.CrossSectionId, out SobekCrossSectionDefinition sobekCrossSectionDefinition) &&
                sobekCrossSectionDefinition !=null
                )
            {
                InitializeStandardBridgeGroundLayer(sobekCrossSectionDefinition, bridge);
                InitializeBridgeCrossSectionDefinition(sobekCrossSectionDefinition, bridge);
            }
            else
            {
                bridge.BridgeType = BridgeType.Rectangle;
            }
        }
        private static void InitializeStandardBridgeGroundLayer(SobekCrossSectionDefinition sobekCrossSectionDefinition, IBridge bridge)
        {
            bridge.GroundLayerEnabled = sobekCrossSectionDefinition.UseGroundLayer;
            bridge.GroundLayerThickness = sobekCrossSectionDefinition.GroundLayerDepth;
        }

        private static void InitializeBridgeCrossSectionDefinition(SobekCrossSectionDefinition sobekCrossSectionDefinition, IBridge bridge)
        {
            if (sobekCrossSectionDefinition.Type != SobekCrossSectionDefinitionType.Tabulated)
            {
                log.WarnFormat(Resources.DefaultBridgeInitializer_InitializeBridgeCrossSectionDefinition_Only_sobek2_bridge_geometric_profiles_of_type_tabular__0__supported_and_implemented_,
                               SobekCrossSectionDefinitionType.Tabulated);
                bridge.BridgeType = BridgeType.Rectangle;
                return;
            }

            var hfswData = CreateHeightFlowStorageWidths(sobekCrossSectionDefinition.TabulatedProfile, bridge.Shift);

            bridge.TabulatedCrossSectionDefinition.SetWithHfswData(hfswData);
            bridge.YZCrossSectionDefinition.SetWithHfswData(hfswData);

            if (sobekCrossSectionDefinition.Name != null &&
                sobekCrossSectionDefinition.Name.StartsWith("r_"))
            {
                SetPropertiesForRectangleBridge(sobekCrossSectionDefinition, bridge);
                bridge.BridgeType = BridgeType.Rectangle;
            }
            else
            {
                bridge.BridgeType = BridgeType.Tabulated;
            }
        }

        private static void SetPropertiesForRectangleBridge(SobekCrossSectionDefinition sobekCrossSectionDefinition, IBridge bridge)
        {
            //set the type of the bridge geometry and set width and height, bed level in the bridge
            bridge.Width = sobekCrossSectionDefinition.TabulatedProfile[1].TotalWidth;
            bridge.Height = sobekCrossSectionDefinition.TabulatedProfile[1].Height -
                            sobekCrossSectionDefinition.TabulatedProfile[0].Height;
        }

        private static HeightFlowStorageWidth[] CreateHeightFlowStorageWidths(IEnumerable<SobekTabulatedProfileRow> sobekTabulatedProfileRows, double bridgeShift)
        {
            return sobekTabulatedProfileRows
                   .Select(sobekTabulatedProfileRow => 
                               new HeightFlowStorageWidth(
                                   sobekTabulatedProfileRow.Height + bridgeShift, 
                                   sobekTabulatedProfileRow.TotalWidth, 
                                   sobekTabulatedProfileRow.FlowWidth)).ToArray();
        }
    }
}