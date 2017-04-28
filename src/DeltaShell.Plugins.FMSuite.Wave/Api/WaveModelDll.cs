using System;
using System.IO;
using System.Runtime.InteropServices;
using DelftTools.Utils.Interop;

namespace DeltaShell.Plugins.FMSuite.Wave.Api
{
    public static class WaveModelDll
    {
        static WaveModelDll()
        {
            var dir = Path.GetDirectoryName(typeof (WaveModelDll).Assembly.Location);

            if (dir != null)
            {
                var dllDir = Path.Combine(dir, "Delft3D", Arch, "wave", "bin");
                using (new WaveModelApi.WaveDllHelper(string.Empty))
                {
                    NativeLibrary.LoadNativeDll("wave.dll", dllDir);
                }
            }
        }

        public static string Arch
        {
            get { return "win64"; }
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