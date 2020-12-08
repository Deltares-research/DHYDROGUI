using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    public interface IWavmFileFunctionStore : IFMNetCdfFileFunctionStore
    {
        /// <summary>
        /// Gets the grid of this <see cref="WavmFileFunctionStore"/>.
        /// </summary>
        CurvilinearGrid Grid { get; }
    }
}