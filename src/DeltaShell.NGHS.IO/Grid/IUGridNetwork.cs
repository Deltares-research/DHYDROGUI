namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridNetwork : IGrid
    {
        void CreateNetworkInFile(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int networkId);

        void WriteNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames);
        void WriteNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames, int[] branchOrderNumbers);
        void WriteNetworkGeometry(double[] geopointsX, double[] geopointsY);

        int GetNumberOfNetworkNodes(int networkId);
        int GetNumberOfNetworkBranches(int networkId);
        int GetNumberOfNetworkGeometryPoints(int networkId);


        void ReadNetworkNodes(int networkId, out double[] nodesX, out double[] nodesY, out string[] nodesIds, out string[] nodesLongnames);
        void ReadNetworkBranches(int networkId, out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames, out int[] branchOrderNumbers);
        void ReadNetworkGeometry(int networkId, out double[] geopointsX, out double[] geopointsY);
    }
}