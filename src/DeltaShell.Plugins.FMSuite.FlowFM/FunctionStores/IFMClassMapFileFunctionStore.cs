using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    /// <summary>
    /// Interface for the class map file function store.
    /// </summary>
    public interface IFMClassMapFileFunctionStore : IFMNetCdfFileFunctionStore
    {
        UnstructuredGrid Grid { get; }
        ICoordinateSystem CoordinateSystem { get; }
        IDiscretization Discretization { get; }
        IHydroNetwork Network { get; }
        IList<ILink1D2D> Links { get; }
    }
}