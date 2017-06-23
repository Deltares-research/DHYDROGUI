using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGrid :IGrid
    {
        int NumberOf2DMeshes();
        int NumberOfNodes(int mesh);
        int NumberOfEdges(int mesh);
        int NumberOfFaces(int mesh);
        int NumberOfMaxFaceNodes(int mesh);
        bool GetAllNodeCoordinates(int mesh);
        void GetFaceNodesForMesh(int mesh);
        void GetEdgeNodesForMesh(int mesh);
        int[][,] FaceNodes { get; }
        int[][,] EdgeNodes { get; }
        Dictionary<int,Coordinate[]> NodeCoordinates{ get; }
        double zCoordinateFillValue { get; set; }
    }
}