using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridApi1D : GridApi, IUGridApi1D
    {
        private int networkId;

        private int nNodes;
        private int nBranches;
        private int nGeometryPoints;

        public UGridApi1D()
        {
            networkId = -1;

            nNodes = -1;
            nBranches = -1;
            nGeometryPoints = -1;
        }

        #region Implementation of IUGridApi1D

        public int Create1DNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints)
        {
            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            int ierr = GridApiDataSet.GridConstants.IONC_NOERR;
            try
            {
                ierr = GridWrapper.ionc_create_1d_network(ref ioncid, ref networkId, name, ref numberOfNodes, ref numberOfBranches, ref totalNumberOfGeometryPoints);
            }
            catch
            {
                ierr = GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            if (ierr == GridApiDataSet.GridConstants.IONC_NOERR)
            {
                nNodes = numberOfNodes;
                nBranches = numberOfBranches;
                nGeometryPoints = totalNumberOfGeometryPoints;
            }
            return ierr;
        }
        
        public int Write1DNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            if (!Initialized || !NetworkReady) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            var numberOfNodes = GetNumberOfNetworkNodes();
            if (numberOfNodes < 0
                || numberOfNodes != nodesX.Length
                || numberOfNodes != nodesY.Length
                || numberOfNodes != nodesids.Length
                || numberOfNodes != nodeslongNames.Length) return GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR;

            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double))*numberOfNodes);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double))*numberOfNodes);
            try
            {

                Marshal.Copy(nodesX, 0, xPtr, numberOfNodes);
                Marshal.Copy(nodesY, 0, yPtr, numberOfNodes);
                GridWrapper.interop_charinfo[] nodesinfo = new GridWrapper.interop_charinfo[4];
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
                var ierr = GridWrapper.ionc_write_1d_network_nodes(ref ioncid, ref networkId, ref xPtr, ref yPtr,
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
            if (!Initialized || !NetworkReady) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            var numberOfBranches = GetNumberOfNetworkBranches();
            if (numberOfBranches < 0
                || numberOfBranches != sourceNodeId.Length
                || numberOfBranches != targetNodeId.Length
                || numberOfBranches != branchLengths.Length
                || numberOfBranches != nbranchgeometrypoints.Length
                || numberOfBranches != branchIds.Length
                || numberOfBranches != branchLongnames.Length)
                return GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR;

            IntPtr sourceIdPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int))*numberOfBranches);
            IntPtr targetIdPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int))*numberOfBranches);

            IntPtr branchLengthsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double))*numberOfBranches);
            IntPtr nrOfGeometryPointsInBranchPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int))*numberOfBranches);

            try
            {
                Marshal.Copy(sourceNodeId, 0, sourceIdPtr, numberOfBranches);
                Marshal.Copy(targetNodeId, 0, targetIdPtr, numberOfBranches);

                Marshal.Copy(branchLengths, 0, branchLengthsPtr, numberOfBranches);
                Marshal.Copy(nbranchgeometrypoints, 0, nrOfGeometryPointsInBranchPtr, numberOfBranches);

                GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[3];
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
                var ierr = GridWrapper.ionc_write_1d_network_branches(ref ioncid, ref networkId, ref sourceIdPtr,
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
            if (!Initialized || !NetworkReady) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            var numberOfGeometryPoints = GetNumberOfNetworkGeometryPoints();
            if (numberOfGeometryPoints < 0
                || numberOfGeometryPoints != geopointsX.Length
                || numberOfGeometryPoints != geopointsY.Length)
                return GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR;

            IntPtr geopointsXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double))*numberOfGeometryPoints);
            IntPtr geopointsYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double))*numberOfGeometryPoints);

            try
            {
                Marshal.Copy(geopointsX, 0, geopointsXPtr, numberOfGeometryPoints);
                Marshal.Copy(geopointsY, 0, geopointsYPtr, numberOfGeometryPoints);
                var ierr = GridWrapper.ionc_write_1d_network_branches_geometry(ref ioncid, ref networkId,
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
            if (Initialized && NetworkReady && nNodes > 0)
            {
                return nNodes;
            }
            int rnNodes = -1;
            try
            {
                var ierr = GridWrapper.ionc_get_1d_network_nodes_count(ref ioncid, ref networkId, ref rnNodes);
                if (ierr < 0)
                    Log.ErrorFormat("Couldn't get number of network nodes because of io netcdf error nr : {0}", ierr);
            }
            catch
            {
                // on exception don't crash...
                Log.ErrorFormat("Couldn't get number of network nodes");
            }
            nNodes = rnNodes;
            return nNodes;
        }

        public virtual int GetNumberOfNetworkBranches()
        {
            if (Initialized && NetworkReady && nBranches > 0)
            {
                return nBranches;
            }
            int rnBranches = -1;
            try
            {
                var ierr = GridWrapper.ionc_get_1d_network_branches_count(ref ioncid, ref networkId, ref rnBranches);
                if (ierr < 0)
                    Log.ErrorFormat("Couldn't get number of branches because of io netcdf error nr : {0}", ierr);
            }
            catch
            {
                // on exception don't crash...
                Log.ErrorFormat("Couldn't get number of branches");
            }
            nBranches = rnBranches;
            return nBranches;
        }

        public virtual int GetNumberOfNetworkGeometryPoints()
        {
            if (Initialized && NetworkReady && nGeometryPoints > 0)
            {
                return nGeometryPoints;
            }
            int rnGeometryPoints = -1;
            try
            {
                var ierr = GridWrapper.ionc_get_1d_network_branches_geometry_coordinate_count(ref ioncid, ref networkId,
                    ref rnGeometryPoints);
                if (ierr < 0)
                    Log.ErrorFormat("Couldn't get number of geometry points because of io netcdf error nr : {0}", ierr);
            }
            catch
            {
                // on exception don't crash...
                Log.ErrorFormat("Couldn't get number of geometry points");
            }
            nGeometryPoints = rnGeometryPoints;
            return nGeometryPoints;
        }


        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            Close();
        }

        #endregion

        public bool NetworkReady
        {
            get { return networkId > 0; }
        }
    }
}