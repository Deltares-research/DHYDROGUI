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
            if (mesh > uGrid.NumberOf2DMeshes() || mesh <=0 ) return null;

            var grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(uGrid.NodeCoordinates[mesh-1].ToList(), uGrid.EdgeNodesPerMesh[mesh-1], uGrid.FaceNodesPerMesh[mesh-1]);
            if (grid != null) grid.CoordinateSystem = uGrid.CoordinateSystem;
            return grid;
        }

        public void Dispose()
        {
            if (uGrid != null) uGrid.Dispose();
        }
    }
}
