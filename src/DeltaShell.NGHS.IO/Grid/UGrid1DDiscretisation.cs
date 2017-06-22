using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGrid1DDiscretisation : AGrid, IUGrid1DMesh
    {
        public UGrid1DDiscretisation()
        {
        }

        public UGrid1DDiscretisation(string file, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : base(file, mode)
        {
            GridApi = GridApiFactory.CreateNew1DMesh();
        }

        public int GetNumberOf1DDiscretisations()
        {
            if(!IsInitialized()) Initialize();
            var uGridApi = GridApi as IUGridApi1DDiscretisation;
            if(uGridApi == null) throw new Exception(""); // TODO: DO.

            int numberOf1DDiscretisations;
            var ierr = uGridApi.GetNumberOfMeshByType(UGridMeshType.Mesh1D, out numberOf1DDiscretisations);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new Exception(string.Format("Couldn't get the number of 1D network discretisations because of error number: {0}", ierr));
            }
            return numberOf1DDiscretisations;
        }

        public int[] Get1DMeshDiscretisationIds(int numberOfMeshes)
        {
            if(!IsInitialized()) Initialize();
            var uGridApi = GridApi as IUGridApi1DDiscretisation;
            if(uGridApi == null) throw new Exception(""); // TODO: DO.

            int[] meshIds;
            var ierr = uGridApi.GetMeshIdsByType(UGridMeshType.Mesh1D, numberOfMeshes, out meshIds);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new Exception(string.Format("Couldn't get the 1D discretisation mesh IDs because of error number: {0}", ierr));
            }
            return meshIds;
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

        public string Get1DMeshDiscretisationName(int meshId)
        {
            var uGridApi = GridApi as IUGridApi1DDiscretisation;
            if(uGridApi == null) throw new Exception(""); // TODO: make an exception
            string meshName;
            var ierr = uGridApi.GetMeshDiscretisationName(meshId, out meshName);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new Exception(string.Format("Couldn't get the mesh discretisation name because of error number {0}", ierr));
            }

            return meshName;
        }

        public int GetNumberOf1DMeshDiscretisationPoints(int meshId)
        {
            var uGridApi = GridApi as IUGridApi1DDiscretisation;
            if (uGridApi == null) return 0;
            var numberOfMeshPoints = uGridApi.GetNumberOf1dDiscretisationPoints(meshId);
            if (numberOfMeshPoints < 0)
            {
                throw new InvalidOperationException(string.Format("Couldn't get the number of 1D mesh discretisation points because of error number {0}", numberOfMeshPoints));
            }
            return numberOfMeshPoints;
        }

        public int Read1DMeshDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset)
        {
            branchIdx = new int[0];
            offset = new double[0];

            var uGridApi = GridApi as IUGridApi1DDiscretisation;
            if (uGridApi == null)
            {
                return GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR; 
            }
            var ierr = uGridApi.Read1dDiscretisationPoints(meshId, out branchIdx, out offset);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't read the 1D mesh discretisation points because of error number {0}", ierr));
            }
            return ierr;
        }

        #endregion

        public void InitializeForLoading(int meshId)
        {
            var uGridApi = GridApi as IUGridApi1DDiscretisation;
            if(uGridApi == null) throw new Exception(string.Format("Communication with netCDF file was unsuccessful. API is not set"));

            GetNumberOf1DMeshDiscretisationPoints(meshId);


           
        }
    }
}