using System;
using System.Linq;
using DeltaShell.NGHS.IO.Grid;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using SharpMap.Api.GridGeom;

namespace DeltaShell.NGHS.IO.Adapters
{
    public class UGridToUnstructuredGridAdapter : IDisposable
    {
        public UGridToUnstructuredGridAdapter(string filename)
        {
            uGrid = new UGrid(filename);
        }

        public UGrid uGrid { get; set; }

        /// <summary>
        /// Gets the unstructured grid associated UGrid mesh identifier.
        /// </summary>
        /// <param name="meshId">The mesh identifier.</param>
        /// <param name="oneBased">
        /// Whether the mesh associated with the meshID is one-based (fortran),
        /// or zero based (C-based).
        /// <paramref name="oneBased"/> defaults to false.
        /// </param>
        /// <param name="callCreateCells">
        /// Whether to call CreateCells for the retrieved grid.
        /// <paramref name="callCreateCells"/> defaults to false.
        /// </param>
        /// <returns>
        /// The grid associated with the <paramref name="meshId"/>
        /// of this <see cref="UGridToUnstructuredGridAdapter"/>
        /// </returns>
        /// <remarks>
        /// CreateCells will recalculate the cell centers using the kernel.
        /// This will ensure the correct cell centers will be used for spatial
        /// operations. This should be called for input grids that are used for
        /// spatial operations. This SHOULD NOT be called for output grids.
        /// CreateCells will reshuffle the indices. When this is called for output
        /// grids, the data associated with cells will be incorrect, if the indices
        /// are reshuffled.
        /// </remarks>
        public UnstructuredGrid GetUnstructuredGridFromUGridMeshId(int meshId,
                                                                   bool oneBased = false,
                                                                   bool callCreateCells = false)
        {
            if (meshId > uGrid.GetNumberOf2DMeshes() || meshId <= 0)
            {
                return null;
            }

            uGrid.GetAllNodeCoordinatesForMeshId(meshId);
            uGrid.GetEdgeNodesForMeshId(meshId);
            uGrid.GetFaceNodesForMeshId(meshId);

            UnstructuredGrid grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(
                uGrid.NodeCoordinatesByMeshId[meshId - 1].ToList(),
                uGrid.EdgeNodesByMeshId[meshId - 1],
                uGrid.FaceNodesByMeshId[meshId - 1],
                oneBased: oneBased);

            if (callCreateCells)
            {
                CreateCells(grid);
            }

            if (grid != null)
            {
                grid.CoordinateSystem = uGrid.CoordinateSystem;
            }

            return grid;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                uGrid?.Dispose();
            }
        }

        private static void CreateCells(UnstructuredGrid grid)
        {
            using (var api = new RemoteGridGeomApi())
            using (var mesh = new DisposableMeshGeometry(grid))
            {
                DisposableMeshGeometry resultMesh = api.CreateCells(mesh);
                grid.Cells = resultMesh.CreateCells();
            }
        }
    }
}