using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Utils.Interop;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.rtc_kernel
{
    public static class RealTimeControlModelDll
    {
        private static string _dllPath;
        public const string RTCTOOLSDLL_NAME = "RTCTools_BMI.dll";

        public static string DllDirectory
        {
            get { return Path.Combine(Path.GetDirectoryName(typeof(RealTimeControlModelDll).Assembly.Location), "rtc_kernel"); }
        }

        public static string DllPath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", RTCTOOLSDLL_NAME); }
        }

        static RealTimeControlModelDll()
        {
            NativeLibrary.LoadNativeDllForCurrentPlatform(RTCTOOLSDLL_NAME, DllDirectory);
        }

    }
}
