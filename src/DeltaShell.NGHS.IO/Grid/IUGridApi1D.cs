using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridApi1D : IGridApi, IDisposable
    {
        int Create1DNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints);
        bool NetworkReady { get; }

        int Write1DNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames);
        int Write1DNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames);
        int Write1DNetworkGeometry(double[] geopointsX, double[] geopointsY);

        int GetNumberOfNetworkNodes();
        int GetNumberOfNetworkBranches();
        int GetNumberOfNetworkGeometryPoints();
    }
}