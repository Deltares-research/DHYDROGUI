using System;
using System.Runtime.InteropServices;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridApi1DDiscretisation : GridApi, IUGridApi1DDiscretisation
    {
        private int meshId;
        private int nMeshPoints;
        private int nMeshEdges;

        public UGridApi1DDiscretisation()
        {
            // Get the 1D network id in the constructor? While opening the file?
            // Obtain meshIds, networkIds?
            meshId = -1;
            nMeshPoints = -1;
            nMeshEdges = -1;
        }

        #region Write 1D discretisation

        public int Create1dDiscretisation(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId)
        {
            //meshId = identifier;
            if (!Initialized)
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
                ierr = wrapper.ionc_create_1d_mesh(ref ioncid, ref networkId, ref meshId, name, ref numberOfMeshPoints, ref numberOfMeshEdges);

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

        public int Write1dDiscretisationPoints(int[] branchIdx, double[] offset)
        {
            if (!Initialized || !NetworkReady)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }

            int numberOfMeshPoints = GetNumberOf1dDiscretisationPoints();

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

                var ierr = wrapper.ionc_write_1d_mesh_discretisation_points(ref ioncid, ref meshId, ref branchIdxPtr,
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

        #endregion

        #region Read 1D discretisation

        public int GetNumberOf1dDiscretisationPoints()
        {
            if (Initialized && NetworkReady && nMeshPoints > 0)
            {
                return nMeshPoints;
            }
            int numberOfMeshPoints = -1;
            try
            {
                var ierr = wrapper.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref meshId, ref numberOfMeshPoints);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }
            }
            catch
            {
                // on exception don't crash...
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            nMeshPoints = numberOfMeshPoints;
            return numberOfMeshPoints;
        }

        public int Read1dDiscretisationPoints(out int[] branchIdx, out double[] offset)
        {
            branchIdx = new int[0];
            offset = new double[0];

            var numberOfMeshPoints = GetNumberOf1dDiscretisationPoints();
            if (Initialized || !NetworkReady || numberOfMeshPoints < 0)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            }
            
            IntPtr branchIdxPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nMeshPoints);
            IntPtr offsetPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nMeshPoints);

            try
            {
                var ierr = wrapper.ionc_read_1d_mesh_discretisation_points(ref ioncid, ref meshId, ref branchIdxPtr, ref offsetPtr, ref numberOfMeshPoints);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    return ierr;
                }

                branchIdx = new int[numberOfMeshPoints];
                offset = new double[numberOfMeshPoints];

                Marshal.Copy(branchIdxPtr, branchIdx, 0, numberOfMeshPoints);
                Marshal.Copy(offsetPtr, offset, 0, numberOfMeshPoints);

                return GridApiDataSet.GridConstants.IONC_NOERR;
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

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            Close();
        }

        #endregion

        public virtual bool NetworkReady
        {
            get { return meshId > 0; }
        }
    }
}