using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// Interface for the wave map file function store.
    /// </summary>
    public interface IWavmFileFunctionStore : IFMNetCdfFileFunctionStore
    {
        /// <summary>
        /// Gets the grid of this <see cref="IWavmFileFunctionStore"/>.
        /// </summary>
        CurvilinearGrid Grid { get; }
    }
}