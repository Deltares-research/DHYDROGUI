using System;
using DelftTools.Utils.Remoting;
using DeltaShell.Dimr;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGridNetworkApi : RemoteGridApi, IUGridNetworkApi
    {
    
        public RemoteUGridNetworkApi()
        {
            // We need to pass the Dimr Assembly here, in order to get the SharedDllPath
            var dimrDllAssembly = typeof(DimrRunner).Assembly;

            api = RemoteInstanceContainer.CreateInstance<IUGridNetworkApi, UGridNetworkApi>(Environment.Is64BitOperatingSystem, null, false, dimrDllAssembly);
        }

        public virtual int CreateNetwork(int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints, out int nwId)
        {
            nwId = -1;
            var uGridNetworkApi = api as IUGridNetworkApi;
            return uGridNetworkApi != null
                ? uGridNetworkApi.CreateNetwork(numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints,out nwId)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public virtual bool NetworkReadyForWriting
        {
            get { return GetFromValidUGridNetworkApi(ugridNetworkApi => ugridNetworkApi.NetworkReadyForWriting, false); }
        }
        
        public virtual int WriteNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            return GetFromValidUGridNetworkApi(ugridNetworkApi => ugridNetworkApi.WriteNetworkNodes(nodesX, nodesY, nodesids, nodeslongNames),GridApiDataSet.GridConstants.GENERAL_FATAL_ERR);
        }

        public virtual int WriteNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames, int[] branchOrderNumbers)
        {
            return GetFromValidUGridNetworkApi(ugridNetworkApi => ugridNetworkApi.WriteNetworkBranches(sourceNodeId, targetNodeId, branchLengths, nbranchgeometrypoints, branchIds, branchLongnames, branchOrderNumbers), GridApiDataSet.GridConstants.GENERAL_FATAL_ERR);
        }

        public virtual int WriteNetworkGeometry(double[] geopointsX, double[] geopointsY)
        {
            return GetFromValidUGridNetworkApi(ugridNetworkApi => ugridNetworkApi.WriteNetworkGeometry(geopointsX, geopointsY), GridApiDataSet.GridConstants.GENERAL_FATAL_ERR);
        }

        public int GetNetworkName(int networkId, out string networkName)
        {
            networkName = string.Empty;
            var uGridNetworkApi = api as IUGridNetworkApi;
            return uGridNetworkApi != null
                ? uGridNetworkApi.GetNetworkName(networkId, out networkName)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public virtual int GetNumberOfNetworkNodes(int networkId, out int numberOfNetworkNodes)
        {
            numberOfNetworkNodes = - 1;
            var ugridNetworkApi = api as IUGridNetworkApi;
            return ugridNetworkApi != null
                ? ugridNetworkApi.GetNumberOfNetworkNodes(networkId, out numberOfNetworkNodes)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public virtual int GetNumberOfNetworkBranches(int networkId, out int numberOfNetworkBranches)
        {
            numberOfNetworkBranches = -1;
            var uGridNetworkApi = api as IUGridNetworkApi;
            return uGridNetworkApi != null
                ? uGridNetworkApi.GetNumberOfNetworkBranches(networkId, out numberOfNetworkBranches)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public virtual int GetNumberOfNetworkGeometryPoints(int networkId, out int numberOfNetworkGeometryPoints)
        {
            numberOfNetworkGeometryPoints = -1;
            var uGridNetworkApi = api as IUGridNetworkApi;
            return uGridNetworkApi != null
                ? uGridNetworkApi.GetNumberOfNetworkGeometryPoints(networkId, out numberOfNetworkGeometryPoints)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
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
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public virtual int ReadNetworkBranches(int networkId, out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths, out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames, out int[] branchOrderNumbers)
        {
            sourceNodes = new int[0];
            targetNodes = new int[0];
            branchLengths = new double[0];
            branchGeoPoints = new int[0];
            branchIds = new string[0];
            branchLongnames = new string[0];
            branchOrderNumbers = new int[0];

            var ugridNetworkApi = api as IUGridNetworkApi;

            return ugridNetworkApi != null
                ? ugridNetworkApi.ReadNetworkBranches(networkId, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public virtual int ReadNetworkGeometry(int networkId, out double[] geopointsX, out double[] geopointsY)
        {
            geopointsX = new double[0];
            geopointsY = new double[0];

            var ugridNetworkApi = api as IUGridNetworkApi;

            return ugridNetworkApi != null
                ? ugridNetworkApi.ReadNetworkGeometry(networkId, out geopointsX, out geopointsY)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
            
        }
        
        private T GetFromValidUGridNetworkApi<T>(Func<IUGridNetworkApi, T> function, T defaultValue)
        {
            var ugridNetworkApi = api as IUGridNetworkApi;
            return ugridNetworkApi != null ? function(ugridNetworkApi) : defaultValue;
        }
    }
}