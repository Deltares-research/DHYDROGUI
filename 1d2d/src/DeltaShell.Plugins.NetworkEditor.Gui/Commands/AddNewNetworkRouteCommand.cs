using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class AddNewNetworkRouteCommand : NetworkEditorCommand
    {
        protected override void OnExecute(params object[] arguments)
        {
            var hydroNetwork = HydroRegionEditorMapTool.HydroRegions.OfType<IHydroNetwork>().FirstOrDefault();

            if (hydroNetwork == null || !hydroNetwork.Branches.Any())
            {
                return;
            }

            HydroNetworkHelper.AddNewRouteToNetwork(hydroNetwork);

            var activeNetworkCoverageGroupLayer = ((GroupLayer) MapView.GetLayerForData(hydroNetwork.Routes)).Layers.OfType<INetworkCoverageGroupLayer>().LastOrDefault();
            if (activeNetworkCoverageGroupLayer != null)
            {
                activeNetworkCoverageGroupLayer.Visible = true;
            }
            
            HydroRegionEditorMapTool.ActiveNetworkCoverageGroupLayer = activeNetworkCoverageGroupLayer;

            MapView.MapControl.ActivateTool(MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.AddNetworkLocationToolName));
        }
        
        private static HydroRegionEditorMapTool HydroRegionEditorMapTool
        {
            get
            {
                if (null != MapView)
                {
                    var mapControl = MapView.MapControl;
                    return mapControl.GetToolByType<HydroRegionEditorMapTool>();
                }
                return null;
            }
        }
        
    }
}