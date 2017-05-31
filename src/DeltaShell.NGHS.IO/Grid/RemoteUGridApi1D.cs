using System;
using System.Collections.Generic;
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

        public virtual int Create1DNetwork(string name, int numberOfNodes, int numberOfBranches, int totalNumberOfGeometryPoints)
        {
            return GetFromValidUGridApi1D(ugridApi1D => ugridApi1D.Create1DNetwork(name, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public bool NetworkReady
        {
            get { return GetFromValidUGridApi1D(ugridApi1D => ugridApi1D.NetworkReady, false); }
        }

        public virtual bool IsInitialized()
        {
            return Initialized;
        }

        public virtual bool IsNetworkReady()
        {
            return NetworkReady;
        }

        public virtual int Write1DNetworkNodes(double[] nodesX, double[] nodesY, string[] nodesids, string[] nodeslongNames)
        {
            return GetFromValidUGridApi1D(ugridApi1D => ugridApi1D.Write1DNetworkNodes(nodesX, nodesY, nodesids, nodeslongNames),GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int Write1DNetworkBranches(int[] sourceNodeId, int[] targetNodeId, double[] branchLengths, int[] nbranchgeometrypoints, string[] branchIds, string[] branchLongnames)
        {
            return GetFromValidUGridApi1D(ugridApi1D => ugridApi1D.Write1DNetworkBranches(sourceNodeId, targetNodeId, branchLengths, nbranchgeometrypoints, branchIds, branchLongnames), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int Write1DNetworkGeometry(double[] geopointsX, double[] geopointsY)
        {
            return GetFromValidUGridApi1D(ugridApi1D => ugridApi1D.Write1DNetworkGeometry(geopointsX, geopointsY), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int GetNumberOfNetworkNodes()
        {
            return GetFromValidUGridApi1D(ugridApi1D => ugridApi1D.GetNumberOfNetworkNodes(), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int GetNumberOfNetworkBranches()
        {
            return GetFromValidUGridApi1D(ugridApi1D => ugridApi1D.GetNumberOfNetworkBranches(), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int GetNumberOfNetworkGeometryPoints()
        {
            return GetFromValidUGridApi1D(ugridApi1D => ugridApi1D.GetNumberOfNetworkGeometryPoints(), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int Create1DMesh(string name, int numberOfMeshPoints, int numberOfMeshEdges)
        {
            return GetFromValidUGridApi1D(ugridApi1D => ugridApi1D.Create1DMesh(name, numberOfMeshPoints, numberOfMeshEdges), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int Write1DMeshDiscretisationPoints(int[] branchIdx, double[] offset)
        {
            return GetFromValidUGridApi1D(ugridApi1D => ugridApi1D.Write1DMeshDiscretisationPoints(branchIdx, offset), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        public virtual int GetNumberOfMeshDiscretisationPoints()
        {
            return GetFromValidUGridApi1D(ugridApi1D => ugridApi1D.GetNumberOfMeshDiscretisationPoints(), GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR);
        }

        // TODO: uncomment this with new functionality
        //public List<Tuple<int, double>> GetMeshDiscretisationPoints()
        //{
        //    return GetFromValidUGridApi1D(ugridApi1D => ugridApi1D.GetMeshDiscretisationPoints(), new List<Tuple<int, double>>());
        //}

        public virtual int Read1DNetworkNodes(out double[] nodesX, out double[] nodesY, out string[] nodesIs, out string[] nodesLongnames)
        {
            nodesX = new double[0];
            nodesY = new double[0];
            nodesIs = new string[0];
            nodesLongnames = new string[0];

            var ugridApi1D = api as IUGridApi1D;

            return ugridApi1D != null
                ? ugridApi1D.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIs, out nodesLongnames)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual int Read1DNetworkBranches(out int[] sourceNodes, out int[] targetNodes, out double[] branchLengths,
            out int[] branchGeoPoints, out string[] branchIds, out string[] branchLongnames)
        {
            sourceNodes = new int[0];
            targetNodes = new int[0];
            branchLengths = new double[0];
            branchGeoPoints = new int[0];
            branchIds = new string[0];
            branchLongnames = new string[0];

            var ugridApi1D = api as IUGridApi1D;

            return ugridApi1D != null
                ? ugridApi1D.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public virtual int Read1DNetworkGeometry(out double[] geopointsX, out double[] geopointsY)
        {
            geopointsX = new double[0];
            geopointsY = new double[0];

            var ugridApi1D = api as IUGridApi1D;

            return ugridApi1D != null
                ? ugridApi1D.Read1DNetworkGeometry(out geopointsX, out geopointsY)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
            
        }

        private T GetFromValidUGridApi1D<T>(Func<IUGridApi1D, T> function, T defaultValue)
        {
            var ugridApi1D = api as IUGridApi1D;
            return ugridApi1D != null ? function(ugridApi1D) : defaultValue;
        }
    }
}