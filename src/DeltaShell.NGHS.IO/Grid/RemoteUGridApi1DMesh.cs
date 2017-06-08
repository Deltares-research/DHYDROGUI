using System;
using DelftTools.Utils.Remoting;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGridApi1DMesh : RemoteGridApi, IUGridApi1DMesh
    {
        public RemoteUGridApi1DMesh()
        {
            // DeltaShell is 32bit, however we still want to take advantage of the 64bit dflowfm.dll if the system can use it, 
            // so we need to start the 64bit worker. This works as long as the data send over the IFlexibleMeshModelApi border 
            // is not bit dependent, eg IntPtr and the like.
            api = RemoteInstanceContainer.CreateInstance<IUGridApi1DMesh, UGridApi1DMesh>(Environment.Is64BitOperatingSystem);
        }
        
        public virtual int Create1DMesh(string name, int numberOfMeshPoints, int numberOfMeshEdges, int networkId)
        {
            return GetFromValidUGridApi1D(ugridApi1DMesh => ugridApi1DMesh.Create1DMesh(name, numberOfMeshPoints, numberOfMeshEdges, networkId), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int Write1DMeshDiscretisationPoints(int[] branchIdx, double[] offset)
        {
            return GetFromValidUGridApi1D(ugridApi1DMesh => ugridApi1DMesh.Write1DMeshDiscretisationPoints(branchIdx, offset), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int GetNumberOf1DMeshDiscretisationPoints()
        {
            return GetFromValidUGridApi1D(ugridApi1DMesh => ugridApi1DMesh.GetNumberOf1DMeshDiscretisationPoints(), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public int Read1DMeshDiscretisationPoints(out int[] branchIdx, out double[] offset)
        {
            branchIdx = new int[0];
            offset = new double[0];

            var uGridApi1DMesh = api as IUGridApi1DMesh;
            return uGridApi1DMesh != null
                ? uGridApi1DMesh.Read1DMeshDiscretisationPoints(out branchIdx, out offset)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        private T GetFromValidUGridApi1D<T>(Func<IUGridApi1DMesh, T> function, T defaultValue)
        {
            var ugridApi1DMesh = api as IUGridApi1DMesh;
            return ugridApi1DMesh != null ? function(ugridApi1DMesh) : defaultValue;
        }

    }
}