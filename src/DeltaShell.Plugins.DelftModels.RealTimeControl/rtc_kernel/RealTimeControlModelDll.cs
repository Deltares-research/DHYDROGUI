using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.rtc_kernel
{
    public static class RealTimeControlModelDll
    {
        public const string RTCTOOLS_FOLDER_NAME = "drtc";
        public const string RTCTOOLS_BINFOLDER_NAME = "bin";
        public const string RTCTOOLS_DLL_NAME = "FBCTools_BMI.dll";

        public static string DllPath
        {
            get { return Path.Combine(DimrApiDataSet.DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", RTCTOOLS_FOLDER_NAME, RTCTOOLS_BINFOLDER_NAME); }
        }

        static RealTimeControlModelDll()
        {
            DimrApiDataSet.SetSharedPath();
            NativeLibrary.LoadNativeDll(RTCTOOLS_DLL_NAME, DllPath);
        }

    }
}
