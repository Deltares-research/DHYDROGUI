namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridNetworkDiscretisation : IGrid
    {
        void CreateNetworkDiscretisationInFile(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId);
        void WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset);
        int GetNetworkId(int meshId);
        int GetNumberOfNetworkDiscretisationPoints(int meshId);
        void ReadNetworkDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset);

    }
}