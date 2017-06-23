using System;
using DelftTools.Utils.Remoting;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGridApiNetwork : RemoteGridApi, IUGridApiNetwork
    {
    
        public RemoteUGridApiNetwork()
        {
            // DeltaShell is 32bit, however we still want to take advantage of the 64bit dflowfm.dll if the system can use it, 
            // so we need to start the 64bit worker. This works as long as the data send over the IFlexibleMeshModelApi border 
            // is not bit dependent, eg IntPtr and the like.
            api = RemoteInstanceContainer.CreateInstance<IUGridApiNetwork, UGridApiNetwork>(Environment.Is64BitOperatingSystem);
        }

        public virtual int CreateNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int nwId)
        {
            nwId = -1;
            var uGridApiNetwork = api as IUGridApiNetwork;
            return uGridApiNetwork != null
                ? uGridApiNetwork.CreateNetwork(name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints,out nwId)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual bool NetworkReadyForWriting
        {
            get { return GetFromValidUGridApi1D(ugridApiNetwork => ugridApiNetwork.NetworkReadyForWriting, false); }
        }
        
        public virtual int WriteNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            return GetFromValidUGridApi1D(ugridApiNetwork => ugridApiNetwork.WriteNetworkNodes(nodesX, nodesY, nodesids, nodeslongNames),GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int WriteNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames)
        {
            return GetFromValidUGridApi1D(ugridApiNetwork => ugridApiNetwork.WriteNetworkBranches(sourceNodeId, targetNodeId, branchLengths, nbranchgeometrypoints, branchIds, branchLongnames), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int WriteNetworkGeometry(double[] geopointsX, double[] geopointsY)
        {
            return GetFromValidUGridApi1D(ugridApiNetwork => ugridApiNetwork.WriteNetworkGeometry(geopointsX, geopointsY), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public int GetNetworkName(int networkId, out string networkName)
        {
            networkName = string.Empty;
            var uGridApi1D = api as IUGridApiNetwork;
            return uGridApi1D != null
                ? uGridApi1D.GetNetworkName(networkId, out networkName)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual int GetNumberOfNetworkNodes(int networkId, out int numberOfNetworkNodes)
        {
            numberOfNetworkNodes = - 1;
            var ugridApiNetwork = api as IUGridApiNetwork;
            return ugridApiNetwork != null
                ? ugridApiNetwork.GetNumberOfNetworkNodes(networkId, out numberOfNetworkNodes)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual int GetNumberOfNetworkBranches(int networkId, out int numberOfNetworkBranches)
        {
            numberOfNetworkBranches = -1;
            var uGridApi1D = api as IUGridApiNetwork;
            return uGridApi1D != null
                ? uGridApi1D.GetNumberOfNetworkBranches(networkId, out numberOfNetworkBranches)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual int GetNumberOfNetworkGeometryPoints(int networkId, out int numberOfNetworkGeometryPoints)
        {
            numberOfNetworkGeometryPoints = -1;
            var uGridApi1D = api as IUGridApiNetwork;
            return uGridApi1D != null
                ? uGridApi1D.GetNumberOfNetworkGeometryPoints(networkId, out numberOfNetworkGeometryPoints)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }
        
        public virtual int ReadNetworkNodes(int networkId, out double[] nodesX, out double[] nodesY, out string[] nodesIs, out string[] nodesLongnames)
        {
            nodesX = new double[0];
            nodesY = new double[0];
            nodesIs = new string[0];
            nodesLongnames = new string[0];

            var ugridApiNetwork = api as IUGridApiNetwork;

            return ugridApiNetwork != null   
                ? ugridApiNetwork.ReadNetworkNodes(networkId, out nodesX, out nodesY, out nodesIs, out nodesLongnames)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual int ReadNetworkBranches(int networkId, out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths,
            out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames)
        {
            sourceNodes = new int[0];
            targetNodes = new int[0];
            branchLengths = new double[0];
            branchGeoPoints = new int[0];
            branchIds = new string[0];
            branchLongnames = new string[0];

            var ugridApiNetwork = api as IUGridApiNetwork;

            return ugridApiNetwork != null
                ? ugridApiNetwork.ReadNetworkBranches(networkId, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual int ReadNetworkGeometry(int networkId, out double[] geopointsX, out double[] geopointsY)
        {
            geopointsX = new double[0];
            geopointsY = new double[0];

            var ugridApiNetwork = api as IUGridApiNetwork;

            return ugridApiNetwork != null
                ? ugridApiNetwork.ReadNetworkGeometry(networkId, out geopointsX, out geopointsY)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            
        }
        
        private T GetFromValidUGridApi1D<T>(Func<IUGridApiNetwork, T> function, T defaultValue)
        {
            var ugridApiNetwork = api as IUGridApiNetwork;
            return ugridApiNetwork != null ? function(ugridApiNetwork) : defaultValue;
        }
    }
}