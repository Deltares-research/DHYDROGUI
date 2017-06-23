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
            var uGridNetworkApi = GridApi as IUGridNetworkApi;
            if (uGridNetworkApi == null) return string.Empty;

            string networkName;
            var ierr = uGridNetworkApi.GetNetworkName(networkId, out networkName);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't obtain the network name because of error: {0}", ierr));
            }

            return networkName;
        }

        public void InitializeForLoading(int networkId)
        {
            var uGridNetworkApi = GridApi as IUGridNetworkApi;
            if(uGridNetworkApi == null) throw new InvalidOperationException("Communication with netCDF file was unsuccessful. API is not set");
            
            GetNumberOfNetworkNodes(networkId);
            GetNumberOfNetworkBranches(networkId);
            GetNumberOfNetworkGeometryPoints(networkId);
        }

        public int GetNumberOfNetworkNodes(int networkId)
        {
            int numberOfNetworkNodes;
            IUGridNetworkApi uGridNetworkApi = GetValidIUGridNetworkApi();
            var ierr = uGridNetworkApi.GetNumberOfNetworkNodes(networkId, out numberOfNetworkNodes);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't get number of network nodes because of error number {0}", ierr));
            }
            return numberOfNetworkNodes;
        }

        public int GetNumberOfNetworkBranches(int networkId)
        {
            int numberOfNetworkBranches;
            IUGridNetworkApi uGridNetworkApi = GetValidIUGridNetworkApi();
            var ierr = uGridNetworkApi.GetNumberOfNetworkBranches(networkId, out numberOfNetworkBranches);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't get the number of network branches because of error number {0}", ierr));
            }
            return numberOfNetworkBranches;
        }

        public int GetNumberOfNetworkGeometryPoints(int networkId)
        {
            int numberOfNetworkGeometryPoints;
            IUGridNetworkApi uGridNetworkApi = GetValidIUGridNetworkApi();
            var ierr = uGridNetworkApi.GetNumberOfNetworkGeometryPoints(networkId, out numberOfNetworkGeometryPoints);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't get the number of network geometry points because of error number {0}", ierr));
            }
            return numberOfNetworkGeometryPoints;
        }

        public void ReadNetworkNodes(int networkId, out double[] nodesX, out double[] nodesY, out string[] nodesIds, out string[] nodesLongnames)
        {
            IUGridNetworkApi uGridNetworkApi = GetValidIUGridNetworkApi();
            var ierr = uGridNetworkApi.ReadNetworkNodes(networkId, out nodesX, out nodesY, out nodesIds, out nodesLongnames);

            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(
                    string.Format("Couldn't read network nodes because of error number {0}", ierr));
            }
        }

        public void ReadNetworkBranches(int networkId, out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames)
        {
            IUGridNetworkApi uGridNetworkApi = GetValidIUGridNetworkApi();
            var ierr = uGridNetworkApi.ReadNetworkBranches(networkId, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);

            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't read network branches because of error number {0}", ierr));
            }

        }

        public void ReadNetworkGeometry(int networkId, out double[] geopointsX, out double[] geopointsY)
        {
            var uGridNetworkApi = GetValidIUGridNetworkApi();
            var ierr = uGridNetworkApi.ReadNetworkGeometry(networkId, out geopointsX, out geopointsY);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(
                    string.Format("Couldn't read 1d network geometry because of error number {0}", ierr));
            }
        }

        #endregion

        public override bool IsInitialized()
        {
            var uGridNetworkApi = GridApi as IUGridNetworkApi;
            if (uGridNetworkApi == null) return false;
            return base.IsInitialized();
        }

        private T GetFromValidUGridApi<T>(Func<IUGridNetworkApi, T> function, T defaultValue)
        {
            var uGridNetworkApi = GetValidIUGridNetworkApi();
            return uGridNetworkApi != null ? function(uGridNetworkApi) : defaultValue;
        }

        private void DoWithValidUGridNetworkApi(Func<IUGridNetworkApi, int> function, string errorMessage)
        {
            var uGridNetworkApi = GetValidIUGridNetworkApi();
            var ierr = function(uGridNetworkApi);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format(errorMessage + " because of error number {0}", ierr));
            }
        }

        private IUGridNetworkApi GetValidIUGridNetworkApi()
        {
            if (!IsInitialized()) Initialize();
            var uGridNetworkApi = GridApi as IUGridNetworkApi;

            var isValid = uGridNetworkApi != null && IsInitialized() && IsValid();
            if (!isValid)
            {
                throw new InvalidOperationException("Communication with netCDF file was unsuccessful, API is not set");
            }
            return uGridNetworkApi;
        }
    }
}