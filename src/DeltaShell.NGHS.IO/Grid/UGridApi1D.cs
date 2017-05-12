using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridApi1D : GridApi, IUGridApi1D
    {
        private int networkId;

        private int nNodes;
        private int nBranches;
        private int nGeometryPoints;
        private int nMeshPoints;
        private int nMeshEdges;

        public UGridApi1D()
        {
            networkId = -1;

            nNodes = -1;
            nBranches = -1;
            nGeometryPoints = -1;

            nMeshPoints = -1;
            nMeshEdges = -1;

        }

        #region Implementation of IUGridApi1D

        public int Create1DNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints)
        {
            if (!IsInitialized()) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            // replace spaces in network name by underscores
            if (name != null)
            {
                name = name.Replace(' ', '_');
            }

            int ierr = GridApiDataSet.GridConstants.IONC_NOERR;
            try
            {
                ierr = wrapper.ionc_create_1d_network(ref ioncid, ref networkId, name, ref numberOfNodes, ref numberOfBranches, ref totalNumberOfGeometryPoints);

                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }

                nNodes = numberOfNodes;
                nBranches = numberOfBranches;
                nGeometryPoints = totalNumberOfGeometryPoints;

                return ierr;

            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
        }

        public int Write1DNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            if (!IsInitialized() || !IsNetworkReady()) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            // replace spaces by underscores in the branch names/ids
            nodesids = nodesids.ReplaceSpacesInString();
            nodeslongNames = nodeslongNames.ReplaceSpacesInString();

            var numberOfNodes = GetNumberOfNetworkNodes();
            if (numberOfNodes < 0
                || numberOfNodes != nodesX.Length
                || numberOfNodes != nodesY.Length
                || numberOfNodes != nodesids.Length
                || numberOfNodes != nodeslongNames.Length) return GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR;

            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNodes);
            try
            {

                Marshal.Copy(nodesX, 0, xPtr, numberOfNodes);
                Marshal.Copy(nodesY, 0, yPtr, numberOfNodes);
                GridWrapper.interop_charinfo[] nodesinfo = new GridWrapper.interop_charinfo[numberOfNodes];
                for (int i = 0; i < numberOfNodes; i++)
                {
                    string tmpstring;
                    tmpstring = nodesids[i] ?? string.Empty;
                    tmpstring = tmpstring.PadRight(GridWrapper.idssize, ' ');
                    nodesinfo[i].ids = tmpstring.ToCharArray();
                    tmpstring = nodeslongNames[i] ?? string.Empty;
                    tmpstring = tmpstring.PadRight(GridWrapper.longnamessize, ' ');
                    nodesinfo[i].longnames = tmpstring.ToCharArray();
                }
                var ierr = wrapper.ionc_write_1d_network_nodes(ref ioncid, ref networkId, ref xPtr, ref yPtr,
                    nodesinfo, ref numberOfNodes);
                return ierr;
            }
            catch
            {
                // on exception don't crash
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            finally
            {
                if (xPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(xPtr);
                xPtr = IntPtr.Zero;
                if (yPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(yPtr);
                yPtr = IntPtr.Zero;
            }
        }

        public int Write1DNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths,
            int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames)
        {
            if (!IsInitialized() || !IsNetworkReady()) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            // replace spaces by underscores in the branch names/ids
            branchIds = branchIds.ReplaceSpacesInString();
            branchLongnames = branchLongnames.ReplaceSpacesInString();

            var numberOfBranches = GetNumberOfNetworkBranches();
            if (numberOfBranches < 0
                || numberOfBranches != sourceNodeId.Length
                || numberOfBranches != targetNodeId.Length
                || numberOfBranches != branchLengths.Length
                || numberOfBranches != nbranchgeometrypoints.Length
                || numberOfBranches != branchIds.Length
                || numberOfBranches != branchLongnames.Length)
                return GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR;

            IntPtr sourceIdPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfBranches);
            IntPtr targetIdPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfBranches);

            IntPtr branchLengthsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfBranches);
            IntPtr nrOfGeometryPointsInBranchPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfBranches);

            try
            {
                Marshal.Copy(sourceNodeId, 0, sourceIdPtr, numberOfBranches);
                Marshal.Copy(targetNodeId, 0, targetIdPtr, numberOfBranches);

                Marshal.Copy(branchLengths, 0, branchLengthsPtr, numberOfBranches);
                Marshal.Copy(nbranchgeometrypoints, 0, nrOfGeometryPointsInBranchPtr, numberOfBranches);

                GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[numberOfBranches];

                for (int i = 0; i < numberOfBranches; i++)
                {
                    string tmpstring;
                    tmpstring = branchIds[i] ?? string.Empty;
                    tmpstring = tmpstring.PadRight(GridWrapper.idssize, ' ');
                    branchinfo[i].ids = tmpstring.ToCharArray();
                    tmpstring = branchLongnames[i] ?? string.Empty;
                    tmpstring = tmpstring.PadRight(GridWrapper.longnamessize, ' ');
                    branchinfo[i].longnames = tmpstring.ToCharArray();
                }
                var ierr = wrapper.ionc_write_1d_network_branches(ref ioncid, ref networkId, ref sourceIdPtr,
                    ref targetIdPtr, branchinfo, ref branchLengthsPtr, ref nrOfGeometryPointsInBranchPtr,
                    ref numberOfBranches);
                return ierr;
            }
            catch
            {
                // on exception don't crash
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            finally
            {
                if (sourceIdPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(sourceIdPtr);
                sourceIdPtr = IntPtr.Zero;
                if (targetIdPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(targetIdPtr);
                targetIdPtr = IntPtr.Zero;
                if (branchLengthsPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(branchLengthsPtr);
                branchLengthsPtr = IntPtr.Zero;
                if (nrOfGeometryPointsInBranchPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(nrOfGeometryPointsInBranchPtr);
                nrOfGeometryPointsInBranchPtr = IntPtr.Zero;
            }
        }

        public int Write1DNetworkGeometry(double[] geopointsX, double[] geopointsY)
        {
            if (!IsInitialized() || !IsNetworkReady()) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            var numberOfGeometryPoints = GetNumberOfNetworkGeometryPoints();
            if (numberOfGeometryPoints < 0
                || numberOfGeometryPoints != geopointsX.Length
                || numberOfGeometryPoints != geopointsY.Length)
                return GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR;

            IntPtr geopointsXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfGeometryPoints);
            IntPtr geopointsYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfGeometryPoints);

            try
            {
                Marshal.Copy(geopointsX, 0, geopointsXPtr, numberOfGeometryPoints);
                Marshal.Copy(geopointsY, 0, geopointsYPtr, numberOfGeometryPoints);
                var ierr = wrapper.ionc_write_1d_network_branches_geometry(ref ioncid, ref networkId,
                    ref geopointsXPtr, ref geopointsYPtr, ref numberOfGeometryPoints);
                return ierr;
            }
            catch
            {
                // on exception don't crash
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            finally
            {
                if (geopointsXPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(geopointsXPtr);
                geopointsXPtr = IntPtr.Zero;
                if (geopointsYPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(geopointsYPtr);
                geopointsYPtr = IntPtr.Zero;
            }
        }

        public virtual int GetNumberOfNetworkNodes()
        {
            if (IsInitialized() && IsNetworkReady() && nNodes > 0)
            {
                return nNodes;
            }
            int rnNodes = -1;
            try
            {
                var ierr = wrapper.ionc_get_1d_network_nodes_count(ref ioncid, ref networkId, ref rnNodes);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                    //Log.ErrorFormat("Couldn't get number of network nodes because of io netcdf error nr : {0}", ierr);
                }
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
                //Log.ErrorFormat("Couldn't get number of network nodes");
            }
            nNodes = rnNodes;
            return nNodes;
        }

        public virtual int GetNumberOfNetworkBranches()
        {
            if (IsInitialized() && IsNetworkReady() && nBranches > 0)
            {
                return nBranches;
            }
            int rnBranches = -1;
            try
            {
                var ierr = wrapper.ionc_get_1d_network_branches_count(ref ioncid, ref networkId, ref rnBranches);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                    //Log.ErrorFormat("Couldn't get number of branches because of io netcdf error nr : {0}", ierr);

                }
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
                //Log.ErrorFormat("Couldn't get number of branches");
            }
            nBranches = rnBranches;
            return nBranches;
        }

        public virtual int GetNumberOfNetworkGeometryPoints()
        {
            if (IsInitialized() && IsNetworkReady() && nGeometryPoints > 0)
            {
                return nGeometryPoints;
            }
            int rnGeometryPoints = -1;
            try
            {
                var ierr = wrapper.ionc_get_1d_network_branches_geometry_coordinate_count(ref ioncid, ref networkId,
                    ref rnGeometryPoints);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }
                //Log.ErrorFormat("Couldn't get number of geometry points because of io netcdf error nr : {0}", ierr);
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
                //Log.ErrorFormat("Couldn't get number of geometry points");
            }
            nGeometryPoints = rnGeometryPoints;
            return nGeometryPoints;
        }

        #endregion

        public int Create1DMesh(string name, int numberOfMeshPoints, int numberOfMeshEdges)
        {
            if (!IsInitialized())
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            // replace spaces in network name by underscores
            if (name != null)
            {
                name = name.Replace(' ', '_');
            }

            // ReSharper disable once RedundantAssignment
            int ierr = GridApiDataSet.GridConstants.IONC_NOERR;

            try
            {
                ierr = wrapper.ionc_create_1d_mesh(ref ioncid, ref networkId, name, ref numberOfMeshPoints, ref numberOfMeshEdges);

                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }

                nMeshPoints = numberOfMeshPoints;
                nMeshEdges = numberOfMeshEdges;
            }
            catch
            {
                ierr = GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            return ierr;
        }

        public int Write1DMeshDiscretisationPoints(int[] branchIdx, double[] offset)
        {
            if (!IsInitialized() || !IsNetworkReady())
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            int numberOfMeshPoints = GetNumberOfMeshDiscretisationPoints();

            if (numberOfMeshPoints < 0
                || numberOfMeshPoints != branchIdx.Length
                || numberOfMeshPoints != offset.Length)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR;
            }

            IntPtr branchIdxPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfMeshPoints);
            IntPtr offsetPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfMeshPoints);

            try
            {
                Marshal.Copy(branchIdx, 0, branchIdxPtr, numberOfMeshPoints);
                Marshal.Copy(offset, 0, offsetPtr, numberOfMeshPoints);

                var ierr = wrapper.ionc_write_1d_mesh_discretisation_points(ref ioncid, ref networkId, ref branchIdxPtr,
                    ref offsetPtr, ref numberOfMeshPoints);
                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            finally
            {
                if (branchIdxPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(branchIdxPtr);
                branchIdxPtr = IntPtr.Zero;
                if (offsetPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(offsetPtr);
                offsetPtr = IntPtr.Zero;
            }
        }

        public virtual int GetNumberOfMeshDiscretisationPoints()
        {
            if (IsInitialized() && IsNetworkReady() && nMeshPoints > 0)
            {
                return nMeshPoints;
            }
            int numberOfMeshPoints = -1;
            try
            {
                var ierr = wrapper.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                    //Log.ErrorFormat("Couldn't get number of mesh points because of io netcdf error nr : {0}", ierr);
                }
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
                //Log.ErrorFormat("Couldn't get number of mesh points");
            }
            nMeshPoints = numberOfMeshPoints;
            return numberOfMeshPoints;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Close();
        }

        #endregion

        public virtual bool IsInitialized()
        {
            return Initialized;
        }

        public virtual bool IsNetworkReady()
        {
            return NetworkReady;
        }

        public bool NetworkReady
        {
            get { return networkId > 0; }
        }
    }
}