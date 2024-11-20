using System;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using System.Text;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    [GeneratedCode("to silence fxcop","1.0")]
    public static class FlexibleMeshModelDll
    {
        //repos/ds/trunk/additional/unstruc/src/unstruc_bmi.f90
        public const int MAXDIMS = 6;
        public const int MAXSTRLEN = 1024; // Must be equal to MAXSTRLEN in

        static FlexibleMeshModelDll()
        {
            DimrApiDataSet.AddKernelDirToPath();
            NativeLibrary.LoadNativeDll(DimrApiDataSet.DFlowFmDllName, DimrApiDataSet.DFlowFmDllDirectory);
        }

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int initialize([In] string mduFile);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_version_string", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_version_string([Out] StringBuilder vers);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_start_time", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_start_time([Out] out double t);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_end_time", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_end_time([Out] out double t);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_current_time", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_current_time([Out] out double t);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_time_step", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_time_step([Out] out double dt);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_string_attribute", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_string_attribute([In] string name, [Out] StringBuilder value);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "update", CallingConvention = CallingConvention.Cdecl)]
        public static extern int update([In] double dt);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_var", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_var([In] string variable, [In, Out] ref IntPtr ptr);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_var_count", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_var_count([In, Out] ref int count);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_var_name", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_var_name([In] int index, [Out] StringBuilder variable);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_var_location", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_var_location([In] string variable, [Out] StringBuilder location);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_var_shape", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_var_shape([In] string variable, [Out] int[] shape);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_var_rank", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_var_rank([In] string variable, [Out] out int rank);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_var_type", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_var_type([In] string variable, [Out] StringBuilder value);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "set_var", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_var([In] string variable, [In, Out] double [] values);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "set_compound_field", CallingConvention = CallingConvention.Cdecl)]
        public static extern void set_compound_field([In] string featureCategory, [In] string featureName, [In] string parameterName, [In] IntPtr value);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_compound_field", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_compound_field([In] string featureCategory, [In] string featureName, [In] string parameterName, [In, Out] ref IntPtr value);
        
        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "dfm_init_user_timestep", CallingConvention = CallingConvention.Cdecl)]
        public static extern int dfm_init_user_timestep([In, Out] ref double targetTime);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "dfm_finalize_user_timestep", CallingConvention = CallingConvention.Cdecl)]
        public static extern int dfm_finalize_user_timestep();

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "dfm_init_computational_timestep", CallingConvention = CallingConvention.Cdecl)]
        public static extern int dfm_init_computational_timestep([In, Out] ref double targetTime, [In, Out] ref double timeStep);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "dfm_compute_1d2d_coefficients", CallingConvention = CallingConvention.Cdecl)]
        public static extern int dfm_compute_1d2d_coefficients();

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "dfm_run_computational_timestep", CallingConvention = CallingConvention.Cdecl)]
        public static extern int dfm_run_computational_timestep([In, Out] ref double actualTimeStep);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "dfm_finalize_computational_timestep", CallingConvention = CallingConvention.Cdecl)]
        public static extern int dfm_finalize_computational_timestep();

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "get_snapped_feature", CallingConvention = CallingConvention.Cdecl)]
        public static extern void get_snapped_feature([In] string featureType, [In] ref int numIn, [In] ref IntPtr xin,
                                                      [In] ref IntPtr yin, [In, Out] ref int numOut,
                                                      [In, Out] ref IntPtr xout, [In, Out] ref IntPtr yout,
                                                      [In, Out] ref IntPtr featureIds, ref int errorCode);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "write_netgeom", CallingConvention = CallingConvention.Cdecl)]
        public static extern void write_net_geom([In] string filePath);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "write_partition_metis", CallingConvention = CallingConvention.Cdecl)]
        public static extern void write_partition_metis([In] string inputfilePath, [In] string outputFilePath, [In] ref int numDomains, [In] ref int contiguous);

        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "write_partition_pol", CallingConvention = CallingConvention.Cdecl)]
        public static extern void write_partition_pol([In] string inputfilePath, [In] string outputFilePath, [In] string polFilePath);
        
        [DllImport(DimrApiDataSet.DFlowFmDllName, EntryPoint = "finalize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void finalize();

    }
}