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

        #region Write 1D network

        public void Create1DGridInFile(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int networkId)
        {
            networkId = -1;
            if (!IsInitialized()) Initialize();
            var uGridApi1D = GridApi as IUGridApi1D;
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
            if (!IsInitialized()) Initialize();
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {
                var ierr = uGridApi1D.Write1DNetworkNodes(nodesX, nodesY, nodesids, nodeslongNames);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new InvalidOperationException(string.Format("Couldn't write 1d network nodes because of error number {0}", ierr));
                }
            }
        }

        public void Write1DNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames)
        {
            if (!IsInitialized()) Initialize();
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {
                var ierr = uGridApi1D.Write1DNetworkBranches(sourceNodeId, targetNodeId, branchLengths, nbranchgeometrypoints, branchIds, branchLongnames);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new InvalidOperationException(string.Format("Couldn't write 1d network branches because of error number {0}", ierr));
                }
            }
            // TODO: Throw when uGridApi1D == null ?
        }

        public void Write1DNetworkGeometry(double[] geopointsX, double[] geopointsY)
        {
            if (!IsInitialized()) Initialize();
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {
                var ierr = uGridApi1D.Write1DNetworkGeometry(geopointsX, geopointsY);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new InvalidOperationException(string.Format("Couldn't write 1d network geometry because of error number {0}", ierr));
                }
            }
        }

        #endregion

        #region Read 1D network

        public string GetNetworkName()
        {
            // TODO: Not working yet.
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {
                var name = uGridApi1D.GetNetworkName();
            }


            return string.Empty;
        }

        public int GetNumberOfNetworkNodes()
        {
            if (!IsInitialized()) Initialize();
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D == null)
            {
                throw new InvalidOperationException(string.Format("Couldn't get the 1D number of network nodes because the Api is null"));
            }
            var numberOfNetworkNodes = -1;
            var ierr = uGridApi1D.GetNumberOfNetworkNodes(out numberOfNetworkNodes);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't get 1D number of network nodes because of error number {0}", ierr));
            }
            return numberOfNetworkNodes;
        }

        public int GetNumberOfNetworkBranches()
        {
            if (!IsInitialized()) Initialize();
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D == null)
            {
                throw new InvalidOperationException(string.Format("Couldn't get the 1D number of network branches because the Api is null"));
            }
            var numberOfNetworkBranches = -1;
            var ierr = uGridApi1D.GetNumberOfNetworkBranches(out numberOfNetworkBranches);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't get the 1D number of network branches because of error number {0}", ierr));
            }
            return numberOfNetworkBranches;
        }

        public int GetNumberOfNetworkGeometryPoints()
        {
            if (!IsInitialized()) Initialize();
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D == null)
            {
                throw new InvalidOperationException(string.Format("Couldn't get the 1D number of network geometry points because the Api is null"));
            }
            var numberOfNetworkGeometryPoints = -1;
            var ierr = uGridApi1D.GetNumberOfNetworkGeometryPoints(out numberOfNetworkGeometryPoints);
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't get the 1D number of network geometry points because of error number {0}", ierr));
            }
            return numberOfNetworkGeometryPoints;
        }

        public void Read1DNetworkNodes(out double[] nodesX, out double[] nodesY, out string[] nodesIds, out string[] nodesLongnames)
        {
            if (!IsInitialized()) Initialize();
            nodesY = new double[0];
            nodesX = new double[0];
            nodesIds = new string[0];
            nodesLongnames = new string[0];

            if (!IsInitialized())
            {
                throw new InvalidOperationException(string.Format("Couldn't read the 1D network nodes because the Api is not initialized")); 
            }
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D == null)
            {
                throw new InvalidOperationException(string.Format("Couldn't read the 1D network nodes because the Api is null")); ;
            }
            var ierr = uGridApi1D.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames);

            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(
                    string.Format("Couldn't read 1D network nodes because of error number {0}", ierr));
            }
        }

        public void Read1DNetworkBranches(out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames)
        {
            sourceNodes = new int[0];
            targetNodes = new int[0];
            branchLengths = new double[0];
            branchGeoPoints = new int[0];
            branchIds = new string[0];
            branchLongnames = new string[0];

            if (!IsInitialized())
            {
                throw new InvalidOperationException(string.Format("Couldn't read the 1D network branches because the Api is not initialized"));
            }
            IUGridApi1D uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D == null)
            {
                throw new InvalidOperationException(string.Format("Couldn't read the 1D network branches because the Api is null")); ;
            }
            var ierr = uGridApi1D.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);

            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new InvalidOperationException(string.Format("Couldn't read 1D network branches because of error number {0}", ierr));
            }

        }

        public void Read1DNetworkGeometry(out double[] geopointsX, out double[] geopointsY)
        {
            geopointsX = new double[0];
            geopointsY = new double[0];

            if (!IsInitialized())
            {
                throw new InvalidOperationException(string.Format("Couldn't read the 1D network geometry points because the Api is not initialized"));
            }

            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D == null)
            {
                throw new InvalidOperationException(string.Format("Couldn't read the 1D network branches because the Api is null")); ;
            }
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
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D == null) return false;
            return base.IsInitialized() && uGridApi1D.NetworkReady;
        }
    }
}