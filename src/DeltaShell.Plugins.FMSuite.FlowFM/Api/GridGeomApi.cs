using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DelftTools.Hydro;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

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

        public int Get1d2dLinksFromGridAndNetwork(string gridFilePath, IDiscretization networkDiscretization, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            var gridWrapper = new GridWrapper();

            //1. open the file with the 2d mesh
            int ioncId = 0; //file variable 
            int mode = 0;   //create in read mode
            int iConvType = 2;
            double convVersion = 0.0;

            var ierr = gridWrapper.Open(gridFilePath, mode, ref ioncId, ref iConvType, ref convVersion);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            //2. get the 2d mesh Id
            int meshId = 1;
            ierr = gridWrapper.Get2DMeshId(ref ioncId, ref meshId);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            //2.1. Fill in the data related to the 2dMesh
            int num2dNodes = 0;
            ierr = gridWrapper.GetNodeCount(ioncId, meshId, ref num2dNodes);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            int num2dEdges = 0;
            ierr = gridWrapper.GetEdgeCount(ioncId, meshId, ref num2dEdges);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            //3. get the dimensions of the 2d mesh
            GridWrapper.meshgeomdim meshtwoddim = new GridWrapper.meshgeomdim();
            ierr = gridWrapper.get_meshgeom_dim(ref ioncId, ref meshId, ref meshtwoddim);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            //4. allocate the arrays in meshgeom for storing the 2d mesh coordinates, edge_nodes
            GridWrapper.meshgeom meshtwod = new GridWrapper.meshgeom();
            meshtwod.nodex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * num2dNodes);
            meshtwod.nodey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * num2dNodes);
            meshtwod.nodez = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * num2dNodes);
            meshtwod.edge_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * num2dEdges * 2);

            //5. get the meshgeom arrays
            bool includeArrays = true;
            ierr = gridWrapper.get_meshgeom(ref ioncId, ref meshId, ref meshtwod, includeArrays);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            double[] rc_twodnodex = new double[num2dNodes];
            double[] rc_twodnodey = new double[num2dNodes];
            double[] rc_twodnodez = new double[num2dNodes];
            Marshal.Copy(meshtwod.nodex, rc_twodnodex, 0, num2dNodes);
            Marshal.Copy(meshtwod.nodey, rc_twodnodey, 0, num2dNodes);
            Marshal.Copy(meshtwod.nodez, rc_twodnodez, 0, num2dNodes);

            //6. allocate the 1d arrays for storing the 1d coordinates and edge_nodes
            int nMeshPoints = 0;
            int nBranches = 0;
            IntPtr c_meshXCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nMeshPoints);
            IntPtr c_meshYCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nMeshPoints);
            IntPtr c_branchids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nMeshPoints);
            IntPtr c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);

            var discretisationPoints = networkDiscretization.Locations.AllValues.Select(v => v.Geometry.Coordinate)
                .ToList();

            double[] meshXCoords = discretisationPoints.Select(dPoints => dPoints.X).ToArray();
            double[] meshYCoords = discretisationPoints.Select(dPoints => dPoints.Y).ToArray();
            int[] branchIds = networkDiscretization.Network.Branches.Select(b => unchecked((int) b.Id)).ToArray();
            int[] sourceNodeId = networkDiscretization.Network.Branches.Select(b => unchecked((int)b.Source.Id)).ToArray();
            int[] targetNodeId = networkDiscretization.Network.Branches.Select(b => unchecked((int)b.Target.Id)).ToArray();

            Marshal.Copy(branchIds, 0, c_branchids, nMeshPoints);
            Marshal.Copy(meshXCoords, 0, c_meshXCoords, nMeshPoints);
            Marshal.Copy(meshYCoords, 0, c_meshYCoords, nMeshPoints);
            Marshal.Copy(sourceNodeId, 0, c_sourcenodeid, nBranches);
            Marshal.Copy(targetNodeId, 0, c_targetnodeid, nBranches);

            //7. fill kn (Herman datastructure) for creating the links
            ierr = geomWrapper.Convert1dArray(ref c_meshXCoords, ref c_meshYCoords, ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nBranches, ref nMeshPoints);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            ierr = geomWrapper.Convert(ref meshtwod, ref meshtwoddim);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }
            //9. make the links
            ierr = geomWrapper.Make1d2dInternalnetlinks();
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            //10. get the number of links
            linksCount = 0;
            ierr = geomWrapper.GetLinkCount(ref linksCount);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            //11. get the links: arrayfrom = 2d cell index, arrayto = 1d node index 
            IntPtr c_arrayfrom = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * linksCount); //2d cell number
            IntPtr c_arrayto = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * linksCount); //1d node
            ierr = geomWrapper.Get1d2dLinks(ref c_arrayfrom, ref c_arrayto, ref linksCount);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }

            int[] rcArrayFrom = new int[linksCount];
            int[] rcArrayTo = new int[linksCount];
            Marshal.Copy(c_arrayfrom, rcArrayFrom, 0, linksCount);
            Marshal.Copy(c_arrayto, rcArrayTo, 0, linksCount);
            //for writing the links look io_netcdf ionc_def_mesh_contact, ionc_put_mesh_contact 

            linksFrom = rcArrayFrom.ToList();
            linksTo = rcArrayTo.ToList();

            //Free 2d arrays
            Marshal.FreeCoTaskMem(meshtwod.nodex);
            Marshal.FreeCoTaskMem(meshtwod.nodey);
            Marshal.FreeCoTaskMem(meshtwod.nodez);
            Marshal.FreeCoTaskMem(meshtwod.edge_nodes);

            //Free 1d arrays
            Marshal.FreeCoTaskMem(c_meshXCoords);
            Marshal.FreeCoTaskMem(c_meshYCoords);
            Marshal.FreeCoTaskMem(c_branchids);

            //Free from and to arrays describing the links 
            Marshal.FreeCoTaskMem(c_arrayfrom);
            Marshal.FreeCoTaskMem(c_arrayto);

            return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }


        public int Convert(ref GridWrapper.meshgeom c_meshgeom, ref GridWrapper.meshgeomdim c_meshgeomdim)
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

        public int Make1d2dInternalnetlinks()
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

        public int Convert1dArray(int meshId, int numberOfNodes, int nBranches)
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

        public int GetLinkCount(ref int nbranches)
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

        
        #endregion
    }
}