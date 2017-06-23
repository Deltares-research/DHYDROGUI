using System;
using DelftTools.Utils.Remoting;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGridApiNetworkDiscretisation : RemoteGridApi, IUGridApiNetworkDiscretisation
    {
        public RemoteUGridApiNetworkDiscretisation()
        {
            // DeltaShell is 32bit, however we still want to take advantage of the 64bit dflowfm.dll if the system can use it, 
            // so we need to start the 64bit worker. This works as long as the data send over the IFlexibleMeshModelApi border 
            // is not bit dependent, eg IntPtr and the like.
            api = RemoteInstanceContainer.CreateInstance<IUGridApiNetworkDiscretisation, UGridApiNetworkDiscretisation>(Environment.Is64BitOperatingSystem);
        }
        
        public virtual int CreateNetworkDiscretisation(string name, int numberOfNetworkPoints, int numberOfMeshEdges, int networkId)
        {
            return GetFromValidUGridApiNetwork(ugridApiNetwork => ugridApiNetwork.CreateNetworkDiscretisation(name, numberOfNetworkPoints, numberOfMeshEdges, networkId), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int WriteNetworkDiscretisationPoints(int[] branchIdx, double[] offset)
        {
            return GetFromValidUGridApiNetwork(ugridApiNetwork => ugridApiNetwork.WriteNetworkDiscretisationPoints(branchIdx, offset), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public int GetNetworkDiscretisationName(int meshId, out string meshName)
        {
            var uGridApiNetworkDiscretisation = api as IUGridApiNetworkDiscretisation;
            meshName = string.Empty;
            return uGridApiNetworkDiscretisation != null
                ? uGridApiNetworkDiscretisation.GetNetworkDiscretisationName(meshId, out meshName)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }


        public virtual int GetNumberOfNetworkDiscretisationPoints(int meshId)
        {
            return GetFromValidUGridApiNetwork(ugridApiNetwork => ugridApiNetwork.GetNumberOfNetworkDiscretisationPoints(meshId), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public int ReadNetworkDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset)
        {
            branchIdx = new int[0];
            offset = new double[0];

            var uGridApiNetworkDiscretisation = api as IUGridApiNetworkDiscretisation;
            return uGridApiNetworkDiscretisation != null
                ? uGridApiNetworkDiscretisation.ReadNetworkDiscretisationPoints(meshId, out branchIdx, out offset)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        private T GetFromValidUGridApiNetwork<T>(Func<IUGridApiNetworkDiscretisation, T> function, T defaultValue)
        {
            var uGridApiNetworkDiscretisation = api as IUGridApiNetworkDiscretisation;
            return uGridApiNetworkDiscretisation != null ? function(uGridApiNetworkDiscretisation) : defaultValue;
        }

    }
}