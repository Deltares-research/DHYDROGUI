using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGrid1DDiscretisation : AGrid, IUGrid1DMesh
    {
        public UGrid1DDiscretisation()
        {
        }

        public UGrid1DDiscretisation(string file, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_write) : base(file, mode)
        {
            GridApi = GridApiFactory.CreateNew1DMesh();
            //Initialize(file, mode);
        }

        #region Write 1D Discretisation

        public void Create1DMeshInFile(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId)
        {
            if (!IsInitialized()) Initialize();
            var uGridApi = GridApi as IUGridApi1DDiscretisation;
            if (uGridApi != null) // TODO: What to do when api == null?
            {
                var ierr = uGridApi.Create1dDiscretisation(name, numberOfMeshPoints, numberOfMeshEdges, networkId);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new InvalidOperationException(string.Format(
                        "Couldn't create new 1d mesh {0} with number of mesh points {1}, number of mesh edges {2} because of error number {3}",
                        name, numberOfMeshPoints, numberOfMeshEdges, ierr));
                }
            }
        }

        public void Write1DMeshDiscretizationPoints(int[] branchIdx, double[] offset)
        {
            if (!IsInitialized()) Initialize();
            var uGridApi = GridApi as IUGridApi1DDiscretisation;
            if (uGridApi != null)
            {
                var ierr = uGridApi.Write1dDiscretisationPoints(branchIdx, offset);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new InvalidOperationException(string.Format("Couldn't write 1d mesh discretisation points because of error number {0}", ierr));
                }
            }
        }

        #endregion

        #region Read 1D Discretisation

        public int GetNumberOf1DMeshDiscretisationPoints()
        {
            var uGridApi = GridApi as IUGridApi1DDiscretisation;
            if (uGridApi != null)
            {
                var numberOfMeshPoints = uGridApi.GetNumberOf1dDiscretisationPoints();
                if (numberOfMeshPoints < 0)
                {
                    throw new InvalidOperationException(string.Format("Couldn't get the number of 1D mesh discretisation points because of error number {0}", numberOfMeshPoints));
                }
                return numberOfMeshPoints;
            }
            return -1;
        }

        public int Read1DMeshDiscretisationPoints(out int[] branchIdx, out double[] offset)
        {
            branchIdx = new int[0];
            offset = new double[0];

            var uGridApi = GridApi as IUGridApi1DDiscretisation;
            if (uGridApi == null)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR; 
            }
            var ierr = uGridApi.Read1dDiscretisationPoints(out branchIdx, out offset);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't read the 1D mesh discretisation points because of error number {0}", ierr));
            }
            return ierr;
        }

        #endregion
    }
}