namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGrid1DMesh : IGrid
    {
        void Create1DMeshInFile(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId);
        void Write1DMeshDiscretizationPoints(int[] branchIdx, double[] offset);
        int GetNumberOf1DMeshDiscretisationPoints();
        int Read1DMeshDiscretisationPoints(out int[] branchIdx, out double[] offset);

    }
}