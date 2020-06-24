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

        public UnstructuredGrid GetUnstructuredGridFromUGridMeshId(int meshId, bool oneBased = false)
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

            CreateCells(grid);

            if (grid != null)
            {
                grid.CoordinateSystem = uGrid.CoordinateSystem;
            }

            return grid;
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

        public void Dispose()
        {
            uGrid?.Dispose();
        }
    }
}