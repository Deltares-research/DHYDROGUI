using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridApi1D : IGridApi, IDisposable
    {
        int Create1DNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints);
        bool NetworkReady { get; }
        bool IsInitialized();
        bool IsNetworkReady();


        int Write1DNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames);
        int Write1DNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames);
        int Write1DNetworkGeometry(double[] geopointsX, double[] geopointsY);

        int GetNumberOfNetworkNodes();
        int GetNumberOfNetworkBranches();
        int GetNumberOfNetworkGeometryPoints();

        int Create1DMesh(string name, int numberOfMeshPoints, int numberOfMeshEdges);
        int Write1DMeshDiscretisationPoints(int[] branchIdx, double[] offset);
        int GetNumberOfMeshDiscretisationPoints();

        int Read1DNetworkNodes(out double[] nodesX, out double[] nodesY, out string[] nodesIs, out string[] nodesLongnames);
        int Read1DNetworkBranches(out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames);
        int Read1DNetworkGeometry(out double[] geopointsX, out double[] geopointsY);
    }
}