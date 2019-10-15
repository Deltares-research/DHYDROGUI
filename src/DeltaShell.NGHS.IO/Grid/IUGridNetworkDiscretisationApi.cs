namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridNetworkDiscretisationApi : IGridApi
    {
        int CreateNetworkDiscretisation(int numberOfMeshPoints, int numberOfMeshEdges);
        int WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset, double[] discretisationPointsX,
            double[] discretisationPointsY, int[] edgeIdx, double[] edgeOffset, double[] edgePointsX,
            double[] edgePointsY, int[] edgeNodes, string[] ids, string[] names);
        int GetNetworkIdFromMeshId(int meshId, out int networkId);
        int GetNetworkDiscretisationName(int meshId, out string meshName);
        int GetNumberOfNetworkDiscretisationPoints(int meshId, out int numberOfDiscretisationPoints);
        int ReadNetworkDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset, out double[] discretisationPointsX, out double[] discretisationPointsY, out string[] ids, out string[] names);
    }
}