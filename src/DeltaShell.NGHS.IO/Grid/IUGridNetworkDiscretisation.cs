namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridNetworkDiscretisation : IGrid
    {
        void CreateNetworkDiscretisationInFile(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId);
        void WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset);
        int GetNetworkIdForMeshId(int meshId);
        int GetNumberOfNetworkDiscretisationPointsForMeshId(int meshId);
        void ReadNetworkDiscretisationPointsForMeshId(int meshId, out int[] branchIdx, out double[] offset);

    }
}