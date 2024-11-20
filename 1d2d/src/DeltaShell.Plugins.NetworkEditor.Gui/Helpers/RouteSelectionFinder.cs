using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Helpers
{
    /// <summary>
    /// Class for finding which route is selected.
    /// </summary>
    public class RouteSelectionFinder : IRouteSelectionFinder
    {
        /// <inheritdoc/>
        public bool IsRouteSelected(IGui gui)
        {
            return GetSelectedRoute(gui) != null;
        }

        /// <inheritdoc/>
        public Route GetSelectedRoute(IGui gui)
        {
            MapView mapView = GetMapView(gui);

            if (mapView == null)
            {
                return null;
            }

            MapControl mapControl = mapView.MapControl;
            var hydroNetworkEditorMapTool = mapControl.GetToolByType<IHydroNetworkEditorMapTool>();

            if (hydroNetworkEditorMapTool == null)
            {
                return null;
            }

            if (hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer == null)
            {
                return null;
            }

            return hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer.NetworkCoverage as Route;
        }

        private MapView GetMapView(IGui gui)
        {
            return gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().FirstOrDefault();
        }
    }
}