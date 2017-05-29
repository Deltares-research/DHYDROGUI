namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGrid1D : IGrid
    {
        void Create1DGridInFile(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints);

        void Write1DNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames);
        void Write1DNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames);
        void Write1DNetworkGeometry(double[] geopointsX, double[] geopointsY);

        int GetNumberOfNetworkNodes();
        int GetNumberOfNetworkBranches();
        int GetNumberOfNetworkGeometryPoints();

        void Create1DMeshInFile(string name, int numberOfMeshPoints, int numberOfMeshEdges);
        void Write1DMeshDiscretizationPoints(int[] branchIdx, double[] offset);
        int GetNumberOfMeshDiscretisationPoints();
        int Read1DNetworkGeometry(out double[] geopointsX, out double[] geopointsY);
        int Read1DNetworkNodes(out double[] nodesX, out double[] nodesY, out string[] nodesIds, out string[] nodesLongnames);
        int Read1DNetworkBranches(out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames);
    }
}