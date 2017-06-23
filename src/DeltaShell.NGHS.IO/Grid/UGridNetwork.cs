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

        #region Write 1D network

        public void CreateNetworkInFile(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int networkId)
        {
            networkId = -1;
            if (!IsInitialized()) Initialize();
            var uGridApi1D = GridApi as IUGridApiNetwork;
            if (uGridApi1D == null) return;

            var ierr = uGridApi1D.CreateNetwork(name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints, out networkId);
            if (ierr == GridApiDataSet.GridConstants.IONC_NOERR) return;

            throw new InvalidOperationException(
                string.Format(
                    "Couldn't create new 1d network {0} with number of nodes {1}, number of branches {2}, number of geometry points {3} because of error number {4}",
                    name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints, ierr));
        }

        public void WriteNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            const string errorMessage = "Couldn't write 1d network nodes";
            DoWithValidUGridApi1D(uGridApi1D => uGridApi1D.WriteNetworkNodes(nodesX, nodesY, nodesids, nodeslongNames), errorMessage);
        }
        
        public void WriteNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames)
        {
            const string errorMessage = "Couldn't write 1d network branches";
            DoWithValidUGridApi1D(uGridApi1D => uGridApi1D.WriteNetworkBranches(sourceNodeId, targetNodeId, branchLengths, nbranchgeometrypoints, branchIds, branchLongnames), errorMessage);
        }

        public void WriteNetworkGeometry(double[] geopointsX, double[] geopointsY)
        {
            const string errorMessage = "Couldn't write 1d network geometry";
            DoWithValidUGridApi1D(uGridApi1D => uGridApi1D.WriteNetworkGeometry(geopointsX, geopointsY), errorMessage);
        }

        #endregion

        #region Read 1D network

        public string GetNetworkName(int networkId)
        {
            var uGridApi1D = GridApi as IUGridApiNetwork;
            if (uGridApi1D == null) return string.Empty;

            string networkName;
            var ierr = uGridApi1D.GetNetworkName(networkId, out networkName);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't obtain the network name because of error: {0}", ierr));
            }

            return networkName;
        }

        public void InitializeForLoading(int networkId)
        {
            var uGridApi1D = GridApi as IUGridApiNetwork;
            if(uGridApi1D == null) throw new InvalidOperationException("Communication with netCDF file was unsuccessful. API is not set");
            
            GetNumberOfNetworkNodes(networkId);
            GetNumberOfNetworkBranches(networkId);
            GetNumberOfNetworkGeometryPoints(networkId);
        }

        public int GetNumberOfNetworkNodes(int networkId)
        {
            int numberOfNetworkNodes;
            IUGridApiNetwork uGridApi1D = GetValidIUGridApi1D();
            var ierr = uGridApi1D.GetNumberOfNetworkNodes(networkId, out numberOfNetworkNodes);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't get 1D number of network nodes because of error number {0}", ierr));
            }
            return numberOfNetworkNodes;
        }

        public int GetNumberOfNetworkBranches(int networkId)
        {
            int numberOfNetworkBranches;
            IUGridApiNetwork uGridApi1D = GetValidIUGridApi1D();
            var ierr = uGridApi1D.GetNumberOfNetworkBranches(networkId, out numberOfNetworkBranches);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't get the 1D number of network branches because of error number {0}", ierr));
            }
            return numberOfNetworkBranches;
        }

        public int GetNumberOfNetworkGeometryPoints(int networkId)
        {
            int numberOfNetworkGeometryPoints;
            IUGridApiNetwork uGridApi1D = GetValidIUGridApi1D();
            var ierr = uGridApi1D.GetNumberOfNetworkGeometryPoints(networkId, out numberOfNetworkGeometryPoints);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't get the 1D number of network geometry points because of error number {0}", ierr));
            }
            return numberOfNetworkGeometryPoints;
        }

        public void ReadNetworkNodes(int networkId, out double[] nodesX, out double[] nodesY, out string[] nodesIds, out string[] nodesLongnames)
        {
            IUGridApiNetwork uGridApi1D = GetValidIUGridApi1D();
            var ierr = uGridApi1D.ReadNetworkNodes(networkId, out nodesX, out nodesY, out nodesIds, out nodesLongnames);

            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(
                    string.Format("Couldn't read 1D network nodes because of error number {0}", ierr));
            }
        }

        public void ReadNetworkBranches(int networkId, out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames)
        {
            IUGridApiNetwork uGridApi1D = GetValidIUGridApi1D();
            var ierr = uGridApi1D.ReadNetworkBranches(networkId, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);

            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't read 1D network branches because of error number {0}", ierr));
            }

        }

        public void ReadNetworkGeometry(int networkId, out double[] geopointsX, out double[] geopointsY)
        {
            var uGridApi1D = GetValidIUGridApi1D();
            var ierr = uGridApi1D.ReadNetworkGeometry(networkId, out geopointsX, out geopointsY);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(
                    string.Format("Couldn't read 1d network geometry because of error number {0}", ierr));
            }
        }

        #endregion

        public override bool IsInitialized()
        {
            var uGridApi1D = GridApi as IUGridApiNetwork;
            if (uGridApi1D == null) return false;
            return base.IsInitialized();
        }

        private T GetFromValidUGridApi<T>(Func<IUGridApiNetwork, T> function, T defaultValue)
        {
            var uGridApi1D = GetValidIUGridApi1D();
            return uGridApi1D != null ? function(uGridApi1D) : defaultValue;
        }

        private void DoWithValidUGridApi1D(Func<IUGridApiNetwork, int> function, string errorMessage)
        {
            var uGridApi1D = GetValidIUGridApi1D();
            var ierr = function(uGridApi1D);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format(errorMessage + " because of error number {0}", ierr));
            }
        }

        private IUGridApiNetwork GetValidIUGridApi1D()
        {
            if (!IsInitialized()) Initialize();
            var uGridApi1D = GridApi as IUGridApiNetwork;

            var isValid = uGridApi1D != null && IsInitialized() && IsValid();
            if (!isValid)
            {
                throw new InvalidOperationException("Communication with netCDF file was unsuccessful, API is not set");
            }
            return uGridApi1D;
        }
    }
}