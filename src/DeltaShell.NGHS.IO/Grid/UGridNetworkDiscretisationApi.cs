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

        public int CreateNetworkDiscretisation(int numberOfMeshPoints, int numberOfMeshEdges)
        {
            if (!Initialized)
            {
                return GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            // ReSharper disable once RedundantAssignment
            int ierr = GridApiDataSet.GridConstants.NOERR;

            try
            {
                ierr = wrapper.Create1DMesh(ioncId, GridApiDataSet.DataSetNames.Network, ref meshIdForWriting, GridApiDataSet.DataSetNames.Mesh1D, numberOfMeshPoints, numberOfMeshEdges);

                if (ierr != GridApiDataSet.GridConstants.NOERR) return ierr;
            }
            catch
            {
                ierr = GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            }

            return ierr;
        }

        public int WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset, double[] discretisationPointsX,
            double[] discretisationPointsY, int[] edgeIdx, double[] edgeOffset, double[] edgePointsX, double[] edgePointsY, int[] edgeNodes, string[] discretisationPointIds,
            string[] discretisationPointLongnames)
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
            
            IntPtr edgeIdxPtr = IntPtr.Zero;
            IntPtr edgeOffsetPtr = IntPtr.Zero;
            IntPtr edgeXPtr = IntPtr.Zero;
            IntPtr edgeYPtr = IntPtr.Zero;

            IntPtr edgeNodePtr = IntPtr.Zero;

            try
            {
                const int startIndex = 0;
                var numberOfEdgeNodes = 0;

                branchIdxPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfDiscretisationPoints);
                offsetPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfDiscretisationPoints);
                discretisationPointsXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfDiscretisationPoints);
                discretisationPointsYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfDiscretisationPoints);
                
                Marshal.Copy(branchIdx, 0, branchIdxPtr, numberOfDiscretisationPoints);
                Marshal.Copy(offset, 0, offsetPtr, numberOfDiscretisationPoints);
                Marshal.Copy(discretisationPointsX, 0, discretisationPointsXPtr, numberOfDiscretisationPoints);
                Marshal.Copy(discretisationPointsY, 0, discretisationPointsYPtr, numberOfDiscretisationPoints);
                //Marshal.Copy(ed, 0, discretisationPointsYPtr, numberOfDiscretisationPoints);

                int ierr;
                using (var register = new UnmanagedMemoryRegister())
                {
                    var idsBuffer = StringBufferHandling.MakeStringBuffer(ref discretisationPointIds, GridWrapper.idssize);
                    var longNamesBuffer = StringBufferHandling.MakeStringBuffer(ref discretisationPointLongnames, GridWrapper.longnamessize);
                    IntPtr idsPtr = register.AddString(ref idsBuffer);
                    IntPtr longNamesPtr = register.AddString(ref longNamesBuffer);

                    ierr = wrapper.Write1DMeshDiscretisationPoints(ioncId, meshIdForWriting, branchIdxPtr, offsetPtr, discretisationPointsXPtr, discretisationPointsYPtr, idsPtr, longNamesPtr, numberOfDiscretisationPoints, startIndex);
                    //return ierr;
                }

                var numberOfEdges = edgeIdx.Length;
                edgeIdxPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfEdges);
                edgeOffsetPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfEdges);
                edgeXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfEdges);
                edgeYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOfEdges);

                Marshal.Copy(edgeIdx, 0, edgeIdxPtr, numberOfEdges);
                Marshal.Copy(edgeOffset, 0, edgeOffsetPtr, numberOfEdges);
                Marshal.Copy(edgePointsX, 0, edgeXPtr, numberOfEdges);
                Marshal.Copy(edgePointsX, 0, edgeYPtr, numberOfEdges);
                
                ierr = wrapper.Write1dMeshEdges(ref ioncId, ref meshIdForWriting, ref edgeIdxPtr, ref edgeOffsetPtr, ref numberOfEdges, startIndex,ref edgeXPtr, ref edgeYPtr);

                edgeNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfEdges * 2);
                Marshal.Copy(edgeNodes, 0, edgeNodePtr, numberOfEdges*2);
                ierr = wrapper.Write1dMeshEdgeNodes(ref ioncId, ref meshIdForWriting, ref numberOfEdges, ref edgeNodePtr, startIndex);
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
                if (edgeNodePtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(edgeNodePtr);
                edgeNodePtr = IntPtr.Zero;
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

                using (var register = new UnmanagedMemoryRegister())
                {
                    var idsBuffer = StringBufferHandling.MakeStringBuffer(numberOfDiscretisationPoints, GridWrapper.idssize);
                    var longNamesBuffer = StringBufferHandling.MakeStringBuffer(numberOfDiscretisationPoints, GridWrapper.longnamessize);
                    IntPtr idsPtr = register.AddString(ref idsBuffer);
                    IntPtr longNamesPtr = register.AddString(ref longNamesBuffer);

                    var startIndex = 0;
                    ierr = wrapper.Read1DMeshDiscretisationPoints(ioncId, meshId, ref branchIdxPtr, ref offsetPtr, ref discretisationPointsXPtr, ref discretisationPointsYPtr, ref idsPtr, ref longNamesPtr, numberOfDiscretisationPoints, startIndex);
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

                    var tmpids = StringBufferHandling.ParseString(idsPtr, numberOfDiscretisationPoints, GridWrapper.idssize).ToArray();
                    var tmpnames = StringBufferHandling.ParseString(longNamesPtr, numberOfDiscretisationPoints, GridWrapper.longnamessize).ToArray();

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
                        if (tmpbranchIdx[i] == int.MinValue + 1) continue;
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