using System;
using System.Linq;
using DeltaShell.NGHS.IO.Grid;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.NGHS.IO.Adaptors
{
    public class UGridToUnstructuredGridAdaptor : IDisposable
    {
        public UGrid uGrid { get; set; }

        public UGridToUnstructuredGridAdaptor(string filename)
        {
           uGrid = new UGrid(filename);
        }
        
        public UnstructuredGrid GetUnstructuredGridFromUGridMeshId(int meshId)
        {
            if(uGrid.GetNumberOf2DMeshes() > 1 || meshId <= 0 ) return null;

            uGrid.GetAllNodeCoordinatesForMeshId(meshId);
            uGrid.GetEdgeNodesForMeshId(meshId);
            uGrid.GetFaceNodesForMeshId(meshId);

            var grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(
                uGrid.NodeCoordinatesByMeshId[meshId].ToList(), 
                uGrid.EdgeNodesByMeshId[meshId], 
                uGrid.FaceNodesByMeshId[meshId], 
                oneBased: false);

            if (grid != null) grid.CoordinateSystem = uGrid.CoordinateSystem;
            return grid;
        }

        public void Dispose()
        {
            if (uGrid != null) uGrid.Dispose();
        }

        public int? GetMesh2DId()
        {
            return uGrid.GetMesh2DIds().FirstOrDefault();
        }
    }
}
