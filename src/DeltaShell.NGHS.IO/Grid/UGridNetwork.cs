using System;

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
            networkId = -1;
            if (!IsInitialized()) Initialize();
            var uGridNetworkApi = GridApi as IUGridNetworkApi;
            if (uGridNetworkApi == null) return;

            var ierr = uGridNetworkApi.CreateNetwork(name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints, out networkId);
            if (ierr == GridApiDataSet.GridConstants.IONC_NOERR) return;

            throw new InvalidOperationException(
                string.Format(
                    "Couldn't create new 1d network {0} with number of nodes {1}, number of branches {2}, number of geometry points {3} because of error number {4}",
                    name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints, ierr));
        }

        public void WriteNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            const string errorMessage = "Couldn't write 1d network nodes";
            DoWithValidUGridNetworkApi(uGridNetworkApi => uGridNetworkApi.WriteNetworkNodes(nodesX, nodesY, nodesids, nodeslongNames), errorMessage);
        }
        
        public void WriteNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames)
        {
            const string errorMessage = "Couldn't write 1d network branches";
            DoWithValidUGridNetworkApi(uGridNetworkApi => uGridNetworkApi.WriteNetworkBranches(sourceNodeId, targetNodeId, branchLengths, nbranchgeometrypoints, branchIds, branchLongnames), errorMessage);
        }

        public void WriteNetworkGeometry(double[] geopointsX, double[] geopointsY)
        {
            const string errorMessage = "Couldn't write 1d network geometry";
            DoWithValidUGridNetworkApi(uGridNetworkApi => uGridNetworkApi.WriteNetworkGeometry(geopointsX, geopointsY), errorMessage);
        }

        #endregion

        #region Read network

        public string GetNetworkName(int networkId)
        {
            string networkName;
            const string errorMessage = "Couldn't obtain the network name";
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(errorMessage);
            var ierr = uGridNetworkApi.GetNetworkName(networkId, out networkName);
            ThrowIfError(ierr, errorMessage);

            return networkName;
        }

        public void InitializeForLoading(int networkId)
        {
            GetNumberOfNetworkNodes(networkId);
            GetNumberOfNetworkBranches(networkId);
            GetNumberOfNetworkGeometryPoints(networkId);
        }

        public int GetNumberOfNetworkNodes(int networkId)
        {
            int numberOfNetworkNodes;
            const string errorMessage = "Couldn't get number of network nodes";
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(errorMessage);
            var ierr = uGridNetworkApi.GetNumberOfNetworkNodes(networkId, out numberOfNetworkNodes);
            ThrowIfError(ierr, errorMessage);

            return numberOfNetworkNodes;
        }

        public int GetNumberOfNetworkBranches(int networkId)
        {
            int numberOfNetworkBranches;
            const string errorMessage = "Couldn't get the number of network branches";
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(errorMessage);
            var ierr = uGridNetworkApi.GetNumberOfNetworkBranches(networkId, out numberOfNetworkBranches);
            ThrowIfError(ierr, errorMessage);

            return numberOfNetworkBranches;
        }

        public int GetNumberOfNetworkGeometryPoints(int networkId)
        {
            int numberOfNetworkGeometryPoints;
            const string errorMessage = "Couldn't get the number of network geometry points";
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(errorMessage);
            var ierr = uGridNetworkApi.GetNumberOfNetworkGeometryPoints(networkId, out numberOfNetworkGeometryPoints);
            ThrowIfError(ierr, errorMessage);

            return numberOfNetworkGeometryPoints;
        }

        public void ReadNetworkNodes(int networkId, out double[] nodesX, out double[] nodesY, out string[] nodesIds, out string[] nodesLongnames)
        {
            const string errorMessage = "Couldn't read network nodes";
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(errorMessage);
            var ierr = uGridNetworkApi.ReadNetworkNodes(networkId, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            ThrowIfError(ierr, errorMessage);
        }

        public void ReadNetworkBranches(int networkId, out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames)
        {
            const string errorMessage = "Couldn't read network branches";
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(errorMessage);
            var ierr = uGridNetworkApi.ReadNetworkBranches(networkId, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            ThrowIfError(ierr, errorMessage);
        }

        public void ReadNetworkGeometry(int networkId, out double[] geopointsX, out double[] geopointsY)
        {
            const string errorMessage = "Couldn't read network geometry";
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(errorMessage);
            var ierr = uGridNetworkApi.ReadNetworkGeometry(networkId, out geopointsX, out geopointsY);
            ThrowIfError(ierr, errorMessage);
        }

        #endregion

        public override bool IsInitialized()
        {
            var uGridNetworkApi = GridApi as IUGridNetworkApi;
            if (uGridNetworkApi == null) return false;
            return base.IsInitialized();
        }

        private T GetFromValidUGridApi<T>(Func<IUGridNetworkApi, T> function, T defaultValue, string errorMessage)
        {
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(errorMessage);
            return uGridNetworkApi != null ? function(uGridNetworkApi) : defaultValue;
        }

        private void DoWithValidUGridNetworkApi(Func<IUGridNetworkApi, int> function, string errorMessage)
        {
            var uGridNetworkApi = GetValidGridApi<IUGridNetworkApi>(errorMessage);
            var ierr = function(uGridNetworkApi);
            ThrowIfError(ierr, errorMessage);
        }
    }
}