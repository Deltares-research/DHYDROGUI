using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridNetworkApi : IGridApi, IDisposable
    {
        int CreateNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int networkId);
        bool NetworkReadyForWriting { get; }
        int GetNetworkName(int networkId, out string networkName);
        int GetNumberOfNetworkNodes(int networkId, out int numberOfNetworkNodes);
        int GetNumberOfNetworkBranches(int networkId, out int numberOfNetworkBranches);
        int GetNumberOfNetworkGeometryPoints(int networkId, out int numberOfNetworkGeometryPoints);
        int WriteNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames);
        int WriteNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames);
        int WriteNetworkGeometry(double[] geopointsX, double[] geopointsY);
        int ReadNetworkNodes(int networkId, out double[] nodesX, out double[] nodesY, out string[] nodesIs, out string[] nodesLongnames);
        int ReadNetworkBranches(int networkId, out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames);
        int ReadNetworkGeometry(int networkId, out double[] geopointsX, out double[] geopointsY);
    }
}