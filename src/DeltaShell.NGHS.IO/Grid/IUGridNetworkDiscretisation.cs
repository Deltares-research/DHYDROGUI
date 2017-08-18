namespace DeltaShell.NGHS.IO.Grid
{
    public interface IUGridNetworkDiscretisation : IGrid
    {
        void CreateNetworkDiscretisationInFile(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId);
        void WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset, string[] ids, string[] names);
        string GetNetworkDiscretisationNameForMeshId(int meshId);
        int GetNetworkIdForMeshId(int meshId);
        int GetNumberOfNetworkDiscretisationPointsForMeshId(int meshId);
        int GetNumberOfNetworkDiscretisations();
        int[] GetNetworkDiscretisationIds(int numberOfMeshes);
        void ReadNetworkDiscretisationPointsForMeshId(int meshId, out int[] branchIdx, out double[] offset, out string[] ids, out string[] names);
    }
}