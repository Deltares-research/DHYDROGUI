using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridNetworkDiscretisationApi : GridApi, IUGridNetworkDiscretisationApi
    {
        private int meshIdForWriting;
        private int nNetworkPoints;

        public UGridNetworkDiscretisationApi()
        {
            // Get the network id in the constructor? While opening the file?
            // Obtain meshIds, networkIds?
            meshIdForWriting = -1;
            nNetworkPoints = -1;
        }

        #region Write Network Discretisation

        public int CreateNetworkDiscretisation(string name, int numberOfNetworkPoints, int numberOfMeshEdges, int networkId)
        {
            //meshId = identifier;
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
                nNetworkPoints = numberOfNetworkPoints;
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
            
            if (nNetworkPoints < 0
                || nNetworkPoints != branchIdx.Length
                || nNetworkPoints != offset.Length)
            {
                return GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR;
            }

            IntPtr branchIdxPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nNetworkPoints);
            IntPtr offsetPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNetworkPoints);

            try
            {
                Marshal.Copy(branchIdx, 0, branchIdxPtr, nNetworkPoints);
                Marshal.Copy(offset, 0, offsetPtr, nNetworkPoints);

                var ierr = wrapper.Write1DMeshDiscretisationPoints(ioncId, meshIdForWriting, branchIdxPtr,
                    offsetPtr, nNetworkPoints);
                return ierr;
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
                var ierr = wrapper.GetNetworkIdFromMeshId(ioncId, meshId, ref networkId);
                return ierr;
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
            if (Initialized && nNetworkPoints > 0)
            {
                numberOfDiscretisationPoints = nNetworkPoints;
                return GridApiDataSet.GridConstants.NOERR;
            }
            try
            {
                var ierr = wrapper.Get1DMeshDiscretisationPointsCount(ioncId, meshId, ref numberOfDiscretisationPoints);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            nNetworkPoints = numberOfDiscretisationPoints;
            return GridApiDataSet.GridConstants.NOERR;
        }

        public int ReadNetworkDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset)
        {
            branchIdx = new int[0];
            offset = new double[0];
            
            if (!Initialized) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            if (nNetworkPoints < 0)
            {
                int numberOfDiscretisationPoints;
                GetNumberOfNetworkDiscretisationPoints(meshId, out numberOfDiscretisationPoints);
                if(nNetworkPoints < 0) return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }
            
            IntPtr branchIdxPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nNetworkPoints);
            IntPtr offsetPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNetworkPoints);

            try
            {
                var ierr = wrapper.Read1DMeshDiscretisationPoints(ioncId, meshId, ref branchIdxPtr, ref offsetPtr, nNetworkPoints);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    return ierr;
                }

                branchIdx = new int[nNetworkPoints];
                offset = new double[nNetworkPoints];

                Marshal.Copy(branchIdxPtr, branchIdx, 0, nNetworkPoints);
                Marshal.Copy(offsetPtr, offset, 0, nNetworkPoints);

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