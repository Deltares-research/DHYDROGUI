using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    /// <summary>
    /// Interface for the his file function store.
    /// </summary>
    public interface IFMHisFileFunctionStore : IFMNetCdfFileFunctionStore
    {
        /// <summary>
        /// Gets or sets the coordinate system.
        /// </summary>
        ICoordinateSystem CoordinateSystem { get; set; }
    }
}