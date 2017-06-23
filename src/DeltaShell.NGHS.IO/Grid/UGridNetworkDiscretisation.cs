using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridNetworkDiscretisation : AGrid, IUGridNetworkDiscretisation
    {
        public UGridNetworkDiscretisation(string filename, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : base(filename, mode)
        {
            GridApi = GridApiFactory.CreateNewNetworkDiscretisation();
        }

        #region Write network discretisation

        public void CreateNetworkInFile(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId)
        {
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>("Not able to create the network in " + filename);

            var ierr = uGridApi.CreateNetworkDiscretisation(name, numberOfMeshPoints, numberOfMeshEdges, networkId);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format(
                    "Couldn't create new 1d mesh {0} with number of mesh points {1}, number of mesh edges {2} because of error number {3}",
                    name, numberOfMeshPoints, numberOfMeshEdges, ierr));
            }
        }

        public void WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset)
        {
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>("Not able to write the network discretisation points");
            
            var ierr = uGridApi.WriteNetworkDiscretisationPoints(branchIdx, offset);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't write 1d mesh discretisation points because of error number {0}", ierr));
            }
        }

        #endregion

        #region Read network discretisation

        public string GetNetworkDiscretisationName(int meshId)
        {
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>("Not able to retrieve the network discretisation name");

            string meshName;
            var ierr = uGridApi.GetNetworkDiscretisationName(meshId, out meshName);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new Exception(string.Format("Couldn't get the mesh discretisation name because of error number {0}", ierr));
            }

            return meshName;
        }

        public int GetNumberOfNetworkDiscretisations()
        {
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>("Not able to retrieve the number of network discretisations");

            int numberOfNetworkDiscretisations;
            var ierr = uGridApi.GetNumberOfMeshByType(UGridMeshType.Mesh1D, out numberOfNetworkDiscretisations);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new Exception(string.Format("Couldn't get the number of network discretisations because of error number: {0}", ierr));
            }
            return numberOfNetworkDiscretisations;
        }

        public int[] GetNetworkDiscretisationIds(int numberOfMeshes)
        {
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>("Not able to retrieve the network discretisation id's");

            int[] meshIds;
            var ierr = uGridApi.GetMeshIdsByType(UGridMeshType.Mesh1D, numberOfMeshes, out meshIds);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new Exception(string.Format("Couldn't get the network discretisation IDs because of error number: {0}", ierr));
            }
            return meshIds;
        }

        public int GetNumberOfNetworkDiscretisationPoints(int meshId)
        {
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>("Not able to retrieve the number of network discretisation points");

            var numberOfMeshPoints = uGridApi.GetNumberOfNetworkDiscretisationPoints(meshId);
            if (numberOfMeshPoints < 0)
            {
                throw new InvalidOperationException(string.Format("Couldn't get the number of network discretisation points because of error number {0}", numberOfMeshPoints));
            }
            return numberOfMeshPoints;
        }

        public int ReadNetworkDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset)
        {
            branchIdx = new int[0];
            offset = new double[0];

            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>("Not able to retrieve the network discretisation points");

            var ierr = uGridApi.ReadNetworkDiscretisationPoints(meshId, out branchIdx, out offset);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't read the network discretisation points because of error number {0}", ierr));
            }
            return ierr;
        }

        #endregion
        
        public void InitializeForLoading(int meshId)
        {
            GetNumberOfNetworkDiscretisationPoints(meshId);
        }
    }
}