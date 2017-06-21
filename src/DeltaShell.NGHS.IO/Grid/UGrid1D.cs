using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGrid1D : AGrid, IUGrid1D
    {
        public UGrid1D(string file, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_write) : base(file, mode)
        {
            GridApi = GridApiFactory.CreateNew1D();
        }

        public UGrid1D(string file, UGridGlobalMetaData globalMetaData, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_write) : base(file, globalMetaData, mode)
        {
            GridApi = GridApiFactory.CreateNew1D();
        }

        public bool IsUGridFormat()
        {
            return GridApi.GetConvention() == GridApiDataSet.DataSetConventions.IONC_CONV_UGRID;
        }

        #region Write 1D network

        public void Create1DNetworkInFile(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int networkId)
        {
            networkId = -1;
            if (!IsInitialized()) Initialize();
            var uGridApi1D = GridApi as IUGridApi1DNetwork;
            if (uGridApi1D == null) return;

            var ierr = uGridApi1D.Create1DNetwork(name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints, out networkId);
            if (ierr == GridApiDataSet.GridConstants.IONC_NOERR) return;

            throw new InvalidOperationException(
                string.Format(
                    "Couldn't create new 1d network {0} with number of nodes {1}, number of branches {2}, number of geometry points {3} because of error number {4}",
                    name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints, ierr));
        }

        public void Write1DNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            const string errorMessage = "Couldn't write 1d network nodes";
            DoWithValidUGridApi1D(uGridApi1D => uGridApi1D.Write1DNetworkNodes(nodesX, nodesY, nodesids, nodeslongNames), errorMessage);
        }
        
        public void Write1DNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames)
        {
            const string errorMessage = "Couldn't write 1d network branches";
            DoWithValidUGridApi1D(uGridApi1D => uGridApi1D.Write1DNetworkBranches(sourceNodeId, targetNodeId, branchLengths, nbranchgeometrypoints, branchIds, branchLongnames), errorMessage);
        }

        public void Write1DNetworkGeometry(double[] geopointsX, double[] geopointsY)
        {
            const string errorMessage = "Couldn't write 1d network geometry";
            DoWithValidUGridApi1D(uGridApi1D => uGridApi1D.Write1DNetworkGeometry(geopointsX, geopointsY), errorMessage);
        }

        #endregion

        #region Read 1D network

        public string GetNetworkName(int networkId)
        {
            var uGridApi1D = GridApi as IUGridApi1DNetwork;
            if (uGridApi1D == null) return string.Empty;

            string networkName;
            var ierr = uGridApi1D.GetNetworkName(networkId, out networkName);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't obtain the network name because of error: {0}", ierr));
            }

            return networkName;
        }

        public void InitializeForLoading(int nwid)
        {
            var uGridApi1D = GridApi as IUGridApi1DNetwork;
            if(uGridApi1D == null) throw new InvalidOperationException("Communication with netCDF file was unsuccessful, API is not set");

            uGridApi1D.SetNetworkId(nwid);

            GetNumberOfNetworkNodes(nwid);
            GetNumberOfNetworkBranches(nwid);
            GetNumberOfNetworkGeometryPoints(nwid);

        }

        public int GetNumberOfNetworkNodes(int networkId)
        {
            int numberOfNetworkNodes;
            IUGridApi1DNetwork uGridApi1D = GetValidIUGridApi1D();
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
            IUGridApi1DNetwork uGridApi1D = GetValidIUGridApi1D();
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
            IUGridApi1DNetwork uGridApi1D = GetValidIUGridApi1D();
            var ierr = uGridApi1D.GetNumberOfNetworkGeometryPoints(networkId, out numberOfNetworkGeometryPoints);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't get the 1D number of network geometry points because of error number {0}", ierr));
            }
            return numberOfNetworkGeometryPoints;
        }

        public void Read1DNetworkNodes(out double[] nodesX, out double[] nodesY, out string[] nodesIds, out string[] nodesLongnames)
        {
            IUGridApi1DNetwork uGridApi1D = GetValidIUGridApi1D();
            var ierr = uGridApi1D.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames);

            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(
                    string.Format("Couldn't read 1D network nodes because of error number {0}", ierr));
            }
        }

        public void Read1DNetworkBranches(out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames)
        {
            IUGridApi1DNetwork uGridApi1D = GetValidIUGridApi1D();
            var ierr = uGridApi1D.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);

            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't read 1D network branches because of error number {0}", ierr));
            }

        }

        public void Read1DNetworkGeometry(out double[] geopointsX, out double[] geopointsY)
        {
            var uGridApi1D = GetValidIUGridApi1D();
            var ierr = uGridApi1D.Read1DNetworkGeometry(out geopointsX, out geopointsY);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(
                    string.Format("Couldn't read 1d network geometry because of error number {0}", ierr));
            }

        }

        #endregion

        public override bool IsInitialized()
        {
            var uGridApi1D = GridApi as IUGridApi1DNetwork;
            if (uGridApi1D == null) return false;
            return base.IsInitialized() && uGridApi1D.NetworkReady;
        }

        private T GetFromValidUGridApi<T>(Func<IUGridApi1DNetwork, T> function, T defaultValue)
        {
            var uGridApi1D = GetValidIUGridApi1D();
            return uGridApi1D != null ? function(uGridApi1D) : defaultValue;
        }

        private void DoWithValidUGridApi1D(Func<IUGridApi1DNetwork, int> function, string errorMessage)
        {
            var uGridApi1D = GetValidIUGridApi1D();
            var ierr = function(uGridApi1D);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format(errorMessage + " because of error number {0}", ierr));
            }
        }

        private IUGridApi1DNetwork GetValidIUGridApi1D()
        {
            if (!IsInitialized()) Initialize();
            var uGridApi1D = GridApi as IUGridApi1DNetwork;

            var isValid = uGridApi1D != null && IsInitialized() && IsValid();
            if (!isValid)
            {
                throw new InvalidOperationException("Communication with netCDF file was unsuccessful, API is not set");
            }
            return uGridApi1D;
        }
    }
}