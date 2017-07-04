using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGrid :IGrid
    {
        int GetNumberOf2DMeshes();
        int GetNumberOfNodesForMeshId(int meshId);
        int GetNumberOfEdgesForMeshId(int meshId);
        int GetNumberOfFacesForMeshId(int meshId);
        int GetNumberOfMaxFaceNodesForMeshId(int meshId);
        Coordinate[] GetAllNodeCoordinatesForMeshId(int meshId);
        int[,] GetFaceNodesForMeshId(int meshId);
        int[,] GetEdgeNodesForMeshId(int meshId);
        int[][,] FaceNodesByMeshId { get; }
        int[][,] EdgeNodesByMeshId { get; }
        Dictionary<int,Coordinate[]> NodeCoordinatesByMeshId{ get; }
        double ZCoordinateFillValue { get; set; }
    }
}