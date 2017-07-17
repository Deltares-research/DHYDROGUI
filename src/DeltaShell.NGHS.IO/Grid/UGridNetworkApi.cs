using System;
using System.Runtime.InteropServices;
using System.Text;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridNetworkApi : GridApi, IUGridNetworkApi
    {
        private int networkIdForWriting;
        private int nNodes;
        private int nBranches;
        private int nGeometryPoints;
        
        public UGridNetworkApi()
        {
            networkIdForWriting = -1;
            nNodes = -1;
            nBranches = -1;
            nGeometryPoints = -1;
        }

        #region Implementation of IUGridNetworkApi

        #region Write Network

        public int CreateNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int outNetworkId)
        {
            outNetworkId = -1;
            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            // replace spaces in network name by underscores
            if (name != null) name = name.Replace(' ', '_');

            try
            {
                var ierr = wrapper.ionc_create_1d_network(ref ioncid, ref networkIdForWriting, name, ref numberOfNodes, ref numberOfBranches, ref totalNumberOfGeometryPoints);

                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }
                outNetworkId = networkIdForWriting;
                nNodes = numberOfNodes;
                nBranches = numberOfBranches;
                nGeometryPoints = totalNumberOfGeometryPoints;

                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
        }

        public int WriteNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            if (!Initialized || !NetworkReadyForWriting) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            // replace spaces by underscores in the branch names/ids
            nodesids = nodesids.ReplaceSpacesInString();
            nodeslongNames = nodeslongNames.ReplaceSpacesInString();

            int numberOfNodes;
            if (GetNumberOfNetworkNodes(networkIdForWriting, out numberOfNodes) != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

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
                var ierr = wrapper.ionc_write_1d_network_nodes(ref ioncid, ref networkIdForWriting, ref xPtr, ref yPtr,
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

        public int WriteNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths,
            int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames, int[] branchOrderNumbers)
        {
            if (!Initialized || !NetworkReadyForWriting) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            // replace spaces by underscores in the branch names/ids
            branchIds = branchIds.ReplaceSpacesInString();
            branchLongnames = branchLongnames.ReplaceSpacesInString();

            int numberOfBranches;
            if (GetNumberOfNetworkBranches(networkIdForWriting, out numberOfBranches) != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            if (numberOfBranches < 0
                || numberOfBranches != sourceNodeId.Length
                || numberOfBranches != targetNodeId.Length
                || numberOfBranches != branchLengths.Length
                || numberOfBranches != nbranchgeometrypoints.Length
                || numberOfBranches != branchIds.Length
                || numberOfBranches != branchLongnames.Length
                || numberOfBranches != branchOrderNumbers.Length)
                return GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR;

            IntPtr sourceIdPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfBranches);
            IntPtr targetIdPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfBranches);

            IntPtr branchLengthsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfBranches);
            IntPtr nrOfGeometryPointsInBranchPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfBranches);
            IntPtr branchOrderNumbersPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfBranches);

            try
            {
                Marshal.Copy(sourceNodeId, 0, sourceIdPtr, numberOfBranches);
                Marshal.Copy(targetNodeId, 0, targetIdPtr, numberOfBranches);

                Marshal.Copy(branchLengths, 0, branchLengthsPtr, numberOfBranches);
                Marshal.Copy(nbranchgeometrypoints, 0, nrOfGeometryPointsInBranchPtr, numberOfBranches);
                Marshal.Copy(branchOrderNumbers, 0, branchOrderNumbersPtr, numberOfBranches);

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
                var ierr = wrapper.ionc_write_1d_network_branches(ref ioncid, ref networkIdForWriting, ref sourceIdPtr,
                    ref targetIdPtr, branchinfo, ref branchLengthsPtr, ref nrOfGeometryPointsInBranchPtr,
                    ref numberOfBranches);
                if (ierr == GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    ierr = wrapper.ionc_put_1d_network_branchorder(ioncid, networkIdForWriting, branchOrderNumbersPtr, numberOfBranches);
                }
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
                if (branchOrderNumbersPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(branchOrderNumbersPtr);
                branchOrderNumbersPtr = IntPtr.Zero;
            }
        }

        public int WriteNetworkGeometry(double[] geopointsX, double[] geopointsY)
        {
            if (!Initialized || !NetworkReadyForWriting) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;

            int numberOfGeometryPoints;
            if (GetNumberOfNetworkGeometryPoints(networkIdForWriting, out numberOfGeometryPoints) != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
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
                var ierr = wrapper.ionc_write_1d_network_branches_geometry(ref ioncid, ref networkIdForWriting,
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

        #endregion

        #region Read Network

        public int GetNetworkName(int networkId, out string networkName)
        {
            networkName = string.Empty;
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            
            var name = new StringBuilder(GridApiDataSet.GridConstants.MAXSTRLEN);
            var ierr = wrapper.ionc_get_network_name(ref ioncid, ref networkId, name);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                return ierr;
            }
            networkName = name.ToString();
            return GridApiDataSet.GridConstants.IONC_NOERR;
        }

        public virtual int GetNumberOfNetworkNodes(int networkId, out int numberOfNetworkNodes)
        {
            numberOfNetworkNodes = -1;
            if (Initialized && nNodes > 0)
            {
                numberOfNetworkNodes = nNodes;
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            
            try
            {
                int rnNodes = -1;
                var ierr = wrapper.ionc_get_1d_network_nodes_count(ref ioncid, ref networkId, ref rnNodes);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }
                numberOfNetworkNodes = rnNodes;
                nNodes = rnNodes;
                return ierr;
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
        }

        public int ReadNetworkNodes(int networkId, out double[] nodesX, out double[] nodesY, out string[] nodesIds, out string[] nodesLongnames)
        {
            nodesX = new double[0];
            nodesY = new double[0];
            nodesIds = new string[0];
            nodesLongnames = new string[0];

            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            if (nNodes < 0)
            {
                int numberOfNetworkNodes;
                GetNumberOfNetworkNodes(networkId, out numberOfNetworkNodes);
                if (nNodes < 0) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            IntPtr nodesXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr nodesYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);

            try
            {
                var nodesinfo = new GridWrapper.interop_charinfo[nNodes];

                var ierr = wrapper.ionc_read_1d_network_nodes(ref ioncid, ref networkId, ref nodesXPtr, ref nodesYPtr, nodesinfo, ref nNodes);

                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }

                nodesX = new double[nNodes];
                nodesY = new double[nNodes];

                Marshal.Copy(nodesXPtr, nodesX, 0, nNodes);
                Marshal.Copy(nodesYPtr, nodesY, 0, nNodes);

                nodesIds = new string[nNodes];
                nodesLongnames = new string[nNodes];

                for (int i = 0; i < nNodes; ++i)
                {
                    nodesIds[i] = new string(nodesinfo[i].ids).Trim();
                    nodesLongnames[i] = new string(nodesinfo[i].longnames).Trim(); 
                }

                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            finally
            {
                if (nodesXPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(nodesXPtr);
                nodesXPtr = IntPtr.Zero;
                if (nodesYPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(nodesYPtr);
                nodesYPtr = IntPtr.Zero;
            }
        }

        public virtual int GetNumberOfNetworkBranches(int networkId, out int numberOfNetworkBranches)
        {
            numberOfNetworkBranches = -1;
            if (Initialized && nBranches > 0)
            {
                numberOfNetworkBranches = nBranches;
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            
            try
            {
                int rnBranches = -1;
                var ierr = wrapper.ionc_get_1d_network_branches_count(ref ioncid, ref networkId, ref rnBranches);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }
                nBranches = rnBranches;
                numberOfNetworkBranches = rnBranches;
                return ierr;
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
        }

        public int ReadNetworkBranches(int networkId, out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames, out int[] branchOrderNumbers)
        {
            sourceNodes = new int[0];
            targetNodes = new int[0];
            branchLengths = new double[0];
            branchGeoPoints = new int[0];
            branchIds = new string[0];
            branchLongnames = new string[0];
            branchOrderNumbers = new int[0];

            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            if (nBranches < 0)
            {
                int numberOfNetworkBranches;
                GetNumberOfNetworkBranches(networkId, out numberOfNetworkBranches);
                if (nBranches < 0) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            IntPtr sourceNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr targetNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr branchLengthPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nBranches);
            IntPtr branchGeoPointsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr branchOrderNumbersPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);

            try
            {
                var branchinfo = new GridWrapper.interop_charinfo[nBranches];
                var ierr = wrapper.ionc_read_1d_network_branches(ref ioncid, ref networkId, ref sourceNodePtr,
                    ref targetNodePtr, ref branchLengthPtr, branchinfo, ref branchGeoPointsPtr, ref nBranches);

                if (ierr == GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    ierr = wrapper.ionc_get_1d_network_branchorder(ioncid, networkId, ref branchOrderNumbersPtr, nBranches);
                }

                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }

                sourceNodes = new int[nBranches];
                targetNodes = new int[nBranches];
                branchLengths = new double[nBranches];
                branchGeoPoints = new int[nBranches];
                branchIds = new string[nBranches];
                branchLongnames = new string[nBranches];
                branchOrderNumbers = new int[nBranches];

                Marshal.Copy(sourceNodePtr, sourceNodes, 0, nBranches);
                Marshal.Copy(targetNodePtr, targetNodes, 0, nBranches);
                Marshal.Copy(branchLengthPtr, branchLengths, 0, nBranches);
                Marshal.Copy(branchGeoPointsPtr, branchGeoPoints, 0, nBranches);
                Marshal.Copy(branchOrderNumbersPtr, branchOrderNumbers, 0, nBranches);

                for (int i = 0; i < nBranches; ++i)
                {
                    branchIds[i] = new string(branchinfo[i].ids).Trim();
                    branchLongnames[i] = new string(branchinfo[i].longnames).Trim(); 
                }

                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            finally
            {
                if (sourceNodePtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(sourceNodePtr);
                sourceNodePtr = IntPtr.Zero;
                if (targetNodePtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(targetNodePtr);
                targetNodePtr = IntPtr.Zero;
                if (branchLengthPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(branchLengthPtr);
                branchLengthPtr = IntPtr.Zero;
                if (branchGeoPointsPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(branchGeoPointsPtr);
                branchGeoPointsPtr = IntPtr.Zero;
            }

        }

        public virtual int GetNumberOfNetworkGeometryPoints(int networkId, out int numberOfNetworkGeometryPoints)
        {
            numberOfNetworkGeometryPoints = -1;
            if (Initialized && nGeometryPoints > 0)
            {
                numberOfNetworkGeometryPoints = nGeometryPoints;
                return GridApiDataSet.GridConstants.IONC_NOERR;
            }
            
            try
            {
                int rnGeometryPoints = -1;
                var ierr = wrapper.ionc_get_1d_network_branches_geometry_coordinate_count(ref ioncid, ref networkId, ref rnGeometryPoints);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }
                numberOfNetworkGeometryPoints = rnGeometryPoints;
                nGeometryPoints = rnGeometryPoints;
                return ierr;
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
        }

        public int ReadNetworkGeometry(int networkId, out double[] geopointsX, out double[] geopointsY)
        {
            geopointsX = new double[0];
            geopointsY = new double[0];

            if (!Initialized) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            if (nGeometryPoints < 0)
            {
                int numberOfNetworkGeometryPoints;
                GetNumberOfNetworkGeometryPoints(networkId, out numberOfNetworkGeometryPoints);
                if(nGeometryPoints < 0) return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            IntPtr geopointsYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nGeometryPoints);
            IntPtr geopointsXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nGeometryPoints);

            try
            {
                var ierr = wrapper.ionc_read_1d_network_branches_geometry(ref ioncid, ref networkId, ref geopointsXPtr, ref geopointsYPtr, ref nGeometryPoints);

                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }

                geopointsX = new double[nGeometryPoints];
                geopointsY = new double[nGeometryPoints];

                Marshal.Copy(geopointsXPtr, geopointsX, 0, nGeometryPoints);
                Marshal.Copy(geopointsYPtr, geopointsY, 0, nGeometryPoints);

                return ierr;
            }
            catch
            {
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

        //public void SetNetworkId(int nwid)
        //{
        //    networkIdForWriting = nwid;
        //}

        #endregion

        #endregion


        #region Implementation of IDisposable

        public void Dispose()
        {
            Close();
        }

        #endregion
        

        public virtual bool NetworkReadyForWriting
        {
            get { return networkIdForWriting > 0; }
        }
    }
}