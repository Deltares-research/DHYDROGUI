using System;
using DelftTools.Utils.Remoting;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGridApi1DDiscretisation : RemoteGridApi, IUGridApi1DDiscretisation
    {
        public RemoteUGridApi1DDiscretisation()
        {
            // DeltaShell is 32bit, however we still want to take advantage of the 64bit dflowfm.dll if the system can use it, 
            // so we need to start the 64bit worker. This works as long as the data send over the IFlexibleMeshModelApi border 
            // is not bit dependent, eg IntPtr and the like.
            api = RemoteInstanceContainer.CreateInstance<IUGridApi1DDiscretisation, UGridApi1DDiscretisation>(Environment.Is64BitOperatingSystem);
        }
        
        public virtual int Create1dDiscretisation(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId)
        {
            return GetFromValidUGridApi1D(ugridApi1DMesh => ugridApi1DMesh.Create1dDiscretisation(name, numberOfMeshPoints, numberOfMeshEdges, networkId), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int Write1dDiscretisationPoints(int[] branchIdx, double[] offset)
        {
            return GetFromValidUGridApi1D(ugridApi1DMesh => ugridApi1DMesh.Write1dDiscretisationPoints(branchIdx, offset), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public int GetMeshDiscretisationName(int meshId, out string meshName)
        {
            var uGridApi1DMesh = api as IUGridApi1DDiscretisation;
            meshName = string.Empty;
            return uGridApi1DMesh != null
                ? uGridApi1DMesh.GetMeshDiscretisationName(meshId, out meshName)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }


        public virtual int GetNumberOf1dDiscretisationPoints(int meshId)
        {
            return GetFromValidUGridApi1D(ugridApi1DMesh => ugridApi1DMesh.GetNumberOf1dDiscretisationPoints(meshId), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public int Read1dDiscretisationPoints(int meshId, out int[] branchIdx, out double[] offset)
        {
            branchIdx = new int[0];
            offset = new double[0];

            var uGridApi1DMesh = api as IUGridApi1DDiscretisation;
            return uGridApi1DMesh != null
                ? uGridApi1DMesh.Read1dDiscretisationPoints(meshId, out branchIdx, out offset)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        private T GetFromValidUGridApi1D<T>(Func<IUGridApi1DDiscretisation, T> function, T defaultValue)
        {
            var ugridApi1DMesh = api as IUGridApi1DDiscretisation;
            return ugridApi1DMesh != null ? function(ugridApi1DMesh) : defaultValue;
        }

    }
}