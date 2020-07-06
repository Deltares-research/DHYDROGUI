using System;
using DelftTools.Hydro;

namespace DeltaShell.NGHS.IO.Grid.GridGeomApi
{
    public interface IGridGeomApi : IDisposable
    {
        /// <summary>
        /// Generate link information for the provided 1d and 2d mesh using the specified link type
        /// </summary>
        /// <param name="mesh2D">The 2d mesh</param>
        /// <param name="mesh1D">The 1d mesh</param>
        /// <param name="selectedArea">Selected area to generate links for</param>
        /// <param name="filter1DMesh">Mask array for 1d mesh</param>
        /// <param name="linkType">Type of links to generate</param>
        /// <param name="geometryGullies">Geometry of the gullies</param>
        /// <returns>Link information of the 1d/2d links</returns>
        LinkInformation GetLinkInformation(DisposableMeshGeometryGridGeom mesh2D, Mesh1DGeometry mesh1D, GeometriesData selectedArea, bool[] filter1DMesh, LinkGeneratingType linkType, GeometriesData geometryGullies = null);
    }
}