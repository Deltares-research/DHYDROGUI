using DeltaShell.NGHS.IO.Properties;

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
                Resources.UGridNetworkDiscretisation_CreateNetworkDiscretisationInFile_Couldn_t_create_new_network_in_ + filename);
        }

        public void WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset, string[] ids, string[] names)
        {
            DoWithValidGridApi<IUGridNetworkDiscretisationApi>(
                uGridApi => uGridApi.WriteNetworkDiscretisationPoints(branchIdx, offset, ids, names),
                Resources.UGridNetworkDiscretisation_WriteNetworkDiscretisationPoints_Couldn_t_write_the_network_discretisation_points);
        }
        #endregion

        #region Read network discretisation

        public string GetNetworkDiscretisationNameForMeshId(int meshId)
        {
            string meshName;
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(Resources.UGridNetworkDiscretisation_GetNetworkDiscretisationName_Couldn_t_get_the_mesh_discretisation_name);
            var ierr = uGridApi.GetNetworkDiscretisationName(meshId, out meshName);
            ThrowIfError(ierr, Resources.UGridNetworkDiscretisation_GetNetworkDiscretisationName_Couldn_t_get_the_mesh_discretisation_name);

            return meshName;
        }

        public int GetNetworkIdForMeshId(int meshId)
        {
            int networkId;
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(Resources.UGridNetworkDiscretisation_GetNetworkIdForMeshId_Couldn_t_get_the_network_Id_corresponding_to_the_network_discretisation);
            var ierr = uGridApi.GetNetworkIdFromMeshId(meshId, out networkId);
            ThrowIfError(ierr, Resources.UGridNetworkDiscretisation_GetNetworkIdForMeshId_Couldn_t_get_the_network_Id_corresponding_to_the_network_discretisation);

            return networkId;
        }

        public int GetNumberOfNetworkDiscretisations()
        {
            int numberOfNetworkDiscretisations;
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(Resources.UGridNetworkDiscretisation_GetNumberOfNetworkDiscretisations_Couldn_t_get_the_number_of_network_discretisations);
            var ierr = uGridApi.GetNumberOfMeshByType(UGridMeshType.Mesh1D, out numberOfNetworkDiscretisations);
            ThrowIfError(ierr, Resources.UGridNetworkDiscretisation_GetNumberOfNetworkDiscretisations_Couldn_t_get_the_number_of_network_discretisations);

            return numberOfNetworkDiscretisations;
        }

        public int[] GetNetworkDiscretisationIds(int numberOfMeshes)
        {
            int[] meshIds;
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(Resources.UGridNetworkDiscretisation_GetNetworkDiscretisationIds_Couldn_t_get_the_network_discretisation_IDs);
            var ierr = uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh1D, numberOfMeshes, out meshIds);
            ThrowIfError(ierr, Resources.UGridNetworkDiscretisation_GetNetworkDiscretisationIds_Couldn_t_get_the_network_discretisation_IDs);

            return meshIds;
        }

        public int GetNumberOfNetworkDiscretisationPointsForMeshId(int meshId)
        {
            int numberOfDiscretisationPoints;
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(Resources.UGridNetworkDiscretisation_GetNumberOfNetworkDiscretisationPointsForMeshId_Couldn_t_get_the_number_of_network_discretisation_points);
            var ierr = uGridApi.GetNumberOfNetworkDiscretisationPoints(meshId, out numberOfDiscretisationPoints);
            ThrowIfError(ierr, Resources.UGridNetworkDiscretisation_GetNumberOfNetworkDiscretisationPointsForMeshId_Couldn_t_get_the_number_of_network_discretisation_points);

            return numberOfDiscretisationPoints;
        }

        public void ReadNetworkDiscretisationPointsForMeshId(int meshId, out int[] branchIdx, out double[] offset, out string[] ids, out string[] names)
        {
            var uGridApi = GetValidGridApi<IUGridNetworkDiscretisationApi>(Resources.UGridNetworkDiscretisation_ReadNetworkDiscretisationPointsForMeshId_Couldn_t_read_the_network_discretisation_points);
            var ierr = uGridApi.ReadNetworkDiscretisationPoints(meshId, out branchIdx, out offset, out ids, out names);
            ThrowIfError(ierr, Resources.UGridNetworkDiscretisation_ReadNetworkDiscretisationPointsForMeshId_Couldn_t_read_the_network_discretisation_points);
        }
        #endregion
    }
}