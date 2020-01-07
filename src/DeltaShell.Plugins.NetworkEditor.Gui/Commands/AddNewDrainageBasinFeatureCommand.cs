using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    /// <summary>
    /// <see cref="AddNewDrainageBasinFeatureCommand"/> provides the command to add features
    /// dependent on the provided map tool name.
    /// </summary>
    /// <seealso cref="Command" />
    public class AddNewDrainageBasinFeatureCommand : Command
    {
        private readonly string mapToolName;

        /// <summary>
        /// Creates a new <see cref="AddNewDrainageBasinFeatureCommand"/>.
        /// </summary>
        /// <param name="mapToolName">Name of the map tool.</param>
        public AddNewDrainageBasinFeatureCommand(string mapToolName)
        {
            this.mapToolName = mapToolName;
        }

        private IMapTool CurrentTool => MapView.MapControl.GetToolByName(mapToolName);

        protected override void OnExecute(params object[] arguments)
        {
            var newLineTool = (NewPointFeatureTool)CurrentTool;
            MapView.MapControl.ActivateTool(newLineTool);
        }

        public override bool Checked => MapView != null && null != CurrentTool && CurrentTool.IsActive;

        private static MapView MapView => NetworkEditorGuiPlugin.GetFocusedMapView();

        public override bool Enabled
        {
            get
            {
                MapView mapView = NetworkEditorGuiPlugin.GetFocusedMapView();

                return mapView != null &&
                       mapView.Map != null && // Can be null when closing GUI, with HydroNetworkEditor opened next to other View
                       mapView.Map.GetAllVisibleLayers(true).OfType<HydroRegionMapLayer>().FirstOrDefault(l => l.Region is DrainageBasin) != null;
            }
        }
    }
}