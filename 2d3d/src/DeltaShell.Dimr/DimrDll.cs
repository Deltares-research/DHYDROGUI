using System;
using System.Runtime.InteropServices;
using BasicModelInterface;

namespace DeltaShell.Dimr
{
    /// <summary>
    /// <see cref="DimrDll"/> defines the external API calls to the dimr dll.
    /// </summary>
    internal static class DimrDll
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate void Message_Callback([MarshalAs(UnmanagedType.LPStr)] string time, [MarshalAs(UnmanagedType.LPStr)] string message, [MarshalAs(UnmanagedType.U4)] uint level);

        [DllImport(DimrApiDataSet.DimrDllName, EntryPoint = "set_logger_callback", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int set_logger_callback([MarshalAs(UnmanagedType.FunctionPtr)] Message_Callback c_message_callback);

        [DllImport(DimrApiDataSet.DimrDllName, EntryPoint = "initialize", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int initialize(string configFile);

        [DllImport(DimrApiDataSet.DimrDllName, EntryPoint = "update", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int update(double step);

        [DllImport(DimrApiDataSet.DimrDllName, EntryPoint = "finalize", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void finalize();

        [DllImport(DimrApiDataSet.DimrDllName, EntryPoint = "get_start_time", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void get_start_time(ref double start_time);

        [DllImport(DimrApiDataSet.DimrDllName, EntryPoint = "get_end_time", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void get_end_time(ref double stop_time);

        [DllImport(DimrApiDataSet.DimrDllName, EntryPoint = "get_time_step", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void get_time_step(ref double time_step);

        [DllImport(DimrApiDataSet.DimrDllName, EntryPoint = "get_current_time", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void get_current_time(ref double current_time);

        [DllImport(DimrApiDataSet.DimrDllName, EntryPoint = "set_var", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_var(string varName, IntPtr value);

        [DllImport(DimrApiDataSet.DimrDllName, EntryPoint = "get_var", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void get_var(string varName, ref IntPtr value);

        [DllImport(DimrApiDataSet.DimrDllName, EntryPoint = "set_logger", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void set_logger(Logger logger);
    }
}