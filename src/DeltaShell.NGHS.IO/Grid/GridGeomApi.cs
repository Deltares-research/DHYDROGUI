using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.Grid
{
    public class GridGeomApi
    {
        protected GridGeomWrapper geomWrapper;
        public const string LIB_DLL_NAME = "gridgeom.dll";
        private const string DFLOWFM_FOLDER_NAME = "shared";
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
            geomWrapper.DeallocateMemory();
        }

        #region 1d2dlinks logic

        public int Get1d2dLinksFromGridAndNetwork(string gridFilePath, IDiscretization networkDiscretization, ref List<int> linksFrom, ref List<int> linksTo, ref int linksCount)
        {
            IntPtr c_meshXCoords = IntPtr.Zero;
            IntPtr c_meshYCoords = IntPtr.Zero;
            IntPtr c_branchids = IntPtr.Zero;
            IntPtr c_branchoffset = IntPtr.Zero;
            IntPtr c_sourcenodeid = IntPtr.Zero;
            IntPtr c_targetnodeid = IntPtr.Zero;
            IntPtr c_branchlength = IntPtr.Zero;
            IntPtr c_arrayfrom = IntPtr.Zero;
            IntPtr c_arrayto = IntPtr.Zero;
            GridWrapper.meshgeom meshtwod = new GridWrapper.meshgeom();
            GridWrapper.meshgeomdim meshtwoddim = new GridWrapper.meshgeomdim();
            try
            {
                var ierr = geomWrapper.DeallocateMemory();
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
                var gridWrapper = new GridWrapper();

                //1. open the file with the 2d mesh
                int ioncId = 0; //file variable 
                int mode = 0; //create in read mode
                int iConvType = 2;
                double convVersion = 0.0;

                ierr = gridWrapper.Open(gridFilePath, mode, ref ioncId, ref iConvType, ref convVersion);
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
                ierr = gridWrapper.get_meshgeom_dim(ref ioncId, ref meshId, ref meshtwoddim);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                //4. allocate the arrays in meshgeom for storing the 2d mesh coordinates, edge_nodes
                meshtwod.nodex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * num2dNodes);
                meshtwod.nodey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * num2dNodes);
                meshtwod.nodez = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * num2dNodes);
                meshtwod.edge_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * num2dEdges * 2);

                //5. get the meshgeom arrays
                bool includeArrays = true;
                int startIndex = 0;
                ierr = gridWrapper.get_meshgeom(ref ioncId, ref meshId, ref meshtwod, ref startIndex, includeArrays); 
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                //Close file
                ierr = gridWrapper.Close(ioncId);
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
                var discretisationPoints = networkDiscretization.Locations.AllValues.ToList();

                int nBranches = networkDiscretization.Network.Branches.Count;
                int[] branchIds = networkDiscretization.Locations.AllValues
                    .Select(dp => networkDiscretization.Network.Branches.IndexOf(dp.Branch)).ToArray();

                int nMeshPoints = discretisationPoints.Count;

                double[] meshXCoords = discretisationPoints.Select(dPoint => dPoint.Geometry.Coordinate.X).ToArray();
                double[] meshYCoords = discretisationPoints.Select(dPoint => dPoint.Geometry.Coordinate.Y).ToArray();
                double[] branchOffset = discretisationPoints.Select(dPoint => dPoint.Chainage).ToArray();
                double[] branchLength = networkDiscretization.Network.Branches.Select(b => b.Length).ToArray();

                int[] sourceNodeId = networkDiscretization.Network.Branches
                    .Select(b => b.Network.Nodes.IndexOf(b.Source)).ToArray();
                int[] targetNodeId = networkDiscretization.Network.Branches
                    .Select(b => b.Network.Nodes.IndexOf(b.Target)).ToArray();

                c_meshXCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nMeshPoints);
                c_meshYCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nMeshPoints);
                c_branchids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nMeshPoints);
                c_branchoffset = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nMeshPoints);
                c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
                c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
                c_branchlength = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nBranches);

                Marshal.Copy(branchIds, 0, c_branchids, nMeshPoints);
                Marshal.Copy(meshXCoords, 0, c_meshXCoords, nMeshPoints);
                Marshal.Copy(meshYCoords, 0, c_meshYCoords, nMeshPoints);
                Marshal.Copy(sourceNodeId, 0, c_sourcenodeid, nBranches);
                Marshal.Copy(targetNodeId, 0, c_targetnodeid, nBranches);
                Marshal.Copy(branchOffset, 0, c_branchoffset, nMeshPoints);
                Marshal.Copy(branchLength, 0, c_branchlength, nBranches);

                //7. fill kn (Herman datastructure) for creating the links
                int start_index = 0;
                ierr = geomWrapper.Convert1dArray(ref c_meshXCoords, ref c_meshYCoords, ref c_branchoffset,
                    ref c_branchlength, ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nBranches,
                    ref nMeshPoints, ref start_index);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                ierr = geomWrapper.Convert(ref meshtwod, ref meshtwoddim, ref start_index);
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
                c_arrayfrom = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * linksCount); //2d cell number
                c_arrayto = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * linksCount); //1d node
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
            }
            catch (Exception e)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                //Free 2d arrays
                Marshal.FreeCoTaskMem(meshtwod.nodex);
                Marshal.FreeCoTaskMem(meshtwod.nodey);
                Marshal.FreeCoTaskMem(meshtwod.nodez);
                Marshal.FreeCoTaskMem(meshtwod.edge_nodes);

                //Free 1d arrays
                Marshal.FreeCoTaskMem(c_meshXCoords);
                Marshal.FreeCoTaskMem(c_meshYCoords);
                Marshal.FreeCoTaskMem(c_branchids);
                Marshal.FreeCoTaskMem(c_sourcenodeid);
                Marshal.FreeCoTaskMem(c_targetnodeid);
                Marshal.FreeCoTaskMem(c_branchlength);
                Marshal.FreeCoTaskMem(c_branchoffset);

                //Free from and to arrays describing the links 
                Marshal.FreeCoTaskMem(c_arrayfrom);
                Marshal.FreeCoTaskMem(c_arrayto);
            }

            return GridApiDataSet.GridConstants.NOERR;
        }


        public int Convert(ref GridWrapper.meshgeom c_meshgeom, ref GridWrapper.meshgeomdim c_meshgeomdim, ref int startIndex)
        {
            try
            {
                var ierr = geomWrapper.Convert(ref c_meshgeom, ref c_meshgeomdim, ref startIndex);
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

        public int Convert1dArray(int meshId, int numberOfNodes, int nBranches, int start_index)
        {
            IntPtr c_meshXCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            IntPtr c_meshYCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            IntPtr c_branchids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfNodes);
            IntPtr c_branchoffset = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            IntPtr c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr c_branchlength = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nBranches);

            try
            {
                var ierr = geomWrapper.Convert1dArray(ref c_meshXCoords, ref c_meshYCoords, ref c_branchoffset, ref c_branchlength, ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nBranches, ref numberOfNodes, ref start_index);
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
            if (c_branchoffset != IntPtr.Zero)
                Marshal.FreeCoTaskMem(c_branchoffset);
            if (c_sourcenodeid != IntPtr.Zero)
                Marshal.FreeCoTaskMem(c_sourcenodeid);
            if (c_targetnodeid != IntPtr.Zero)
                Marshal.FreeCoTaskMem(c_targetnodeid);
            if (c_branchlength != IntPtr.Zero)
                Marshal.FreeCoTaskMem(c_branchlength);

            c_meshXCoords = IntPtr.Zero;
            c_meshYCoords = IntPtr.Zero;
            c_branchids = IntPtr.Zero;
            c_branchoffset = IntPtr.Zero;
            c_sourcenodeid = IntPtr.Zero;
            c_targetnodeid = IntPtr.Zero;
            c_branchlength = IntPtr.Zero;

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