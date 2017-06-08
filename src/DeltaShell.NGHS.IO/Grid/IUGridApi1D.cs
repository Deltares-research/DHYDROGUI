using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridApi1D : IGridApi, IDisposable
    {
        int Create1DNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int networkId);
        bool NetworkReady { get; }
        string GetNetworkName();
        int GetNumberOfNetworkNodes(out int numberOfNetworkNodes);
        int GetNumberOfNetworkBranches(out int numberOfNetworkBranches);
        int GetNumberOfNetworkGeometryPoints(out int numberOfNetworkGeometryPoints);
        int Write1DNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames);
        int Write1DNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames);
        int Write1DNetworkGeometry(double[] geopointsX, double[] geopointsY);
        int Read1DNetworkNodes(out double[] nodesX, out double[] nodesY, out string[] nodesIs, out string[] nodesLongnames);
        int Read1DNetworkBranches(out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames);
        int Read1DNetworkGeometry(out double[] geopointsX, out double[] geopointsY);
    }
}