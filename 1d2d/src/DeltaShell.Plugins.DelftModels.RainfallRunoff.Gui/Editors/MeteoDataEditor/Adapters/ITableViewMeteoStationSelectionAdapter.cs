using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Adapters
{
    /// <summary>
    /// <see cref="ITableViewMeteoStationSelectionAdapter"/> provides an abstraction to select
    /// specific meteo stations in a ITableView containing meteo stations.
    /// </summary>
    public interface ITableViewMeteoStationSelectionAdapter
    {
        /// <summary>
        /// Set the selection within the table to the specified meteo stations.
        /// </summary>
        /// <param name="selectedMeteoStations">The meteo stations to select.</param>
        void SetSelection(IEnumerable<string> selectedMeteoStations);
    }
}
