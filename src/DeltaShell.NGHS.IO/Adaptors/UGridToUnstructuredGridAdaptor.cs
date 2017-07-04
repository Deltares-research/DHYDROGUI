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
        
        public UnstructuredGrid GetUnstructuredGridFromUGridMeshId(int mesh)
        {
            if (mesh > uGrid.GetNumberOf2DMeshes() || mesh <=0 ) return null;

            var grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(uGrid.NodeCoordinatesByMeshId[mesh-1].ToList(), uGrid.EdgeNodesByMeshId[mesh-1], uGrid.FaceNodesByMeshId[mesh-1]);
            if (grid != null) grid.CoordinateSystem = uGrid.CoordinateSystem;
            return grid;
        }

        public void Dispose()
        {
            if (uGrid != null) uGrid.Dispose();
        }
    }
}
