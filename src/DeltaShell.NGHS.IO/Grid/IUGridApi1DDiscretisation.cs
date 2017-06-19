namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridApi1DDiscretisation : IGridApi
    {
        int Create1dDiscretisation(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId);
        int Write1dDiscretisationPoints(int[] branchIdx, double[] offset);
        int GetNumberOf1dDiscretisationPoints();
        int Read1dDiscretisationPoints(out int[] branchIdx, out double[] offset);
    }
}