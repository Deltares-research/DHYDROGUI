using System;
using DelftTools.Utils.Remoting;
using DeltaShell.Dimr;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGridNetworkDiscretisationApi : RemoteGridApi, IUGridNetworkDiscretisationApi
    {
        public RemoteUGridNetworkDiscretisationApi()
        {
            // We need to pass the Dimr Assembly here, in order to get the SharedDllPath
            var dimrDllAssembly = typeof(DimrRunner).Assembly;

            api = RemoteInstanceContainer.CreateInstance<IUGridNetworkDiscretisationApi, UGridNetworkDiscretisationApi>(Environment.Is64BitOperatingSystem, null, false, dimrDllAssembly);
        }
        
        public virtual int CreateNetworkDiscretisation(int numberOfNetworkPoints)
        {
            return GetFromValidUGridApiNetwork(ugridApiNetwork => ugridApiNetwork.CreateNetworkDiscretisation(numberOfNetworkPoints), GridApiDataSet.GridConstants.GENERAL_FATAL_ERR);
        }

        public virtual int WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset, double[] discretisationPointsX, double[] discretisationPointsY, string[] ids, string[] names)
        {
            return GetFromValidUGridApiNetwork(ugridApiNetwork => ugridApiNetwork.WriteNetworkDiscretisationPoints(branchIdx, offset, discretisationPointsX, discretisationPointsY, ids, names), GridApiDataSet.GridConstants.GENERAL_FATAL_ERR);
        }

        public int GetNetworkIdFromMeshId(int meshId, out int networkId)
        {
            var uGridNetworkDiscretisationApi = api as IUGridNetworkDiscretisationApi;
            networkId = 0;
            return uGridNetworkDiscretisationApi != null
                ? uGridNetworkDiscretisationApi.GetNetworkIdFromMeshId(meshId, out networkId)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetNetworkDiscretisationName(int meshId, out string meshName)
        {
            var uGridNetworkDiscretisationApi = api as IUGridNetworkDiscretisationApi;
            meshName = string.Empty;
            return uGridNetworkDiscretisationApi != null
                ? uGridNetworkDiscretisationApi.GetNetworkDiscretisationName(meshId, out meshName)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }


        public virtual int GetNumberOfNetworkDiscretisationPoints(int meshId, out int numberOfDiscretisationPoints)
        {
            var uGridNetworkDiscretisationApi = api as IUGridNetworkDiscretisationApi;
            numberOfDiscretisationPoints = 0;
            return uGridNetworkDiscretisationApi != null
                ? uGridNetworkDiscretisationApi.GetNumberOfNetworkDiscretisationPoints(meshId, out numberOfDiscretisationPoints)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int ReadNetworkDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset, out double[] discretisationPointsX, out double[] discretisationPointsY, out string[] ids, out string[] names)
        {
            branchIdx = new int[0];
            offset = new double[0];
            ids = new string[0];
            names = new string[0];
            discretisationPointsX = new double[0];
            discretisationPointsY = new double[0];

            var uGridApiNetworkDiscretisation = api as IUGridNetworkDiscretisationApi;
            return uGridApiNetworkDiscretisation != null
                ? uGridApiNetworkDiscretisation.ReadNetworkDiscretisationPoints(meshId, out branchIdx, out offset, out discretisationPointsX, out discretisationPointsY, out ids, out names)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }
        
        private T GetFromValidUGridApiNetwork<T>(Func<IUGridNetworkDiscretisationApi, T> function, T defaultValue)
        {
            var uGridApiNetworkDiscretisation = api as IUGridNetworkDiscretisationApi;
            return uGridApiNetworkDiscretisation != null ? function(uGridApiNetworkDiscretisation) : defaultValue;
        }

    }
}