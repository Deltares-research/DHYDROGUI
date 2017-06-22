namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridApi1DDiscretisation : IGridApi
    {
        int Create1dDiscretisation(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId);
        int Write1dDiscretisationPoints(int[] branchIdx, double[] offset);
        int GetMeshDiscretisationName(int meshId, out string meshName);
        int GetNumberOf1dDiscretisationPoints(int meshId);
        int Read1dDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset);
    }
}