using System;
using System.Runtime.InteropServices;
using System.Text;
using DeltaShell.NGHS.IO.Helpers;

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

        public int CreateNetworkDiscretisation(string name, int numberOfNetworkPoints, int numberOfMeshEdges, int networkId)
        {
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            // replace spaces in network name by underscores
            if (name != null) name = name.Replace(' ', '_');

            // ReSharper disable once RedundantAssignment
            int ierr = GridApiDataSet.GridConstants.NOERR;

            try
            {
                ierr = wrapper.Create1DMesh(ioncId, networkId, ref meshIdForWriting, name, numberOfNetworkPoints, numberOfMeshEdges);

                if (ierr != GridApiDataSet.GridConstants.NOERR) return ierr;
            }
            catch
            {
                ierr = GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            return ierr;
        }

        public int WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset)
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
            
            try
            {
                branchIdxPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfDiscretisationPoints);
                offsetPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfDiscretisationPoints);

                Marshal.Copy(branchIdx, 0, branchIdxPtr, numberOfDiscretisationPoints);
                Marshal.Copy(offset, 0, offsetPtr, numberOfDiscretisationPoints);

                return wrapper.Write1DMeshDiscretisationPoints(ioncId, meshIdForWriting, branchIdxPtr,
                    offsetPtr, numberOfDiscretisationPoints);
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
            }
        }

        public int WriteNetworkDiscretisationPointsIds(int numberOfDiscretisationPoints, string[] discretisationPointIds)
        {
            if (!Initialized || !NetworkReadyForWriting)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            discretisationPointIds = discretisationPointIds.ReplaceSpacesInString();

            try
            {
                StringBuilder discretisationPointIdStorageName = new StringBuilder(GridApiDataSet.UGridApiConstants.DiscretisationPointIds);
                GridWrapper.interop_charinfo[] idInfo = new GridWrapper.interop_charinfo[numberOfDiscretisationPoints];
                for (int i = 0; i < numberOfDiscretisationPoints; ++i)
                {
                    string tmpString;
                    tmpString = discretisationPointIds[i] ?? string.Empty;
                    tmpString = tmpString.PadRight(GridWrapper.idssize, ' ');
                    idInfo[i].ids = tmpString.ToCharArray();
                }
                var ierr = wrapper.CreateNetworkDiscretisationPointIds(ioncId, meshIdForWriting, (int)GridApiDataSet.LocationType.UG_LOC_NODE);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                ierr = wrapper.WriteNetworkDiscretisationPointIds(ioncId, meshIdForWriting, discretisationPointIdStorageName, idInfo, numberOfDiscretisationPoints);
                return ierr;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
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

        public int ReadNetworkDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset)
        {
            branchIdx = new int[0];
            offset = new double[0];
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            int numberOfDiscretisationPoints;

            try
            {
                var ierr = GetNumberOfNetworkDiscretisationPoints(meshId, out numberOfDiscretisationPoints);
                if (ierr != GridApiDataSet.GridConstants.NOERR) return ierr;
                if (numberOfDiscretisationPoints < 0) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            IntPtr branchIdxPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfDiscretisationPoints);
            IntPtr offsetPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfDiscretisationPoints);

            try
            {
                var ierr = wrapper.Read1DMeshDiscretisationPoints(ioncId, meshId, ref branchIdxPtr, ref offsetPtr, numberOfDiscretisationPoints);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                branchIdx = new int[numberOfDiscretisationPoints];
                offset = new double[numberOfDiscretisationPoints];

                Marshal.Copy(branchIdxPtr, branchIdx, 0, numberOfDiscretisationPoints);
                Marshal.Copy(offsetPtr, offset, 0, numberOfDiscretisationPoints);

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
            }
        }

        public int ReadNetworkDiscretisationPointIds(int meshId, out string[] ids)
        {
            ids = new string[0];
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;

            int numberOfDiscretisationPoints;

            try
            {
                var ierr = GetNumberOfNetworkDiscretisationPoints(meshId, out numberOfDiscretisationPoints);
                if (ierr != GridApiDataSet.GridConstants.NOERR) return ierr;
                if (numberOfDiscretisationPoints < 0) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            try
            {
                StringBuilder discretisationPointIdStorageName = new StringBuilder(GridApiDataSet.UGridApiConstants.DiscretisationPointIds);
                GridWrapper.interop_charinfo[] idInfo = new GridWrapper.interop_charinfo[numberOfDiscretisationPoints];
                var ierr = wrapper.ReadNetworkDiscretisationPointIds(ioncId, meshId, discretisationPointIdStorageName, idInfo, numberOfDiscretisationPoints);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                ids = new string[numberOfDiscretisationPoints];
                for (int i = 0; i < numberOfDiscretisationPoints; ++i)
                {
                    ids[i] = new string(idInfo[i].ids).Trim();
                }

                return GridApiDataSet.GridConstants.NOERR;
            }
            catch
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
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