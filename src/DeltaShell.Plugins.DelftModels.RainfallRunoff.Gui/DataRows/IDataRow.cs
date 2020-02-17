using System.Collections.Generic;
using DelftTools.Controls;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows
{
    public interface IDataRow
    {
        void SetColumnEditorForDataWithModel(RainfallRunoffModel model, IEnumerable<ITableViewColumn> tableViewColumns);
    }
}