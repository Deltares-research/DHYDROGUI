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

        public void Create1DGridInFile(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints)
        {
            if (!IsInitialized()) Initialize();
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {
                var ierr = uGridApi1D.Create1DNetwork(name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Couldn't create new 1d network {0} with number of nodes {1}, number of branches {2}, number of geometry points {3} because of error number {4}",
                            name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints, ierr));
                }
            }
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
            // TODO: Thrown when uGridApi1D == null ?
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
        
        public int GetNumberOfNetworkNodes()
        {
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {
                var result = uGridApi1D.GetNumberOfNetworkNodes();
                if (result < 0)
                {
                    throw new InvalidOperationException(string.Format("Couldn't get 1D number of network nodes because of error number {0}", result));
                }
                return result;
            }
            return -1;
        }

        public int GetNumberOfNetworkBranches()
        {
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {
                var result = uGridApi1D.GetNumberOfNetworkBranches();
                if (result < 0)
                {
                    throw new InvalidOperationException(string.Format("Couldn't get the 1D number of network branches because of error number {0}", result));
                }
                return result;
            }
            return -1;
        }

        public int GetNumberOfNetworkGeometryPoints()
        {
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {
                var result = uGridApi1D.GetNumberOfNetworkGeometryPoints();
                if (result < 0)
                {
                    throw new InvalidOperationException(string.Format("Couldn't get the 1D number of network geometry points because of error number {0}", result));
                }
                return result;
            }
            return -1;
        }

        public int Read1DNetworkNodes(out double[] nodesX, out double[] nodesY, out string[] nodesIds, out string[] nodesLongnames)
        {
            nodesY = new double[0];
            nodesX = new double[0];
            nodesIds = new string[0];
            nodesLongnames = new string[0];

            if (!IsInitialized())
            {
                throw new InvalidOperationException(); // TODO: Useful message
            }
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {
                var ierr = uGridApi1D.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames);

                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new InvalidOperationException(
                        string.Format("Couldn't read 1d network nodes because of error number {0}", ierr));
                }
                return ierr;
            }
            return -1;
        }

        public int Read1DNetworkBranches(out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames)
        {
            sourceNodes = new int[0];
            targetNodes = new int[0];
            branchLengths = new double[0];
            branchGeoPoints = new int[0];
            branchIds = new string[0];
            branchLongnames = new string[0];

            if (!IsInitialized())
            {
                throw new InvalidOperationException(); // TODO: Useful message
            }
            IUGridApi1D uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {

                var ierr = uGridApi1D.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);

                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new InvalidOperationException(string.Format("Couldn't read 1d network branches because of error number {0}", ierr));
                }
                return ierr;
            }
            return -1;
        }

        public int Read1DNetworkGeometry(out double[] geopointsX, out double[] geopointsY)
        {
            geopointsX = new double[0];
            geopointsY = new double[0];

            if (!IsInitialized())
            {
                throw new InvalidOperationException(); // TODO: Useful message
            }

            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {
                var ierr = uGridApi1D.Read1DNetworkGeometry(out geopointsX, out geopointsY);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new InvalidOperationException(
                        string.Format("Couldn't read 1d network geometry because of error number {0}", ierr));
                }
                return ierr;
            }
            return -1;
        }

        #endregion

        #region Write 1D network discretisation

        public void Create1DMeshInFile(string name, int numberOfMeshPoints, int numberOfMeshEdges)
        {
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {
                var ierr = uGridApi1D.Create1DMesh(name, numberOfMeshPoints, numberOfMeshEdges);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new InvalidOperationException(string.Format(
                            "Couldn't create new 1d mesh {0} with number of mesh points {1}, number of mesh edges {2} because of error number {3}",
                            name, numberOfMeshPoints, numberOfMeshEdges, ierr));
                }
            }
        }

        public void Write1DMeshDiscretizationPoints(int[] branchIdx, double[] offset)
        {
            var uGridApi1D = GridApi as IUGridApi1D;
            if (uGridApi1D != null)
            {
                var ierr = uGridApi1D.Write1DMeshDiscretisationPoints(branchIdx, offset);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new InvalidOperationException(string.Format("Couldn't write 1d mesh discretisation points because of error number {0}", ierr));
                }
            }
        }

        #endregion

        #region Read 1D network discretisation

        public int GetNumberOfMeshDiscretisationPoints()
        {
            var ugridApi = GridApi as IUGridApi1D;
            if (ugridApi != null)
            {
                var numberOfMeshPoints = ugridApi.GetNumberOfMeshDiscretisationPoints();
                if (numberOfMeshPoints < 0)
                {
                    throw new InvalidOperationException(string.Format("Couldn't get the number of 1D mesh discretisation points because of error number {0}", numberOfMeshPoints));
                }
                return numberOfMeshPoints;
            }
            return -1;
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