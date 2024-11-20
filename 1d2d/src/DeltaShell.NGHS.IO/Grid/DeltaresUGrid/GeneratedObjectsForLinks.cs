using Deltares.UGrid.Api;

namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    /// <summary>
    /// Will be used to collect all objects needed to generate our 1D2D links from a (ugrid) file
    /// and the possibility to read the links too.
    /// </summary>
    public class GeneratedObjectsForLinks : ConvertedUgridFileObjects, IGeneratedObjectsForLinks
    {
        /// <inheritdoc/>
        public Disposable1DMeshGeometry Mesh1d { get; set; }
        
        /// <inheritdoc/>
        public Disposable2DMeshGeometry Mesh2d { get; set; }
        
        /// <inheritdoc/>
        public DisposableNetworkGeometry NetworkGeometry { get; set; }
        
        /// <inheritdoc/>
        public DisposableLinksGeometry LinksGeometry { get; set; }
        
        /// <inheritdoc/>
        public int FillValueMesh2DFaceNodes { get; set; } = (int)UGridFile.DEFAULT_NO_DATA_VALUE;
    }
}