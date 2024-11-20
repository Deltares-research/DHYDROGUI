using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DeltaShell.NGHS.IO.FileWriters.Network;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    /// <inheritdoc/>
    public class ConvertedUgridFileObjects : IConvertedUgridFileObjects
    {
        /// <inheritdoc/>
        public UnstructuredGrid Grid { get; set; }

        /// <inheritdoc/>
        public IHydroNetwork HydroNetwork { get; set; }

        /// <inheritdoc/>
        public IDiscretization Discretization { get; set; }

        /// <inheritdoc/>
        public IList<ILink1D2D> Links1D2D { get; set; }

        /// <inheritdoc/>
        public IEnumerable<CompartmentProperties> CompartmentProperties { get; set; }

        /// <inheritdoc/>
        public IEnumerable<BranchProperties> BranchProperties { get; set; }
    }
}