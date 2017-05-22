using System;
using System.IO;
using System.Runtime.InteropServices;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.FMSuite.Wave.Api
{
    public static class WaveModelDll
    {
        private const string WAVE_FOLDER_NAME = "dwaves";
        private const string WAVE_BINFOLDER_NAME = "bin";
        private const string WAVE_DLL_NAME = "wave.dll";

        public static string DllPath
        {
            get { return Path.Combine(DimrApiDataSet.DllDirectory, Arch, WAVE_FOLDER_NAME, WAVE_BINFOLDER_NAME); }
        }

        static WaveModelDll()
        {
            using (new WaveModelApi.WaveDllHelper(string.Empty))
            {
                DimrApiDataSet.SetSharedPath();
                NativeLibrary.LoadNativeDll(WAVE_DLL_NAME, DllPath);
            }
            
        }

        public static string Arch
        {
            get { return "x64"; } // wave is only 64b
        }

        [DllImport("wave", EntryPoint = "initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int initialize([In] string configFile);

        [DllImport("wave", EntryPoint = "update", CallingConvention = CallingConvention.Cdecl)]
        public static extern void update([In] double dt);

        [DllImport("wave", EntryPoint = "finalize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void finalize();

        [DllImport("wave", EntryPoint = "set_var", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_var([In] string key, [In] string value);

        [DllImport("wave", EntryPoint = "get_current_time", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_current_time([In, Out] ref double t);

        [DllImport("wave", EntryPoint = "get_start_time", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_start_time([In, Out] ref double t);
    }
}