using DelftTools.Utils.Interop;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.rtc_kernel
{
    public static class RealTimeControlModelDll
    {
        public const string RTCTOOLS_DLL_NAME = "FBCTools_BMI.dll";

        static RealTimeControlModelDll()
        {
            DimrApiDataSet.SetSharedPath();
            NativeLibrary.LoadNativeDll(RTCTOOLS_DLL_NAME, DimrApiDataSet.RtcToolsDllPath);
        }

    }
}
