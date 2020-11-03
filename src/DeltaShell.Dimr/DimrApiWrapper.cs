using System;
using System.Runtime.InteropServices;
using BasicModelInterface;

namespace DeltaShell.Dimr
{
    public static class DimrApiWrapper
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void Message_Callback([MarshalAs(UnmanagedType.LPStr)]string time, [MarshalAs(UnmanagedType.LPStr)]string message, [MarshalAs(UnmanagedType.U4)]uint level);

        [DllImport(DimrApiDataSet.DIMR_DLL_NAME, EntryPoint = "set_logger_callback", CallingConvention = CallingConvention.Cdecl)]
        public static extern int set_logger_callback([MarshalAs(UnmanagedType.FunctionPtr)] Message_Callback c_message_callback);

        [DllImport(DimrApiDataSet.DIMR_DLL_NAME, EntryPoint = "initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int initialize(string configFile);

        [DllImport(DimrApiDataSet.DIMR_DLL_NAME, EntryPoint = "update", CallingConvention = CallingConvention.Cdecl)]
        public static extern int update(double step);

        [DllImport(DimrApiDataSet.DIMR_DLL_NAME, EntryPoint = "finalize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int finalize();

        [DllImport(DimrApiDataSet.DIMR_DLL_NAME, EntryPoint = "get_start_time", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_start_time(ref double start_time);

        [DllImport(DimrApiDataSet.DIMR_DLL_NAME, EntryPoint = "get_end_time", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_end_time(ref double stop_time);

        [DllImport(DimrApiDataSet.DIMR_DLL_NAME, EntryPoint = "get_time_step", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_time_step(ref double time_step);

        [DllImport(DimrApiDataSet.DIMR_DLL_NAME, EntryPoint = "get_current_time", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_current_time(ref double current_time);

        [DllImport(DimrApiDataSet.DIMR_DLL_NAME, EntryPoint = "set_var", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_var(string varName, IntPtr value);

        [DllImport(DimrApiDataSet.DIMR_DLL_NAME, EntryPoint = "get_var", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_var(string varName, ref IntPtr value);

        [DllImport(DimrApiDataSet.DIMR_DLL_NAME, EntryPoint = "set_logger", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_logger(Logger logger);

    }
}
