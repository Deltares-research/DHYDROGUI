using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi
{
    public static class Flow1DApiDll
    {
        private const string CF_FOLDER_NAME = "dflow1d";
        private const string CF_BINFOLDER_NAME = "bin";
        public const string CF_DLL_NAME = "cf_dll.dll";

        public static string DllPath
        {
            get { return Path.Combine(DimrApiDataSet.DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", CF_FOLDER_NAME, CF_BINFOLDER_NAME); }
        }

        static Flow1DApiDll()
        {
            DimrApiDataSet.SetSharedPath();
            NativeLibrary.LoadNativeDll(CF_DLL_NAME, DllPath);
        }

        [DllImport(CF_DLL_NAME, EntryPoint = "getMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getMessage([In] ref int b, [In, Out] StringBuilder message, [In] ref int a);

        [DllImport(CF_DLL_NAME, EntryPoint = "MessageCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern int MessageCnt();

        [DllImport(CF_DLL_NAME, EntryPoint = "SetStatisticalOutput", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetStatisticalOutput([In] ref int elementsetId, [In] ref int quantityID,
            [In] ref int operation, [In] ref double outputInterval);

        [DllImport(CF_DLL_NAME, EntryPoint = "GetStatisticalOutputSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetStatisticalOutputSize([In] ref int index);

        [DllImport(CF_DLL_NAME, EntryPoint = "GetStatisticalOutput", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetStatisticalOutput([In] ref int index, double[] values, [In] ref int count);

        [DllImport(CF_DLL_NAME, EntryPoint = "GetStatisticalOutputIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetStatisticalOutputIndex([In] ref int elementsetId, [In] ref int quantityId,
            [In] ref int operation, [In] ref double outputInterval);

        [DllImport(CF_DLL_NAME, EntryPoint = "ModelPerformTimeStep", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ModelPerformTimeStep();

        [DllImport(CF_DLL_NAME, EntryPoint = "ModelInitializeUserTimeStep", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ModelInitializeUserTimeStep();

        [DllImport(CF_DLL_NAME, EntryPoint = "ModelFinalizeUserTimeStep", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ModelFinalizeUserTimeStep();

        [DllImport(CF_DLL_NAME, EntryPoint = "ModelInitializeComputationalTimeStep", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ModelInitializeComputationalTimeStep_([In] ref double newTime, [In, Out] ref double dt);

        [DllImport(CF_DLL_NAME, EntryPoint = "ModelRunComputationalTimeStep", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ModelRunComputationalTimeStep([In, Out] ref double dt);

        [DllImport(CF_DLL_NAME, EntryPoint = "ModelFinalizeComputationalTimeStep", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ModelFinalizeComputationalTimeStep();

        [DllImport(CF_DLL_NAME, EntryPoint = "finalize", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Finalize_();

        [DllImport(CF_DLL_NAME, EntryPoint = "GetStrucControlValue", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetStrucControlValue([In] ref int istru, [In] ref int iparam);

        [DllImport(CF_DLL_NAME, EntryPoint = "SetStrucControlValue", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetStrucControlValue([In] ref int istru, [In] ref int iparam, [In] ref double numValue);

        [DllImport(CF_DLL_NAME, EntryPoint = "get_var", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_var([In] string variable, [In, Out] ref IntPtr ptr);

        [DllImport(CF_DLL_NAME, EntryPoint = "get_var_shape", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_var_shape([In] string variable, [Out] int[] shape);

        [DllImport(CF_DLL_NAME, EntryPoint = "get_var_rank", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_var_rank([In] string variable, [Out] out int rank);

        [DllImport(CF_DLL_NAME, EntryPoint = "get_var_type", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_var_type([In] string variable, [Out] StringBuilder value);

        [DllImport(CF_DLL_NAME, EntryPoint = "set_var", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_var([In] string variable, [In, Out] double[] values);

        [DllImport(CF_DLL_NAME, EntryPoint = "initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int initialize([In] string file);

    }
}
