using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class ShowSideViewCommand : NetworkEditorCommand, IGuiCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveSelectedRouteCommand"/> class.
        /// </summary>
        public ShowSideViewCommand()
        {
            Log = LogManager.GetLogger(typeof(ShowSideViewCommand));
            RouteSelectionFinder = new RouteSelectionFinder();
        }

        public ILog Log { get; set; }
        public IRouteSelectionFinder RouteSelectionFinder { get; set; }

        public override bool Enabled => RouteSelectionFinder.IsRouteSelected(Gui);

        public IGui Gui { get; set; }

        protected override void OnExecute(params object[] arguments)
        {
            Route route = RouteSelectionFinder.GetSelectedRoute(Gui);

            if (route != null)
            {
                Gui.CommandHandler.OpenView(route, typeof(NetworkSideView));
            }
            else
            {
                Log.ErrorFormat("No active route found in active map; can not display sideview.");
            }
        }
    }
}