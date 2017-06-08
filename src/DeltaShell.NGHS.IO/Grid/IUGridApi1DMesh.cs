namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridApi1DMesh : IGridApi
    {
        int Create1DMesh(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId);
        int Write1DMeshDiscretisationPoints(int[] branchIdx, double[] offset);
        int GetNumberOf1DMeshDiscretisationPoints();
        int Read1DMeshDiscretisationPoints(out int[] branchIdx, out double[] offset);
    }
}