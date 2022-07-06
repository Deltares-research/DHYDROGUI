using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class ShowSideViewCommand : NetworkEditorCommand,IGuiCommand
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ShowSideViewCommand));

        protected override void OnExecute(params object[] arguments)
        {
            var route = GetRoute();

            if (route != null)
            {
                Gui.CommandHandler.OpenView(route, typeof (NetworkSideView));
            }
            else
            {
                log.ErrorFormat("No active route found in active map; can not display sideview.");
            }
        }
    
        public override bool Enabled
        {
            get { return GetRoute() != null; }
        }

        private Route GetRoute()
        {
            // 1 Get coordinate(s) of currently selected features
            var mapView = GetMapView();

            if (mapView == null)
            {
                return null;
            }

            var mapControl = mapView.MapControl;
            var hydroNetworkEditorMapTool = mapControl.GetToolByType<IHydroNetworkEditorMapTool>();

            if (null == hydroNetworkEditorMapTool)
            {
                return null;
            }

            if (null == hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer)
            {
                return null;
            }

            return hydroNetworkEditorMapTool.ActiveNetworkCoverageGroupLayer.NetworkCoverage as Route;
        }

        private MapView GetMapView()
        {
            //no mapview should give an exception..
            return Gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().FirstOrDefault();
        }

        public IGui Gui { get; set; }
    }
}