using ProtoBufRemote;

namespace RainfallRunoffModelEngine
{
    [DoNotWaitForVoid]
    public interface IRRRemoteModelApi : IRRModelApi
    {
        void SetAllValuesFormat(int[] quantityIds, int[] elementSetIds, int[] sizes);
        double[] Execute(double[] valuesIn, ref bool modelRan);
    }
}