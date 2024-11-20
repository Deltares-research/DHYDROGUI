using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    /// <summary>
    /// Command to remove the selected Route.
    /// </summary>
    public class RemoveSelectedRouteCommand : NetworkEditorCommand, IGuiCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveSelectedRouteCommand"/> class.
        /// </summary>
        public RemoveSelectedRouteCommand()
        {
            RouteSelectionFinder = new RouteSelectionFinder();
        }
        
        public IRouteSelectionFinder RouteSelectionFinder { get; set; }

        public override bool Enabled => RouteSelectionFinder.IsRouteSelected(Gui);

        public IGui Gui { get; set; }

        protected override void OnExecute(params object[] arguments)
        {
            Route selectedRoute = RouteSelectionFinder.GetSelectedRoute(Gui);
            if (selectedRoute != null)
            {
                HydroNetworkHelper.RemoveRoute(selectedRoute);
            }
        }
    }
}