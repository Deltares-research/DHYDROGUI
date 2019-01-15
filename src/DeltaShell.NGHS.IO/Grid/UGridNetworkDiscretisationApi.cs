using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridNetworkDiscretisationApi : GridApi, IUGridNetworkDiscretisationApi
    {
        private int meshIdForWriting;

        public UGridNetworkDiscretisationApi()
        {
            meshIdForWriting = -1;
        }

        #region Write Network Discretisation

        public int CreateNetworkDiscretisation(int numberOfNetworkPoints)
        {
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            // ReSharper disable once RedundantAssignment
            int ierr = GridApiDataSet.GridConstants.NOERR;

            try
            {
                ierr = wrapper.Create1DMesh(ioncId, GridApiDataSet.DataSetNames.Network, ref meshIdForWriting, GridApiDataSet.DataSetNames.Mesh1D, numberOfNetworkPoints);

                if (ierr != GridApiDataSet.GridConstants.NOERR) return ierr;
            }
            catch
            {
                ierr = GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            return ierr;
        }

        public int WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset, double[] discretisationPointsX, double[] discretisationPointsY, string[] discretisationPointIds, string[] discretisationPointLongnames)
        {
            if (!Initialized || !NetworkReadyForWriting)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            if (branchIdx.Length < 0
                || offset.Length != branchIdx.Length)
            {
                return GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR;
            }

            var numberOfDiscretisationPoints = branchIdx.Length;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;
            IntPtr discretisationPointsXPtr = IntPtr.Zero;
            IntPtr discretisationPointsYPtr = IntPtr.Zero;
            IntPtr edgeNodesPtr = IntPtr.Zero;

            try
            {
                const int startIndex = 0;
                var numberOfEdgeNodes = 0;

                branchIdxPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfDiscretisationPoints);
                offsetPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfDiscretisationPoints);
                discretisationPointsXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfDiscretisationPoints);
                discretisationPointsYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfDiscretisationPoints);
                edgeNodesPtr = Marshal.AllocCoTaskMem(2 * Marshal.SizeOf(typeof(int)) * numberOfEdgeNodes);

                Marshal.Copy(branchIdx, 0, branchIdxPtr, numberOfDiscretisationPoints);
                Marshal.Copy(offset, 0, offsetPtr, numberOfDiscretisationPoints);
                Marshal.Copy(discretisationPointsX, 0, discretisationPointsXPtr, numberOfDiscretisationPoints);
                Marshal.Copy(discretisationPointsY, 0, discretisationPointsYPtr, numberOfDiscretisationPoints);
                var idInfo = new GridWrapper.interop_charinfo[numberOfDiscretisationPoints];
                for (var i = 0; i < numberOfDiscretisationPoints; i++)
                {
                    string tmpString;
                    tmpString = discretisationPointIds[i] ?? string.Empty;
                    tmpString = tmpString.PadRight(GridWrapper.idssize, ' ');
                    idInfo[i].ids = tmpString.ToCharArray();
                    tmpString = discretisationPointLongnames[i] ?? string.Empty;
                    tmpString = tmpString.PadRight(GridWrapper.longnamessize, ' ');
                    idInfo[i].longnames = tmpString.ToCharArray();
                }

                return wrapper.Write1DMeshDiscretisationPoints(ioncId, meshIdForWriting, branchIdxPtr, offsetPtr, discretisationPointsXPtr, discretisationPointsYPtr, idInfo, numberOfDiscretisationPoints, startIndex);
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (branchIdxPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(branchIdxPtr);
                branchIdxPtr = IntPtr.Zero;
                if (offsetPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(offsetPtr);
                offsetPtr = IntPtr.Zero;
                if (edgeNodesPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(edgeNodesPtr);
                edgeNodesPtr = IntPtr.Zero;
                if (discretisationPointsXPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(discretisationPointsXPtr);
                discretisationPointsXPtr = IntPtr.Zero;
                if (discretisationPointsYPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(discretisationPointsYPtr);
                discretisationPointsYPtr = IntPtr.Zero;
            }
        }

        #endregion

        #region Read Network Discretisation

        public int GetNetworkIdFromMeshId(int meshId, out int networkId)
        {
            networkId = 0;
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            try
            {
                return wrapper.GetNetworkIdFromMeshId(ioncId, meshId, ref networkId);
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int GetNetworkDiscretisationName(int meshId, out string meshName)
        {
            meshName = string.Empty;
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            try
            {
                var name = new StringBuilder(GridApiDataSet.GridConstants.MAXSTRLEN);
                var ierr = wrapper.GetMeshName(ioncId, meshId, name);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
                meshName = name.ToString();
                return GridApiDataSet.GridConstants.NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int GetNumberOfNetworkDiscretisationPoints(int meshId, out int numberOfDiscretisationPoints)
        {
            numberOfDiscretisationPoints = 0;
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            try
            {
                return wrapper.Get1DMeshDiscretisationPointsCount(ioncId, meshId, ref numberOfDiscretisationPoints);
            }
            catch
            {
                //on exception don't crash...
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
        }

        public int ReadNetworkDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset, out double[] discretisationPointsX, out double[] discretisationPointsY, out string[] ids, out string[] names)
        {
            branchIdx = new int[0];
            offset = new double[0];
            ids = new string[0];
            names = new string[0];
            discretisationPointsX = new double[0];
            discretisationPointsY = new double[0];

            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            int numberOfDiscretisationPoints;
            var ierr = GetNumberOfNetworkDiscretisationPoints(meshId, out numberOfDiscretisationPoints);
            if (ierr != GridApiDataSet.GridConstants.NOERR) return ierr;
            if (numberOfDiscretisationPoints < 0) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;
            IntPtr discretisationPointsXPtr = IntPtr.Zero;
            IntPtr discretisationPointsYPtr = IntPtr.Zero;

            try
            {
                branchIdxPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfDiscretisationPoints);
                offsetPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfDiscretisationPoints);
                discretisationPointsXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfDiscretisationPoints);
                discretisationPointsYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfDiscretisationPoints);

                var meshpointsinfo = new GridWrapper.interop_charinfo[numberOfDiscretisationPoints];
                var startIndex = 0;
                ierr = wrapper.Read1DMeshDiscretisationPoints(ioncId, meshId, ref branchIdxPtr, ref offsetPtr, ref discretisationPointsXPtr, ref discretisationPointsYPtr, meshpointsinfo, numberOfDiscretisationPoints, startIndex);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                var tmpbranchIdx = new int[numberOfDiscretisationPoints];
                var tmpoffset = new double[numberOfDiscretisationPoints];
                var tmpdiscretisationPointsX = new double[numberOfDiscretisationPoints];
                var tmpdiscretisationPointsY = new double[numberOfDiscretisationPoints];

                Marshal.Copy(branchIdxPtr, tmpbranchIdx, 0, numberOfDiscretisationPoints);
                Marshal.Copy(offsetPtr, tmpoffset, 0, numberOfDiscretisationPoints);
                Marshal.Copy(discretisationPointsXPtr, tmpdiscretisationPointsX, 0, numberOfDiscretisationPoints);
                Marshal.Copy(discretisationPointsYPtr, tmpdiscretisationPointsY, 0, numberOfDiscretisationPoints);

                var tmpids = new string[numberOfDiscretisationPoints];
                var tmpnames = new string[numberOfDiscretisationPoints];

                for (int i = 0; i < numberOfDiscretisationPoints; ++i)
                {
                    tmpids[i] = new string(meshpointsinfo[i].ids).Trim();
                    tmpnames[i] = new string(meshpointsinfo[i].longnames).Trim();
                }

                var countRealpoints = tmpbranchIdx.Count(id => id != int.MinValue + 1);
                branchIdx = new int[countRealpoints];
                offset = new double[countRealpoints];
                discretisationPointsX = new double[countRealpoints];
                discretisationPointsY = new double[countRealpoints];
                ids = new string[countRealpoints];
                names = new string[countRealpoints];
                var j = 0;
                for (int i = 0; i < numberOfDiscretisationPoints; i++)
                {
                    if(tmpbranchIdx[i] == int.MinValue + 1) continue;
                    branchIdx[j] = tmpbranchIdx[i];
                    offset[j] = tmpoffset[i];
                    discretisationPointsX[j] = tmpdiscretisationPointsX[i];
                    discretisationPointsY[j] = tmpdiscretisationPointsY[i];
                    ids[j] = tmpids[i];
                    names[j] = tmpnames[i];
                    j++;
                }
                return GridApiDataSet.GridConstants.NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            finally
            {
                if (branchIdxPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(branchIdxPtr);
                branchIdxPtr = IntPtr.Zero;
                if (offsetPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(offsetPtr);
                offsetPtr = IntPtr.Zero;
                if (discretisationPointsXPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(discretisationPointsXPtr);
                discretisationPointsXPtr = IntPtr.Zero;
                if (discretisationPointsYPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(discretisationPointsYPtr);
                discretisationPointsYPtr = IntPtr.Zero;
            }
        }
        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            Close();
        }

        #endregion

        public virtual bool NetworkReadyForWriting
        {
            get { return meshIdForWriting > 0; }
        }
    }
}