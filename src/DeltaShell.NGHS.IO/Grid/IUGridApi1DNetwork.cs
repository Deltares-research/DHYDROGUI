using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridApi1DNetwork : IGridApi, IDisposable
    {
        int Create1DNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int networkId);
        bool NetworkReadyForWriting { get; }
        int GetNetworkName(int networkId, out string networkName);
        int GetNumberOfNetworkNodes(int networkId, out int numberOfNetworkNodes);
        int GetNumberOfNetworkBranches(int networkId, out int numberOfNetworkBranches);
        int GetNumberOfNetworkGeometryPoints(int networkId, out int numberOfNetworkGeometryPoints);
        int Write1DNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames);
        int Write1DNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames);
        int Write1DNetworkGeometry(double[] geopointsX, double[] geopointsY);
        int Read1DNetworkNodes(int networkId, out double[] nodesX, out double[] nodesY, out string[] nodesIs, out string[] nodesLongnames);
        int Read1DNetworkBranches(int networkId, out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames);
        int Read1DNetworkGeometry(int networkId, out double[] geopointsX, out double[] geopointsY);
    }
}