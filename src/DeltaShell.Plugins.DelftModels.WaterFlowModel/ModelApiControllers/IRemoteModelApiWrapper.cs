using DelftTools.Utils.Interop;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using ProtoBufRemote;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers
{
    /// <summary>
    /// Defines an aggregated version of several IModelApi calls to make the remoting-interface less 'chatty' and thus more performant
    /// </summary>
    [DoNotWaitForVoid]
    public interface IRemoteModelApiWrapper : IModelApi
    {
        void NetworkSetBoundaryValues(int[] irefs, double time, double[] values);
        void SetStrucControlValues(int[] irefs, int[] types, double[] values);

        double[] GetAllValues(int[] compositeKeys);
    }

    /// <summary>
    /// (this part runs in a worker process)
    /// </summary>
    public class RemoteModelApiWrapper : ModelApi.ModelApi, IRemoteModelApiWrapper
    {
        static RemoteModelApiWrapper()
        {
            DimrApiDataSet.SetSharedPath();
            NativeLibrary.LoadNativeDll(Flow1DApiDll.CF_DLL_NAME, DimrApiDataSet.CfDllPath);
        }

        public void NetworkSetBoundaryValues(int[] irefs, double time, double[] values)
        {
            for (var i = 0; i < irefs.Length; i++)
                NetworkSetBoundaryValue(irefs[i], time, values[i]);
        }

        public void SetStrucControlValues(int[] irefs, int[] types, double[] values)
        {
            for (var i = 0; i < irefs.Length; i++)
                SetStrucControlValue(irefs[i], (QuantityType) types[i], values[i]);
        }

        public double[] GetAllValues(int[] compositeKeys)
        {
            var values = new double[compositeKeys.Length];
            for (int i = 0; i < compositeKeys.Length; i++)
            {
                // do the inverse operations to retrieve the key back
                var location = compositeKeys[i] & 0xFFFF;
                var elementSet = (ElementSet) ((compositeKeys[i] >> 16) & 0xFF);
                var quantity = (QuantityType) ((compositeKeys[i] >> 24) & 0xFF);
                values[i] = GetValue(quantity, elementSet, location);
            }
            return values;
        }
    }
}