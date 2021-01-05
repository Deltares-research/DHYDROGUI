using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    /// <summary>
    /// Interface for the class map file function store.
    /// </summary>
    public interface IFMClassMapFileFunctionStore : IFMNetCdfFileFunctionStore
    {
        UnstructuredGrid Grid { get; }
    }
}