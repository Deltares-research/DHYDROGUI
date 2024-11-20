using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows
{
    public interface IDataRowProvider
    {
        string Name { get; }
        IEnumerable<IDataRow> Rows { get; }
        event EventHandler RefreshRequired;
        void Disconnect();

        void ClearFilter();
        bool HasFilter();

        IRainfallRunoffModel Model { get; }
    }
}