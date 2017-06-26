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
            string errorMessage = "Couldn't create new network in " + filename;
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(errorMessage);
            var ierr = uGridApi.CreateNetworkDiscretisation(name, numberOfMeshPoints, numberOfMeshEdges, networkId);
            ThrowIfError(ierr, errorMessage);
        }

        public void WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset)
        {
            const string errorMessage = "Couldn't write the network discretisation points";
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(errorMessage);
            var ierr = uGridApi.WriteNetworkDiscretisationPoints(branchIdx, offset);
            ThrowIfError(ierr, errorMessage);
        }

        #endregion

        #region Read network discretisation

        public string GetNetworkDiscretisationName(int meshId)
        {
            string meshName;

            const string errorMessage = "Couldn't get the mesh discretisation name";
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(errorMessage);
            var ierr = uGridApi.GetNetworkDiscretisationName(meshId, out meshName);
            ThrowIfError(ierr, errorMessage);

            return meshName;
        }

        // DELFT3DFM-905
        // Have written a failing test for this method.
        // When the API-call for getting the network ID is in place:
        // *    develop this method
        // *    use this method in UGridToNetworkAdapter.LoadNetworkDiscretisationDataModel && UGridToNetworkAdapter.SaveNetworkDiscretisation && UGridToNetworkAdapter.LoadNetwork
        // *    erase these comments
        public int GetNetworkId(int meshId)
        {
            throw new NotImplementedException();
        }

        public int GetNumberOfNetworkDiscretisations()
        {
            int numberOfNetworkDiscretisations;

            const string errorMessage = "Couldn't get the number of network discretisations";
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(errorMessage);
            var ierr = uGridApi.GetNumberOfMeshByType(UGridMeshType.Mesh1D, out numberOfNetworkDiscretisations);
            ThrowIfError(ierr, errorMessage);

            return numberOfNetworkDiscretisations;
        }

        public int[] GetNetworkDiscretisationIds(int numberOfMeshes)
        {
            int[] meshIds;

            const string errorMessage = "Couldn't get the network discretisation IDs";
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(errorMessage);
            var ierr = uGridApi.GetMeshIdsByType(UGridMeshType.Mesh1D, numberOfMeshes, out meshIds);
            ThrowIfError(ierr, errorMessage);

            return meshIds;
        }

        public int GetNumberOfNetworkDiscretisationPoints(int meshId)
        {
            const string errorMessage = "Couldn't get the number of network discretisation points";
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(errorMessage);
            int numberOfDiscretisationPoints;
            var ierr = uGridApi.GetNumberOfNetworkDiscretisationPoints(meshId, out numberOfDiscretisationPoints);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format(errorMessage + " because of error number {0}", ierr));
            }
            return GridApiDataSet.GridConstants.IONC_NOERR;
        }

        public int ReadNetworkDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset)
        {
            branchIdx = new int[0];
            offset = new double[0];

            const string errorMessage = "Couldn't read the network discretisation points";
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(errorMessage);
            var ierr = uGridApi.ReadNetworkDiscretisationPoints(meshId, out branchIdx, out offset);
            ThrowIfError(ierr, errorMessage);

            return ierr;
        }

        #endregion
        
        public void InitializeForLoading(int meshId)
        {
            GetNumberOfNetworkDiscretisationPoints(meshId);
        }
    }
}