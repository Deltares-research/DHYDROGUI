using System;
using System.Threading;
using DelftTools.Utils.Remoting;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteGridApi : IGridApi
    {
        private bool disposed; 
        private IGridApi api;
        
        public RemoteGridApi()
        {
            // DeltaShell is 32bit, however we still want to take advantage of the 64bit dflowfm.dll if the system can use it, 
            // so we need to start the 64bit worker. This works as long as the data send over the IFlexibleMeshModelApi border 
            // is not bit dependent, eg IntPtr and the like.
            api = RemoteInstanceContainer.CreateInstance<IGridApi, GridApi>(Environment.Is64BitOperatingSystem);
        }
        
        public GridApiDataSet.DataSetConventions GetConvention(string file)
        {
            return api != null ? api.GetConvention(file) : GridApiDataSet.DataSetConventions.IONC_CONV_NULL;
        }

        public bool adherestoConventions(GridApiDataSet.DataSetConventions convtype)
        {
            return api != null && api.adherestoConventions(convtype);
        }
        
        public void Open(string c_path, GridApiDataSet.NetcdfOpenMode mode)
        {
            if(api != null)
                api.Open(c_path, mode) ;
        }

        public bool Initialized()
        {
            return api != null && api.Initialized();
        }

        public void Close()
        {
            if(api != null)
                api.Close();
        }

        public int GetMeshCount()
        {
            return api != null ? api.GetMeshCount() : 0;
        }

        public int GetCoordinateSystemCode()
        {
            return api != null ? api.GetCoordinateSystemCode() : 0;
        }

        public GridApiDataSet.DataSetConventions GetConvention()
        {
            return api != null ? api.GetConvention() : GridApiDataSet.DataSetConventions.IONC_CONV_NULL;
        }

        public double GetVersion()
        {
            return api != null ? api.GetVersion() : double.NaN;
        }

        public int GetNumberOfNodes(int meshid)
        {
            return api != null ? api.GetNumberOfNodes(meshid) : 0;
        }
        
        public int GetNumberOfEdges(int meshid)
        {
            return api != null ? api.GetNumberOfEdges(meshid) : 0;
        }

        public int GetNumberOfFaces(int meshid)
        {
            return api != null ? api.GetNumberOfFaces(meshid) : 0;
        }

        public int GetMaxFaceNodes(int meshid)
        {
            return api != null ? api.GetMaxFaceNodes(meshid) : 0;
        }

        public double[] GetNodeXCoordinates(int meshId)
        {
            return api != null ? api.GetNodeXCoordinates(meshId) : new double[0];
        }

        public double[] GetNodeYCoordinates(int meshId)
        {
            return api != null ? api.GetNodeYCoordinates(meshId) : new double[0];
        }
        
        public double[] GetNodeZCoordinates(int meshId)
        {
            return api != null ? api.GetNodeZCoordinates(meshId) : new double[0];
        }

        public double zCoordinateFillValue
        {
            get
            {
                return api != null ? api.zCoordinateFillValue : double.NaN;
            }
            set
            {
                if (api != null)
                    api.zCoordinateFillValue = value;
            }
        }

        public int[,] GetEdgeNodesForMesh(int meshId)
        {
            return api != null ? api.GetEdgeNodesForMesh(meshId) : new int[0,0];
        }

        public int[,] GetFaceNodesForMesh(int meshId)
        {
            return api != null ? api.GetFaceNodesForMesh(meshId) : new int[0, 0];
        }

        public int GetVarCount(int meshId, int locationId)
        {
            return api != null ? api.GetVarCount(meshId, locationId) : 0;
        }

        public int[] GetVarNames(int meshId, int locationId)
        {
            return api != null ? api.GetVarNames(meshId, locationId) : new int[0];
        }

        public void WriteXYCoordinateValues(int meshId, double[] xValues, double[] yValues)
        {
            if(api != null)
                api.WriteXYCoordinateValues(meshId, xValues, yValues);
        }
        
        public void WriteZCoordinateValues(int meshId, double[] zValues)
        {
            if(api != null)
                api.WriteZCoordinateValues(meshId, zValues);
        }

        public string GetMeshName(int meshId)
        {
            return api != null ? api.GetMeshName(meshId) : string.Empty;
        }
        
        public int ionc_write_geom_ugrid(string filename)
        {
            return api != null ? api.ionc_write_geom_ugrid(filename) : 0;
        }

        public int ionc_write_map_ugrid(string filename)
        {
            return api != null ? api.ionc_write_map_ugrid(filename) : 0;
        }

        
        public int Initialize()
        {
            return api != null ? api.Initialize() : 0;
        }

        
        ~RemoteGridApi()
        {
            // in case someone forgets to dispose..
            DisposeInternal();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        private void DisposeInternal()
        {
            if (api != null)
            {
                RemoteInstanceContainer.RemoveInstance(api);
            }
            api = null;
            disposed = true;
            Thread.Sleep(100); // wait for process to truly exit
        }
    }
}