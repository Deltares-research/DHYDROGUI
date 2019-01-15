namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridNetworkDiscretisationApi : IGridApi
    {
        int CreateNetworkDiscretisation(int numberOfNetworkPoints);
        int WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset, double[] discretisationPointsX, double[] discretisationPointsY, string[] ids, string[] names);
        int GetNetworkIdFromMeshId(int meshId, out int networkId);
        int GetNetworkDiscretisationName(int meshId, out string meshName);
        int GetNumberOfNetworkDiscretisationPoints(int meshId, out int numberOfDiscretisationPoints);
        int ReadNetworkDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset, out double[] discretisationPointsX, out double[] discretisationPointsY, out string[] ids, out string[] names);
    }
}