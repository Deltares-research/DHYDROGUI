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

        public void CreateNetworkDiscretisationInFile(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId)
        {
            DoWithValidGridApi<IUGridNetworkDiscretisationApi>(
                uGridApi => uGridApi.CreateNetworkDiscretisation(name, numberOfMeshPoints, numberOfMeshEdges, networkId),
                "Couldn't create new network in " + filename);
        }

        public void WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset)
        {
            DoWithValidGridApi<IUGridNetworkDiscretisationApi>(
                uGridApi => uGridApi.WriteNetworkDiscretisationPoints(branchIdx, offset),
                "Couldn't write the network discretisation points");
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

        public int GetNetworkIdForMeshId(int meshId)
        {
            int networkId;
            const string errorMessage = "Couldn't get the network Id corresponding to the network discretisation";

            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(errorMessage);
            var ierr = uGridApi.GetNetworkIdFromMeshId(meshId, out networkId);
            ThrowIfError(ierr, errorMessage);

            return networkId;
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

        public int GetNumberOfNetworkDiscretisationPointsForMeshId(int meshId)
        {
            int numberOfDiscretisationPoints;
            const string errorMessage = "Couldn't get the number of network discretisation points";

            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(errorMessage);
            var ierr = uGridApi.GetNumberOfNetworkDiscretisationPoints(meshId, out numberOfDiscretisationPoints);
            ThrowIfError(ierr, errorMessage);

            return numberOfDiscretisationPoints;
        }

        public void ReadNetworkDiscretisationPointsForMeshId(int meshId, out int[] branchIdx, out double[] offset)
        {
            branchIdx = new int[0];
            offset = new double[0];
            const string errorMessage = "Couldn't read the network discretisation points";

            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(errorMessage);
            var ierr = uGridApi.ReadNetworkDiscretisationPoints(meshId, out branchIdx, out offset);
            ThrowIfError(ierr, errorMessage);
        }

        #endregion
    }
}