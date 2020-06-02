using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class ShowSideViewCommand : NetworkEditorCommand, IGuiCommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ShowSideViewCommand));

        public override bool Enabled
        {
            get
            {
                return GetRoute() != null;
            }
        }

        public IGui Gui { get; set; }

        protected override void OnExecute(params object[] arguments)
        {
            Route route = GetRoute();

            if (route != null)
            {
                Gui.CommandHandler.OpenView(route, typeof(NetworkSideView));
            }
            else
            {
                log.ErrorFormat("No active route found in active map; can not display sideview.");
            }
        }

        private Route GetRoute()
        {
            // 1 Get coordinate(s) of currently selected features
            MapView mapView = GetMapView();

            if (mapView == null)
            {
                return null;
            }

            MapControl mapControl = mapView.MapControl;
            var hydroNetworkEditorMapTool = mapControl.GetToolByType<IHydroNetworkEditorMapTool>();

            return hydroNetworkEditorMapTool?.ActiveNetworkCoverageGroupLayer?.NetworkCoverage as Route;
        }

        private MapView GetMapView()
        {
            return Gui.DocumentViews.ActiveView?.GetViewsOfType<MapView>().FirstOrDefault();
        }
    }
}