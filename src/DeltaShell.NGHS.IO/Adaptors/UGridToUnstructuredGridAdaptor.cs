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
            if (mesh > uGrid.NumberOfMesh() || mesh <=0 ) return null;

            // TODO: We would like to set oneBased to false here, we can't do this currently since the Faces are always returned as 1 based by the GridApi (DELFT3DFM-1308)

            var grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(uGrid.NodeCoordinates[mesh-1].ToList(), uGrid.EdgeNodes[mesh-1], uGrid.FaceNodes[mesh-1]);
            if (grid != null) grid.CoordinateSystem = uGrid.CoordinateSystem;
            return grid;
        }

        public void Dispose()
        {
            if (uGrid != null) uGrid.Dispose();
        }
    }
}
