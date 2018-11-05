using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridMesh2DApi : IGridApi, IDisposable
    {
        int CreateMesh2D(GridWrapper.meshgeomdim dimensions, GridWrapper.meshgeom data);
        /*bool Mesh2DReadyForWriting { get; }
        int GetMesh2DName(int Mesh2DId, out string Mesh2DName);
        int GetNumberOfMesh2DNodes(int Mesh2DId, out int numberOfMesh2DNodes);
        int GetNumberOfMesh2DBranches(int Mesh2DId, out int numberOfMesh2DBranches);
        int GetNumberOfMesh2DGeometryPoints(int Mesh2DId, out int numberOfMesh2DGeometryPoints);
        int WriteMesh2DNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames);
        int WriteMesh2DBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames, int[] branchOrderNumbers);
        int WriteMesh2DGeometry(double[] geopointsX, double[] geopointsY);
        int ReadMesh2DNodes(int Mesh2DId, out double[] nodesX, out double[] nodesY, out string[] nodesIs, out string[] nodesLongnames);
        int ReadMesh2DBranches(int Mesh2DId, out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames, out int[] branchOrderNumbers);
        int ReadMesh2DGeometry(int Mesh2DId, out double[] geopointsX, out double[] geopointsY);*/
    }
}