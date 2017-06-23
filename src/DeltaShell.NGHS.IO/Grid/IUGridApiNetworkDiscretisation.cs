namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridApiNetworkDiscretisation : IGridApi
    {
        int CreateNetworkDiscretisation(string name, int numberOfNetworkPoints, int numberOfMeshEdges, int networkId);
        int WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset);
        int GetNetworkDiscretisationName(int meshId, out string meshName);
        int GetNumberOfNetworkDiscretisationPoints(int meshId);
        int ReadNetworkDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset);
    }
}