using System;
using DelftTools.Utils.Remoting;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGridApi : RemoteGridApi, IUGridApi
    {
        public RemoteUGridApi()
        {
            // DeltaShell is 32bit, however we still want to take advantage of the 64bit dflowfm.dll if the system can use it, 
            // so we need to start the 64bit worker. This works as long as the data send over the IFlexibleMeshModelApi border 
            // is not bit dependent, eg IntPtr and the like.
            api = RemoteInstanceContainer.CreateInstance<IUGridApi, UGridApi>(Environment.Is64BitOperatingSystem);
        }
       

        public int GetNumberOfNodes(int meshid)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.GetNumberOfNodes(meshid) : 0;
        }
        
        public int GetNumberOfEdges(int meshid)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.GetNumberOfEdges(meshid) : 0;
        }

        public int GetNumberOfFaces(int meshid)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.GetNumberOfFaces(meshid) : 0;
        }

        public int GetMaxFaceNodes(int meshid)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.GetMaxFaceNodes(meshid) : 0;
        }

        public double[] GetNodeXCoordinates(int meshId)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.GetNodeXCoordinates(meshId) : new double[0];
        }

        public double[] GetNodeYCoordinates(int meshId)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.GetNodeYCoordinates(meshId) : new double[0];
        }
        
        public double[] GetNodeZCoordinates(int meshId)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.GetNodeZCoordinates(meshId) : new double[0];
        }

        public double zCoordinateFillValue
        {
            get
            {
                var ugridApi = api as IUGridApi;
                return ugridApi != null ? ugridApi.zCoordinateFillValue : double.NaN;
            }
            set
            {
                var ugridApi = api as IUGridApi;
                if (ugridApi != null)
                    ugridApi.zCoordinateFillValue = value;
            }
        }

        public int[,] GetEdgeNodesForMesh(int meshId)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.GetEdgeNodesForMesh(meshId) : new int[0,0];
        }

        public int[,] GetFaceNodesForMesh(int meshId)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.GetFaceNodesForMesh(meshId) : new int[0, 0];
        }

        public int GetVarCount(int meshId, int locationId)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.GetVarCount(meshId, locationId) : 0;
        }

        public int[] GetVarNames(int meshId, int locationId)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.GetVarNames(meshId, locationId) : new int[0];
        }

        public void WriteXYCoordinateValues(int meshId, double[] xValues, double[] yValues)
        {
            var ugridApi = api as IUGridApi;
            if (ugridApi != null)
                ugridApi.WriteXYCoordinateValues(meshId, xValues, yValues);
        }
        
        public void WriteZCoordinateValues(int meshId, double[] zValues)
        {
            var ugridApi = api as IUGridApi;
            if (ugridApi != null)
                ugridApi.WriteZCoordinateValues(meshId, zValues);
        }

        public string GetMeshName(int meshId)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.GetMeshName(meshId) : string.Empty;
        }
        
        public int ionc_write_geom_ugrid(string filename)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.ionc_write_geom_ugrid(filename) : 0;
        }

        public int ionc_write_map_ugrid(string filename)
        {
            var ugridApi = api as IUGridApi;
            return ugridApi != null ? ugridApi.ionc_write_map_ugrid(filename) : 0;
        }
    }
}