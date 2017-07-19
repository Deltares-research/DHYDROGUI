using System;
using System.Runtime.InteropServices;
using System.Text;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridNetworkApi : GridApi, IUGridNetworkApi
    {
        private int networkIdForWriting;
        
        public UGridNetworkApi()
        {
            networkIdForWriting = -1;

        }

        #region Implementation of IUGridNetworkApi

        #region Write Network

        public int CreateNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int outNetworkId)
        {
            outNetworkId = -1;
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            // replace spaces in network name by underscores
            if (name != null) name = name.Replace(' ', '_');

            try
            {
                var ierr = wrapper.Create1DNetwork(ioncId, ref networkIdForWriting, name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints);

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
                outNetworkId = networkIdForWriting;


                return GridApiDataSet.GridConstants.NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int WriteNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            if (!Initialized || !NetworkReadyForWriting) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            // replace spaces by underscores in the branch names/ids
            nodesids = nodesids.ReplaceSpacesInString();
            nodeslongNames = nodeslongNames.ReplaceSpacesInString();

            int numberOfNodes;
            if (GetNumberOfNetworkNodes(networkIdForWriting, out numberOfNodes) != GridApiDataSet.GridConstants.NOERR)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            if (numberOfNodes < 0
                || numberOfNodes != nodesX.Length
                || numberOfNodes != nodesY.Length
                || numberOfNodes != nodesids.Length
                || numberOfNodes != nodeslongNames.Length) return GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR;

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
                var ierr = wrapper.Write1DNetworkNodes(ioncId, networkIdForWriting, xPtr, yPtr,
                    nodesinfo, numberOfNodes);
                return ierr;
            }
            catch
            {
                // on exception don't crash
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
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
            if (!Initialized || !NetworkReadyForWriting) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            // replace spaces by underscores in the branch names/ids
            branchIds = branchIds.ReplaceSpacesInString();
            branchLongnames = branchLongnames.ReplaceSpacesInString();

            int numberOfBranches;
            if (GetNumberOfNetworkBranches(networkIdForWriting, out numberOfBranches) != GridApiDataSet.GridConstants.NOERR)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            if (numberOfBranches < 0
                || numberOfBranches != sourceNodeId.Length
                || numberOfBranches != targetNodeId.Length
                || numberOfBranches != branchLengths.Length
                || numberOfBranches != nbranchgeometrypoints.Length
                || numberOfBranches != branchIds.Length
                || numberOfBranches != branchLongnames.Length
                || numberOfBranches != branchOrderNumbers.Length)
                return GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR;

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
                var ierr = wrapper.Write1DNetworkBranches(ioncId, networkIdForWriting, sourceIdPtr,
                    targetIdPtr, branchinfo, branchLengthsPtr, nrOfGeometryPointsInBranchPtr,
                    numberOfBranches);
                if (ierr == GridApiDataSet.GridConstants.NOERR)
                {
                    ierr = wrapper.Put1DNetworkBranchorder(ioncId, networkIdForWriting, branchOrderNumbersPtr, numberOfBranches);
                }
                return ierr;
            }
            catch
            {
                // on exception don't crash
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
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
            if (!Initialized || !NetworkReadyForWriting) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            int numberOfGeometryPoints;
            if (GetNumberOfNetworkGeometryPoints(networkIdForWriting, out numberOfGeometryPoints) != GridApiDataSet.GridConstants.NOERR)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            if (numberOfGeometryPoints < 0
                || numberOfGeometryPoints != geopointsX.Length
                || numberOfGeometryPoints != geopointsY.Length)
                return GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR;

            IntPtr geopointsXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfGeometryPoints);
            IntPtr geopointsYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfGeometryPoints);

            try
            {
                Marshal.Copy(geopointsX, 0, geopointsXPtr, numberOfGeometryPoints);
                Marshal.Copy(geopointsY, 0, geopointsYPtr, numberOfGeometryPoints);
                var ierr = wrapper.Write1DNetworkBranchesGeometry(ioncId, networkIdForWriting,
                    geopointsXPtr, geopointsYPtr, numberOfGeometryPoints);
                return ierr;
            }
            catch
            {
                // on exception don't crash
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
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
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            
            var name = new StringBuilder(GridApiDataSet.GridConstants.MAXSTRLEN);
            var ierr = wrapper.GetNetworkName(ioncId, networkId, name);
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                return ierr;
            }
            networkName = name.ToString();
            return GridApiDataSet.GridConstants.NOERR;
        }

        public virtual int GetNumberOfNetworkNodes(int networkId, out int numberOfNetworkNodes)
        {
            numberOfNetworkNodes = -1;
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            
            try
            {
                return wrapper.Get1DNetworkNodesCount(ioncId, networkId, ref numberOfNetworkNodes);
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int ReadNetworkNodes(int networkId, out double[] nodesX, out double[] nodesY, out string[] nodesIds, out string[] nodesLongnames)
        {
            nodesX = new double[0];
            nodesY = new double[0];
            nodesIds = new string[0];
            nodesLongnames = new string[0];

            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            int numberOfNetworkNodes;
            try
            {
                var ierr = GetNumberOfNetworkNodes(networkId, out numberOfNetworkNodes);
                if (ierr != GridApiDataSet.GridConstants.NOERR) return ierr;
                if (numberOfNetworkNodes < 0) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            IntPtr nodesXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNetworkNodes);
            IntPtr nodesYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNetworkNodes);

            try
            {
                var nodesinfo = new GridWrapper.interop_charinfo[numberOfNetworkNodes];

                var ierr = wrapper.Read1DNetworkNodes(ioncId, networkId, ref nodesXPtr, ref nodesYPtr, nodesinfo, numberOfNetworkNodes);

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                nodesX = new double[numberOfNetworkNodes];
                nodesY = new double[numberOfNetworkNodes];

                Marshal.Copy(nodesXPtr, nodesX, 0, numberOfNetworkNodes);
                Marshal.Copy(nodesYPtr, nodesY, 0, numberOfNetworkNodes);

                nodesIds = new string[numberOfNetworkNodes];
                nodesLongnames = new string[numberOfNetworkNodes];

                for (int i = 0; i < numberOfNetworkNodes; ++i)
                {
                    nodesIds[i] = new string(nodesinfo[i].ids).Trim();
                    nodesLongnames[i] = new string(nodesinfo[i].longnames).Trim(); 
                }

                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
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
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            
            try
            {
                return wrapper.Get1DNetworkBranchesCount(ioncId, networkId, ref numberOfNetworkBranches);
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
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

            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            int numberOfNetworkBranches;
            try
            {
                var ierr = GetNumberOfNetworkBranches(networkId, out numberOfNetworkBranches);
                if (ierr != GridApiDataSet.GridConstants.NOERR) return ierr;
                if (numberOfNetworkBranches < 0) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            IntPtr sourceNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfNetworkBranches);
            IntPtr targetNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfNetworkBranches);
            IntPtr branchLengthPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfNetworkBranches);
            IntPtr branchGeoPointsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfNetworkBranches);
            IntPtr branchOrderNumbersPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfNetworkBranches);

            try
            {
                var branchinfo = new GridWrapper.interop_charinfo[numberOfNetworkBranches];
                var ierr = wrapper.Read1DNetworkBranches(ioncId, networkId, ref sourceNodePtr,
                    ref targetNodePtr, ref branchLengthPtr, branchinfo, ref branchGeoPointsPtr, numberOfNetworkBranches);

                if (ierr == GridApiDataSet.GridConstants.NOERR)
                {
                    ierr = wrapper.Get1DNetworkBranchorder(ioncId, networkId, ref branchOrderNumbersPtr, numberOfNetworkBranches);
                }

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                sourceNodes = new int[numberOfNetworkBranches];
                targetNodes = new int[numberOfNetworkBranches];
                branchLengths = new double[numberOfNetworkBranches];
                branchGeoPoints = new int[numberOfNetworkBranches];
                branchIds = new string[numberOfNetworkBranches];
                branchLongnames = new string[numberOfNetworkBranches];
                branchOrderNumbers = new int[numberOfNetworkBranches];

                Marshal.Copy(sourceNodePtr, sourceNodes, 0, numberOfNetworkBranches);
                Marshal.Copy(targetNodePtr, targetNodes, 0, numberOfNetworkBranches);
                Marshal.Copy(branchLengthPtr, branchLengths, 0, numberOfNetworkBranches);
                Marshal.Copy(branchGeoPointsPtr, branchGeoPoints, 0, numberOfNetworkBranches);
                Marshal.Copy(branchOrderNumbersPtr, branchOrderNumbers, 0, numberOfNetworkBranches);

                for (int i = 0; i < numberOfNetworkBranches; ++i)
                {
                    branchIds[i] = new string(branchinfo[i].ids).Trim();
                    branchLongnames[i] = new string(branchinfo[i].longnames).Trim(); 
                }

                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
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
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            try
            {
                return wrapper.Get1DNetworkBranchesGeometryCoordinateCount(ioncId, networkId, ref numberOfNetworkGeometryPoints);
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int ReadNetworkGeometry(int networkId, out double[] geopointsX, out double[] geopointsY)
        {
            geopointsX = new double[0];
            geopointsY = new double[0];

            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            int numberOfGeometryPoints;
            try
            {
                var ierr = GetNumberOfNetworkGeometryPoints(networkId, out numberOfGeometryPoints);
                if (ierr != GridApiDataSet.GridConstants.NOERR) return ierr;
                if (numberOfGeometryPoints < 0) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            

            IntPtr geopointsYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfGeometryPoints);
            IntPtr geopointsXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfGeometryPoints);

            try
            {
                var ierr = wrapper.Read1DNetworkBranchesGeometry(ioncId, networkId, ref geopointsXPtr, ref geopointsYPtr, numberOfGeometryPoints);

                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                geopointsX = new double[numberOfGeometryPoints];
                geopointsY = new double[numberOfGeometryPoints];

                Marshal.Copy(geopointsXPtr, geopointsX, 0, numberOfGeometryPoints);
                Marshal.Copy(geopointsYPtr, geopointsY, 0, numberOfGeometryPoints);

                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
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