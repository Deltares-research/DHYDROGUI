using System;
using System.Linq;
using DeltaShell.NGHS.IO.Grid;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.NGHS.IO.Adaptors
{
    public class UGridToUnstructuredGridAdaptor : IDisposable
    {
        public IUGrid uGrid { get; set; }

        public UGridToUnstructuredGridAdaptor(string filename)
        {
           uGrid = new UGrid(filename);
        }
        
        public UnstructuredGrid GetUnstructuredGridFromUGridMeshId(int meshId)
        {
            if (meshId > uGrid.GetNumberOf2DMeshes() || meshId <=0 ) return null;

            uGrid.GetAllNodeCoordinatesForMeshId(meshId);
            uGrid.GetEdgeNodesForMeshId(meshId);
            uGrid.GetFaceNodesForMeshId(meshId);

            var grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(uGrid.NodeCoordinatesByMeshId[meshId-1].ToList(), uGrid.EdgeNodesByMeshId[meshId-1], uGrid.FaceNodesByMeshId[meshId-1]);
            if (grid != null) grid.CoordinateSystem = uGrid.CoordinateSystem;
            return grid;
        }

        public void Dispose()
        {
            if (uGrid != null) uGrid.Dispose();
        }
    }
}
