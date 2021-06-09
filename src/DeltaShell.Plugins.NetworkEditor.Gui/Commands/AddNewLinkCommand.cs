using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using SharpMap.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class AddNewLinkCommand : Command
    {
        private IMapTool CurrentTool
        {
            get
            {
                return MapView.MapControl.GetToolByName(AddHydroLinkMapTool.ToolName);
            }
        }

        protected override void OnExecute(params object[] arguments)
        {
            var newLineTool = (NewArrowLineTool)CurrentTool;
            MapView.MapControl.ActivateTool(newLineTool);
        }

        public override bool Checked
        {
            get
            {
                if ((null != MapView) && (null != CurrentTool))
                    return CurrentTool.IsActive;
                else
                    return false;
            }
        }

        protected static MapView MapView
        {
            get
            {
                return NetworkEditorGuiPlugin.GetFocusedMapView();
            }
        }

        public override bool Enabled
        {
            get
            {
                var mapView = NetworkEditorGuiPlugin.GetFocusedMapView();

                if(mapView == null || mapView.Map == null)
                {
                    return false;
                }

                return mapView.Map.GetAllVisibleLayers(true).OfType<VectorLayer>().Any(l => l.DataSource.FeatureType == typeof(HydroLink));
            }
        }

    }
}