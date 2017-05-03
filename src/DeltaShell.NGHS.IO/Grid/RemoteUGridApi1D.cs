using System;
using DelftTools.Utils.Remoting;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGridApi1D : RemoteGridApi, IUGridApi1D
    {
    
        public RemoteUGridApi1D()
        {
            // DeltaShell is 32bit, however we still want to take advantage of the 64bit dflowfm.dll if the system can use it, 
            // so we need to start the 64bit worker. This works as long as the data send over the IFlexibleMeshModelApi border 
            // is not bit dependent, eg IntPtr and the like.
            api = RemoteInstanceContainer.CreateInstance<IUGridApi1D, UGridApi1D>(Environment.Is64BitOperatingSystem);
        }

        public int Create1DNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints)
        {
            var ugridApi1D = api as IUGridApi1D;
            return ugridApi1D != null ? ugridApi1D.Create1DNetwork(name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints) : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public bool NetworkReady
        {
            get
            {
                var ugridApi1D = api as IUGridApi1D;
                return ugridApi1D != null ? ugridApi1D.NetworkReady : false;
            }
        }

        public int Write1DNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            var ugridApi1D = api as IUGridApi1D;
            for (int index = 0; index < nodesids.Length; index++)
            {
                nodesids[index] = nodesids[index]?? string.Empty;
            }
            for (int index = 0; index < nodeslongNames.Length; index++)
            {
                nodeslongNames[index] = nodeslongNames[index] ?? string.Empty;
            }
            return ugridApi1D != null ? ugridApi1D.Write1DNetworkNodes(nodesX, nodesY, nodesids, nodeslongNames) : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int Write1DNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames)
        {
            var ugridApi1D = api as IUGridApi1D;
            for (int index = 0; index < branchIds.Length; index++)
            {
                branchIds[index] = branchIds[index] ?? string.Empty;
            }
            for (int index = 0; index < branchLongnames.Length; index++)
            {
                branchLongnames[index] = branchLongnames[index] ?? string.Empty;
            }
            return ugridApi1D != null ? ugridApi1D.Write1DNetworkBranches(sourceNodeId, targetNodeId, branchLengths, nbranchgeometrypoints, branchIds, branchLongnames) : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int Write1DNetworkGeometry(double[] geopointsX, double[] geopointsY)
        {
            var ugridApi1D = api as IUGridApi1D;
            return ugridApi1D != null ? ugridApi1D.Write1DNetworkGeometry(geopointsX, geopointsY) : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetNumberOfNetworkNodes()
        {
            var ugridApi1D = api as IUGridApi1D;
            return ugridApi1D != null ? ugridApi1D.GetNumberOfNetworkNodes() : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            
        }

        public int GetNumberOfNetworkBranches()
        {
            var ugridApi1D = api as IUGridApi1D;
            return ugridApi1D != null ? ugridApi1D.GetNumberOfNetworkBranches() : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetNumberOfNetworkGeometryPoints()
        {
            var ugridApi1D = api as IUGridApi1D;
            return ugridApi1D != null ? ugridApi1D.GetNumberOfNetworkGeometryPoints() : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }
    }
}