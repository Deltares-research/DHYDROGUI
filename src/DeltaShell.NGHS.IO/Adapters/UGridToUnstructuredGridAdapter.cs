using System;
using System.Linq;
using DeltaShell.NGHS.IO.Grid;
using NetTopologySuite.Extensions.Grids;

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

            if (grid != null)
            {
                grid.CoordinateSystem = uGrid.CoordinateSystem;
            }

            return grid;
        }

        public void Dispose()
        {
            if (uGrid != null)
            {
                uGrid.Dispose();
            }
        }
    }
}