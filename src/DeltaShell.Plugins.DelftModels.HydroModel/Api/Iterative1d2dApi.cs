using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.Grid;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Api
{
    public class Iterative1d2dApi
    {
        public const string ITERATIVE1D2D_FOLDER_NAME = "dflow1d2d";
        public const string ITERATIVE1D2D_BINFOLDER_NAME = "bin";
        public const string ITERATIVE1D2D_DLL_NAME = "flow1d2d.dll";
        
        public static string DllPath
        {
            get { return Path.Combine(DimrApiDataSet.DllDirectory, "x64", ITERATIVE1D2D_FOLDER_NAME, ITERATIVE1D2D_BINFOLDER_NAME); }
        }

        static Iterative1d2dApi()
        {
            DimrApiDataSet.SetSharedPath();
            NativeLibrary.LoadNativeDll(ITERATIVE1D2D_DLL_NAME, DllPath);
        }
    }
}
