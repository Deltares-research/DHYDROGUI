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
        public override bool Checked
        {
            get
            {
                if (null != MapView && null != CurrentTool)
                {
                    return CurrentTool.IsActive;
                }
                else
                {
                    return false;
                }
            }
        }

        public override bool Enabled
        {
            get
            {
                MapView mapView = NetworkEditorGuiPlugin.GetFocusedMapView();

                if (mapView == null || mapView.Map == null)
                {
                    return false;
                }

                // 4debug:
                VectorLayer layerWithoutDs = mapView.Map.GetAllVisibleLayers(true).OfType<VectorLayer>().FirstOrDefault(l => l.DataSource == null);

                return mapView.Map.GetAllVisibleLayers(true).OfType<VectorLayer>().Any(l => l.DataSource.FeatureType == typeof(HydroLink));
            }
        }

        protected static MapView MapView
        {
            get
            {
                return NetworkEditorGuiPlugin.GetFocusedMapView();
            }
        }

        protected override void OnExecute(params object[] arguments)
        {
            var newLineTool = (NewArrowLineTool) CurrentTool;
            MapView.MapControl.ActivateTool(newLineTool);
        }

        private IMapTool CurrentTool
        {
            get
            {
                return MapView.MapControl.GetToolByName(HydroRegionEditorMapTool.AddHydroLinkToolName);
            }
        }
    }
}