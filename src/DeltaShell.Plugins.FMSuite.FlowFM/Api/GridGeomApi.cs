using System;
using System.IO;
using System.Runtime.InteropServices;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.Grid;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    public class GridGeomApi
    {
        protected IGridGeomWrapper geomWrapper;
        public const string LIB_DLL_NAME = "gridgeom.dll";
        private const string DFLOWFM_FOLDER_NAME = "dflowfm";
        private const string DFLOWFM_BINFOLDER_NAME = "bin";

        public static string DllDirectory
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(typeof(DimrApi).Assembly.Location), "kernels");
            }
        }

        public static string DllPath
        {
            get { return Path.Combine(DllDirectory, Environment.Is64BitProcess ? "x64" : "x86", DFLOWFM_FOLDER_NAME, DFLOWFM_BINFOLDER_NAME); }
        }
        static GridGeomApi()
        {
            DimrApiDataSet.SetSharedPath();
            NativeLibrary.LoadNativeDll(LIB_DLL_NAME, DllPath);
        }

        public GridGeomApi()
        {
            geomWrapper = new GridGeomWrapper();
        }

        #region 1d2dlinks logic

        int Convert(ref GridWrapper.meshgeom c_meshgeom, ref GridWrapper.meshgeomdim c_meshgeomdim)
        {
            try
            {
                var ierr = geomWrapper.Convert(ref c_meshgeom, ref c_meshgeomdim);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            return GridApiDataSet.GridConstants.NOERR;
        }

        int Make1d2dInternalnetlinks()
        {
            try
            {
                var ierr = geomWrapper.Make1d2dInternalnetlinks();
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            return GridApiDataSet.GridConstants.NOERR;
        }

        int Convert1dArray(int meshId, int numberOfNodes, int nBranches)
        {
            IntPtr c_meshXCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            IntPtr c_meshYCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            IntPtr c_branchids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfNodes);
            IntPtr c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);

            try
            {
                var ierr = geomWrapper.Convert1dArray(ref c_meshXCoords, ref c_meshYCoords, ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nBranches, ref numberOfNodes);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            //Free allocated memory
            if (c_meshXCoords != IntPtr.Zero)
                Marshal.FreeCoTaskMem(c_meshXCoords);
            if (c_meshYCoords != IntPtr.Zero)
                Marshal.FreeCoTaskMem(c_meshYCoords);
            if (c_branchids != IntPtr.Zero)
                Marshal.FreeCoTaskMem(c_branchids);
            if (c_sourcenodeid != IntPtr.Zero)
                Marshal.FreeCoTaskMem(c_sourcenodeid);
            if (c_targetnodeid != IntPtr.Zero)
                Marshal.FreeCoTaskMem(c_targetnodeid);

            c_meshXCoords = IntPtr.Zero;
            c_meshYCoords = IntPtr.Zero;
            c_branchids = IntPtr.Zero;
            c_sourcenodeid = IntPtr.Zero;
            c_targetnodeid = IntPtr.Zero;

            return GridApiDataSet.GridConstants.NOERR;
        }

        int GetLinkCount(ref int nbranches)
        {
            try
            {
                var ierr = geomWrapper.GetLinkCount(ref nbranches);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            return GridApiDataSet.GridConstants.NOERR;
        }

        int Get1d2dLinks(ref IntPtr arrayfrom, ref IntPtr arrayto, ref int nlinks)
        {
            try
            {
                var ierr = geomWrapper.Get1d2dLinks(ref arrayfrom, ref arrayto, ref nlinks);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            return GridApiDataSet.GridConstants.NOERR;
        }
        #endregion
    }
}