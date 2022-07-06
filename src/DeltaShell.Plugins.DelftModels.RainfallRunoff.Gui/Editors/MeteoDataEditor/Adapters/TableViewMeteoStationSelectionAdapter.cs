using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Adapters
{
    /// <summary>
    /// <see cref="TableViewMeteoStationSelectionAdapter"/> implements <see cref="ITableViewMeteoStationSelectionAdapter"/>
    /// by interacting directly with a <see cref="ITableView"/>.
    /// </summary>
    public class TableViewMeteoStationSelectionAdapter : ITableViewMeteoStationSelectionAdapter
    {
        private readonly ITableView tableView;

        /// <summary>
        /// Creates a new <see cref="TableViewMeteoStationSelectionAdapter"/> with the given
        /// <paramref name="tableView"/>.
        /// </summary>
        /// <param name="tableView">The <see cref="ITableView"/> to select items in.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="tableView"/> is <c>null</c>.
        /// </exception>
        public TableViewMeteoStationSelectionAdapter(ITableView tableView)
        {
            Ensure.NotNull(tableView, nameof(tableView));
            this.tableView = tableView;
        }

        /// <inheritdoc />
        public void SetSelection(IEnumerable<string> selectedMeteoStations)
        {
            // Unfortunately, ITableView does not have any data binding and is a WinForms element.
            // In order to properly forward the selection, we need to interact directly with the element.
            // In order to abstract the ITableView from the view models, this adapter handles the direct
            // interaction logic.
            if (tableView.Columns == null) return;

            int selectedRow = tableView.FocusedRowIndex;
            tableView.ClearSelection();

            foreach (int column in selectedMeteoStations.Select(ToColumn))
            {
                tableView.SelectCells(selectedRow, column, selectedRow, column, false);
            }
        }

        private int ToColumn(string columnName)
        {
            return tableView.Columns.IndexOf(tableView.GetColumnByName(columnName));
        }
    }
}