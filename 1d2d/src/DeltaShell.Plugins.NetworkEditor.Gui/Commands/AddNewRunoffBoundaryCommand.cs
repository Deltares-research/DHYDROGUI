using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class AddNewRunoffBoundaryCommand : Command
    {
        private IMapTool CurrentTool
        {
            get
            {
                return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.AddRunoffBoundaryToolName);
            }
        }

        protected override void OnExecute(params object[] arguments)
        {
            var newLineTool = (NewPointFeatureTool)CurrentTool;
            MapView.MapControl.ActivateTool(newLineTool);
        }

        public override bool Checked
        {
            get { return (null != MapView) && (null != CurrentTool) && CurrentTool.IsActive; }
        }

        private static MapView MapView { get { return NetworkEditorGuiPlugin.GetFocusedMapView(); } }

        public override bool Enabled
        {
            get
            {
                var mapView = NetworkEditorGuiPlugin.GetFocusedMapView();

                return mapView != null &&
                       mapView.Map != null && // Can be null when closing GUI, with HydroNetworkEditor opened next to other View
                       mapView.Map.GetAllVisibleLayers(true).OfType<HydroRegionMapLayer>().FirstOrDefault(l => l.Region is IDrainageBasin) != null;
            }
        }

    }
}