using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DeltaShell.NGHS.IO.FileWriters.Network;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    /// <summary>
    /// Used to collect all object needed for a ugrid file to generate a 1d2d link administration for our DOM.
    /// </summary>
    public interface IConvertedUgridFileObjects
    {
        /// <summary>
        /// Unstructured grid contained in the UGRID file created from UGRID Mesh2D
        /// </summary>
        UnstructuredGrid Grid { get; set; }

        /// <summary>
        /// Network geometry contained in the UGRID file created from UGRID NetworkGeometry.
        /// Using <seealso cref="IHydroNetwork"/> because this contains information about
        /// the coordinate space the network geometry is in. Using this the network from
        /// the UGRID file can be sized correctly on the map.
        /// </summary>
        IHydroNetwork HydroNetwork { get; set; }
        
        /// <summary>
        /// Discretization contained in the UGRID file created from UGRID Mesh1D
        /// </summary>
        IDiscretization Discretization { get; set; }
        
        /// <summary>
        /// Link1D2D administration contained in the UGRID file created from
        /// UGRID ContactLink administration, Mesh1D, Mesh2D and the Network geometry.
        /// It should be modified by the <seealso cref="Grid"/>, the <seealso cref="HydroNetwork"/>
        /// and the <seealso cref="Discretization"/> to be placed correctly on the map.
        /// </summary>
        IList<ILink1D2D> Links1D2D { get; set; }
        
        /// <summary>
        /// Additional properties for creating <seealso cref="CompartmentProperties">Compartment Properties</seealso> in our DOM.
        /// </summary>
        IEnumerable<CompartmentProperties> CompartmentProperties { get; set; }
        
        /// <summary>
        /// Additional properties for creating <seealso cref="BranchProperties">Branch Properties</seealso> in our DOM.
        /// </summary>
        IEnumerable<BranchProperties> BranchProperties { get; set; }
    }
}