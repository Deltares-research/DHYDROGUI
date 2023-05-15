using DelftTools.Shell.Gui;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Helpers
{
    /// <summary>
    /// Interface for finding which route is selected.
    /// </summary>
    public interface IRouteSelectionFinder
    {
        /// <summary>
        /// Determine if a route is selected.
        /// </summary>
        /// <param name="gui">Graphical user interface logic.</param>
        /// <returns><c>true</c> when a route is selected, <c>false</c> when no route is selected.</returns>
        bool IsRouteSelected(IGui gui);

        /// <summary>
        /// Get the selected <see cref="Route"/>.
        /// </summary>
        /// <param name="gui">Graphical user interface logic.</param>
        /// <returns>Selected <see cref="Route"/>.</returns>
        Route GetSelectedRoute(IGui gui);
    }
}