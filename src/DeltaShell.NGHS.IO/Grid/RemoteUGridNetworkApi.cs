using System;
using DelftTools.Utils.Remoting;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGridNetworkApi : RemoteGridApi, IUGridNetworkApi
    {
    
        public RemoteUGridNetworkApi()
        {
            // DeltaShell is 32bit, however we still want to take advantage of the 64bit dflowfm.dll if the system can use it, 
            // so we need to start the 64bit worker. This works as long as the data send over the IFlexibleMeshModelApi border 
            // is not bit dependent, eg IntPtr and the like.
            api = RemoteInstanceContainer.CreateInstance<IUGridNetworkApi, UGridNetworkApi>(Environment.Is64BitOperatingSystem);
        }

        public virtual int CreateNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int nwId)
        {
            nwId = -1;
            var uGridNetworkApi = api as IUGridNetworkApi;
            return uGridNetworkApi != null
                ? uGridNetworkApi.CreateNetwork(name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints,out nwId)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual bool NetworkReadyForWriting
        {
            get { return GetFromValidUGridNetworkApi(ugridNetworkApi => ugridNetworkApi.NetworkReadyForWriting, false); }
        }
        
        public virtual int WriteNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            return GetFromValidUGridNetworkApi(ugridNetworkApi => ugridNetworkApi.WriteNetworkNodes(nodesX, nodesY, nodesids, nodeslongNames),GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int WriteNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames)
        {
            return GetFromValidUGridNetworkApi(ugridNetworkApi => ugridNetworkApi.WriteNetworkBranches(sourceNodeId, targetNodeId, branchLengths, nbranchgeometrypoints, branchIds, branchLongnames), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int WriteNetworkGeometry(double[] geopointsX, double[] geopointsY)
        {
            return GetFromValidUGridNetworkApi(ugridNetworkApi => ugridNetworkApi.WriteNetworkGeometry(geopointsX, geopointsY), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public int GetNetworkName(int networkId, out string networkName)
        {
            networkName = string.Empty;
            var uGridNetworkApi = api as IUGridNetworkApi;
            return uGridNetworkApi != null
                ? uGridNetworkApi.GetNetworkName(networkId, out networkName)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual int GetNumberOfNetworkNodes(int networkId, out int numberOfNetworkNodes)
        {
            numberOfNetworkNodes = - 1;
            var ugridNetworkApi = api as IUGridNetworkApi;
            return ugridNetworkApi != null
                ? ugridNetworkApi.GetNumberOfNetworkNodes(networkId, out numberOfNetworkNodes)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual int GetNumberOfNetworkBranches(int networkId, out int numberOfNetworkBranches)
        {
            numberOfNetworkBranches = -1;
            var uGridNetworkApi = api as IUGridNetworkApi;
            return uGridNetworkApi != null
                ? uGridNetworkApi.GetNumberOfNetworkBranches(networkId, out numberOfNetworkBranches)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual int GetNumberOfNetworkGeometryPoints(int networkId, out int numberOfNetworkGeometryPoints)
        {
            numberOfNetworkGeometryPoints = -1;
            var uGridNetworkApi = api as IUGridNetworkApi;
            return uGridNetworkApi != null
                ? uGridNetworkApi.GetNumberOfNetworkGeometryPoints(networkId, out numberOfNetworkGeometryPoints)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }
        
        public virtual int ReadNetworkNodes(int networkId, out double[] nodesX, out double[] nodesY, out string[] nodesIs, out string[] nodesLongnames)
        {
            nodesX = new double[0];
            nodesY = new double[0];
            nodesIs = new string[0];
            nodesLongnames = new string[0];

            var ugridNetworkApi = api as IUGridNetworkApi;

            return ugridNetworkApi != null   
                ? ugridNetworkApi.ReadNetworkNodes(networkId, out nodesX, out nodesY, out nodesIs, out nodesLongnames)
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

            var ugridNetworkApi = api as IUGridNetworkApi;

            return ugridNetworkApi != null
                ? ugridNetworkApi.ReadNetworkBranches(networkId, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual int ReadNetworkGeometry(int networkId, out double[] geopointsX, out double[] geopointsY)
        {
            geopointsX = new double[0];
            geopointsY = new double[0];

            var ugridNetworkApi = api as IUGridNetworkApi;

            return ugridNetworkApi != null
                ? ugridNetworkApi.ReadNetworkGeometry(networkId, out geopointsX, out geopointsY)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            
        }
        
        private T GetFromValidUGridNetworkApi<T>(Func<IUGridNetworkApi, T> function, T defaultValue)
        {
            var ugridNetworkApi = api as IUGridNetworkApi;
            return ugridNetworkApi != null ? function(ugridNetworkApi) : defaultValue;
        }
    }
}