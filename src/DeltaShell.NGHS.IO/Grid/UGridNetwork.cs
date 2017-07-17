using System;
using DeltaShell.NGHS.IO.Properties;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGridNetwork : AGrid, IUGridNetwork
    {
        public UGridNetwork(string file, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : base(file, mode)
        {
            GridApi = GridApiFactory.CreateNewNetwork();
        }

        public UGridNetwork(string file, UGridGlobalMetaData globalMetaData, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : base(file, globalMetaData, mode)
        {
            GridApi = GridApiFactory.CreateNewNetwork();
        }

        #region Write network

        public void CreateNetworkInFile(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int networkId)
        {
            string errorMessage = 
                string.Format(Resources.UGridNetwork_CreateNetworkInFile_Couldn_t_create_new_network__0__with_number_of_nodes__1___number_of_branches__2___number_of_geometry_points__3_,
                name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints);
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(errorMessage);
            var ierr = uGridNetworkApi.CreateNetwork(name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints, out networkId);
            ThrowIfError(ierr, errorMessage);
        }

        public void WriteNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            DoWithValidGridApi<IUGridNetworkApi>(uGridNetworkApi => uGridNetworkApi.WriteNetworkNodes(nodesX, nodesY, nodesids, nodeslongNames), 
                Resources.UGridNetwork_WriteNetworkNodes_Couldn_t_write_network_nodes);
        }
        
        public void WriteNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames, int[] branchOrderNumbers)
        {
            DoWithValidGridApi<IUGridNetworkApi>(uGridNetworkApi => uGridNetworkApi.WriteNetworkBranches(sourceNodeId, targetNodeId, branchLengths, nbranchgeometrypoints, branchIds, branchLongnames, branchOrderNumbers),
                Resources.UGridNetwork_WriteNetworkBranches_Couldn_t_write_network_branches);
        }

        public void WriteNetworkGeometry(double[] geopointsX, double[] geopointsY)
        {
            DoWithValidGridApi<IUGridNetworkApi>(uGridNetworkApi => uGridNetworkApi.WriteNetworkGeometry(geopointsX, geopointsY), 
                Resources.UGridNetwork_WriteNetworkGeometry_Couldn_t_write_network_geometry);
        }

        #endregion

        #region Read network

        public string GetNetworkName(int networkId)
        {
            string networkName;
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(Resources.UGridNetwork_GetNetworkName_Couldn_t_obtain_the_network_name);
            var ierr = uGridNetworkApi.GetNetworkName(networkId, out networkName);
            ThrowIfError(ierr, Resources.UGridNetwork_GetNetworkName_Couldn_t_obtain_the_network_name);

            return networkName;
        }

        public int GetNumberOfNetworkNodes(int networkId)
        {
            int numberOfNetworkNodes;
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(Resources.UGridNetwork_GetNumberOfNetworkNodes_Couldn_t_get_number_of_network_nodes);
            var ierr = uGridNetworkApi.GetNumberOfNetworkNodes(networkId, out numberOfNetworkNodes);
            ThrowIfError(ierr, Resources.UGridNetwork_GetNumberOfNetworkNodes_Couldn_t_get_number_of_network_nodes);

            return numberOfNetworkNodes;
        }

        public int GetNumberOfNetworkBranches(int networkId)
        {
            int numberOfNetworkBranches;
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(Resources.UGridNetwork_GetNumberOfNetworkBranches_Couldn_t_get_the_number_of_network_branches);
            var ierr = uGridNetworkApi.GetNumberOfNetworkBranches(networkId, out numberOfNetworkBranches);
            ThrowIfError(ierr, Resources.UGridNetwork_GetNumberOfNetworkBranches_Couldn_t_get_the_number_of_network_branches);

            return numberOfNetworkBranches;
        }

        public int GetNumberOfNetworkGeometryPoints(int networkId)
        {
            int numberOfNetworkGeometryPoints;
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(Resources.UGridNetwork_GetNumberOfNetworkGeometryPoints_Couldn_t_get_the_number_of_network_geometry_points);
            var ierr = uGridNetworkApi.GetNumberOfNetworkGeometryPoints(networkId, out numberOfNetworkGeometryPoints);
            ThrowIfError(ierr, Resources.UGridNetwork_GetNumberOfNetworkGeometryPoints_Couldn_t_get_the_number_of_network_geometry_points);

            return numberOfNetworkGeometryPoints;
        }

        public void ReadNetworkNodes(int networkId, out double[] nodesX, out double[] nodesY, out string[] nodesIds, out string[] nodesLongnames)
        {
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(Resources.UGridNetwork_ReadNetworkNodes_Couldn_t_read_network_nodes);
            var ierr = uGridNetworkApi.ReadNetworkNodes(networkId, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            ThrowIfError(ierr, Resources.UGridNetwork_ReadNetworkNodes_Couldn_t_read_network_nodes);
        }

        public void ReadNetworkBranches(int networkId, out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames, out int[] branchOrderNumbers)
        {
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(Resources.UGridNetwork_ReadNetworkBranches_Couldn_t_read_network_branches);
            var ierr = uGridNetworkApi.ReadNetworkBranches(networkId, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers);
            ThrowIfError(ierr, Resources.UGridNetwork_ReadNetworkBranches_Couldn_t_read_network_branches);
        }

        public void ReadNetworkGeometry(int networkId, out double[] geopointsX, out double[] geopointsY)
        {
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(Resources.UGridNetwork_ReadNetworkGeometry_Couldn_t_read_network_geometry);
            var ierr = uGridNetworkApi.ReadNetworkGeometry(networkId, out geopointsX, out geopointsY);
            ThrowIfError(ierr, Resources.UGridNetwork_ReadNetworkGeometry_Couldn_t_read_network_geometry);
        }

        #endregion
    }
}