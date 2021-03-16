using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Helpers
{
    public static class HydroRegionEditorHelper
    {
        /// <summary>
        /// Initializes the network interactor if a network layer is part of the map. This method
        /// is called when a mapview gains or looses focus.
        /// </summary>
        /// <param name="mapControl">The map control from which the tools are obtained.</param>
        public static HydroRegionEditorMapTool AddHydroRegionEditorMapTool(IMapControl mapControl)
        {
            if (mapControl == null)
            {
                return null;
            }

            if (mapControl.Map == null)
            {
                return null;
            }

            var hydroRegionEditorMapTool = mapControl.GetToolByType<HydroRegionEditorMapTool>();

            // networkeditor already present; nothing to do.
            if (hydroRegionEditorMapTool != null)
            {
                return hydroRegionEditorMapTool;
            }

            hydroRegionEditorMapTool = new HydroRegionEditorMapTool
            {
                IsActive = true,
                TopologyRulesEnabled = true
            };

            mapControl.Tools.Add(hydroRegionEditorMapTool);

            return hydroRegionEditorMapTool;
        }

        public static void RemoveHydroRegionEditorMapTool(IMapControl mapControl)
        {
            var hydroRegionEditorMapTool = mapControl.GetToolByType<HydroRegionEditorMapTool>();
            if (hydroRegionEditorMapTool != null)
            {
                //set the mapcontrol to null to be sure the tool unsubscribes from map
                hydroRegionEditorMapTool.MapControl = null;
                mapControl.Tools.Remove(hydroRegionEditorMapTool);
            }
        }

        public static IHydroRegion RootGetHydroRegion(MapView view)
        {
            return view.Map.GetAllVisibleLayers(true).OfType<HydroRegionMapLayer>().Select(l => l.Region).FirstOrDefault(r => r != null && r.Parent == null);
        }
    }
}