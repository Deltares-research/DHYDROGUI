using System;
using System.Threading;
using DelftTools.Utils.Remoting;

namespace DeltaShell.NGHS.IO.Grid
{
    public abstract class RemoteGridApi : IGridApi
    {
        protected bool disposed;
        protected IGridApi api;

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
            if (api != null)
                api.Open(c_path, mode);
        }

        public bool Initialized
        {
            get{ return api != null && api.Initialized;}
        }

        public void Close()
        {
            if (api != null)
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
        

        public int Initialize()
        {
            return api != null ? api.Initialize() : 0;
        }


        ~RemoteGridApi()
        {
            // in case someone forgets to dispose..
            DisposeInternal();
        }

        public virtual void Dispose()
        {
            if (disposed)
                return;
            Close();
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