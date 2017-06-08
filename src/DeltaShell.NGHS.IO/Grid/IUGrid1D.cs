namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGrid1D : IGrid
    {
        void Create1DGridInFile(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int networkId);

        void Write1DNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames);
        void Write1DNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames);
        void Write1DNetworkGeometry(double[] geopointsX, double[] geopointsY);

        int GetNumberOfNetworkNodes();
        int GetNumberOfNetworkBranches();
        int GetNumberOfNetworkGeometryPoints();

      
        void Read1DNetworkGeometry(out double[] geopointsX, out double[] geopointsY);
        void Read1DNetworkBranches(out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames);
        void Read1DNetworkNodes(out double[] nodesX, out double[] nodesY, out string[] nodesIds, out string[] nodesLongnames);
    }
}