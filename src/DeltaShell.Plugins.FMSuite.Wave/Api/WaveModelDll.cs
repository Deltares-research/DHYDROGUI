using System;
using System.Runtime.InteropServices;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.FMSuite.Wave.Api
{
    [Obsolete("Running through DIMR now.")]
    public static class WaveModelDll
    {
        private const string WAVE_DLL_NAME = "wave.dll";

        static WaveModelDll()
        {
            using (new WaveModelApi.WaveDllHelper(string.Empty))
            {
                DimrApiDataSet.SetSharedPath();
                NativeLibrary.LoadNativeDll(WAVE_DLL_NAME, DimrApiDataSet.WaveDllPath);
            }
        }

        [DllImport(WAVE_DLL_NAME, EntryPoint = "initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int initialize([In] string configFile);

        [DllImport(WAVE_DLL_NAME, EntryPoint = "update", CallingConvention = CallingConvention.Cdecl)]
        public static extern void update([In] double dt);

        [DllImport(WAVE_DLL_NAME, EntryPoint = "finalize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void finalize();

        [DllImport(WAVE_DLL_NAME, EntryPoint = "set_var", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_var([In] string key, [In] string value);

        [DllImport(WAVE_DLL_NAME, EntryPoint = "get_current_time", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_current_time([In] [Out] ref double t);

        [DllImport(WAVE_DLL_NAME, EntryPoint = "get_start_time", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_start_time([In] [Out] ref double t);
    }
}