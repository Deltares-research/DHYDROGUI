using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using GeoAPI.Extensions.CoordinateSystems;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    /// <summary>
    /// Interface for the map file function store.
    /// </summary>
    public interface IFMMapFileFunctionStore : IFMNetCdfFileFunctionStore
    {
        /// <summary>
        /// Gets the grid of this <see cref="IFMMapFileFunctionStore"/>.
        /// </summary>
        UnstructuredGrid Grid { get; }

        /// <summary>
        /// Gets the boundary cell values.
        /// </summary>
        IList<ITimeSeries> BoundaryCellValues { get; }
        
        /// <summary>
        /// Sets the coordinate system of the file function store.
        /// </summary>
        /// <param name="coordinateSystem">The <see cref="ICoordinateSystem"/> to set.</param>
        void SetCoordinateSystem(ICoordinateSystem coordinateSystem);

        /// <summary>
        /// Gets the custom velocity coverage function.
        /// </summary>
        IFunction CustomVelocityCoverage { get; }
        
        /// <summary>
        /// Gets the function groupings.
        /// </summary>
        /// <returns>A collection of function groupings.</returns>
        IEnumerable<IGrouping<string, IFunction>> GetFunctionGrouping();
    }
}