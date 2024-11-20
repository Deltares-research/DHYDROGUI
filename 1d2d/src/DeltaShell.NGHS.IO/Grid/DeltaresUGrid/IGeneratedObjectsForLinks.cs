using Deltares.UGrid.Api;

namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    /// <summary>
    /// All the objects needed to generate 1d2d links
    /// </summary>
    public interface IGeneratedObjectsForLinks : IConvertedUgridFileObjects
    {
        /// <summary>
        /// 1D mesh geometry (computation points) based on a 1D network
        /// </summary>
        Disposable1DMeshGeometry Mesh1d { get; set; }

        /// <summary>
        /// 2D mesh (vertices, edges and cells)
        /// </summary>
        Disposable2DMeshGeometry Mesh2d { get; set; }

        /// <summary>
        /// 1D network (nodes, branches etc.)
        /// </summary>
        DisposableNetworkGeometry NetworkGeometry { get; set; }

        /// <summary>
        /// Links between a 1D mesh (see: <see cref="T:Deltares.UGrid.Api.Disposable1DMeshGeometry" />) and a
        /// 2D mesh (see: <see cref="T:Deltares.UGrid.Api.Disposable2DMeshGeometry" />)
        /// </summary>
        DisposableLinksGeometry LinksGeometry { get; set; }

        /// <summary>
        /// Fill value of Mesh2d nodes 
        /// </summary>
        /// <remarks>-999 is default of Deltares / int.MinValue is default to some other partners</remarks>
        int FillValueMesh2DFaceNodes { get; set; }
    }
}