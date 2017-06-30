using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGrid :IGrid
    {
        int NumberOf2DMeshes();
        int NumberOfNodes(int meshId);
        int NumberOfEdges(int meshId);
        int NumberOfFaces(int meshId);
        int NumberOfMaxFaceNodes(int meshId);
        Coordinate[] GetAllNodeCoordinatesForMesh(int meshId);
        int[,] GetFaceNodesForMesh(int meshId);
        int[,] GetEdgeNodesForMesh(int meshId);
        int[][,] FaceNodesPerMesh { get; }
        int[][,] EdgeNodesPerMesh { get; }
        Dictionary<int,Coordinate[]> NodeCoordinates{ get; }
        double zCoordinateFillValue { get; set; }
    }
}