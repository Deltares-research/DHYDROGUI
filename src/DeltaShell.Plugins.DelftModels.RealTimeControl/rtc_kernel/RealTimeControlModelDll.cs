using DelftTools.Utils.Interop;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.rtc_kernel
{
    public static class RealTimeControlModelDll
    {
        static RealTimeControlModelDll()
        {
            DimrApiDataSet.AddKernelDirToPath();
            NativeLibrary.LoadNativeDll(DimrApiDataSet.RtcToolsDllName, DimrApiDataSet.RtcToolsDllDirectory);
        }
    }
}