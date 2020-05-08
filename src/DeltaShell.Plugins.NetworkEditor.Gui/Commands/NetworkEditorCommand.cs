using System.Linq;
using DelftTools.Controls;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    /// <summary>
    /// NetworkEditorCommand is an abstract class that facilitates the implementation of network related commands.
    /// </summary>
    public abstract class NetworkEditorCommand : Command
    {
        public override bool Enabled
        {
            get
            {
                MapView mapView = NetworkEditorGuiPlugin.GetFocusedMapView();

                return mapView != null && mapView.Map != null && mapView.MapControl.GetToolByType<HydroRegionEditorMapTool>() != null &&
                       mapView.Map.GetAllVisibleLayers(true).OfType<HydroRegionMapLayer>().Any(l => l.Region is INetwork);
            }
        }

        protected static MapView MapView
        {
            get
            {
                return NetworkEditorGuiPlugin.GetFocusedMapView();
            }
        }
    }
}