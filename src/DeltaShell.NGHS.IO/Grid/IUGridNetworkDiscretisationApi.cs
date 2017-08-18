namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridNetworkDiscretisationApi : IGridApi
    {
        int CreateNetworkDiscretisation(string name, int numberOfNetworkPoints, int numberOfMeshEdges, int networkId);
        int WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset, string[] ids, string[] names);
        int GetNetworkIdFromMeshId(int meshId, out int networkId);
        int GetNetworkDiscretisationName(int meshId, out string meshName);
        int GetNumberOfNetworkDiscretisationPoints(int meshId, out int numberOfDiscretisationPoints);
        int ReadNetworkDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset, out string[] ids, out string[] names);
    }
}