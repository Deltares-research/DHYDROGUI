using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Utils.Interop;
using DeltaShell.NGHS.IO.Grid;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Api
{
    public class Iterative1d2dApi
    {
        public const string ITERATIVE1D2DDLL_NAME = "flow1d2d.dll";

        private static string DllDirectory
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(typeof(Iterative1d2dApi).Assembly.Location), "kernels");
            }
        }

        public static string DllPath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", ITERATIVE1D2DDLL_NAME); }
        }
        static Iterative1d2dApi()
        {
            NativeLibrary.LoadNativeDllForCurrentPlatform(ITERATIVE1D2DDLL_NAME, DllDirectory);
        }
    }
}
