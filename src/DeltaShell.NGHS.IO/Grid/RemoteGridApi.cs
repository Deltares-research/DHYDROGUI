using System;
using System.Threading;
using DelftTools.Utils.Remoting;
using ProtoBufRemote;

namespace DeltaShell.NGHS.IO.Grid
{
    public abstract class RemoteGridApi : IGridApi
    {
        protected bool disposed;
        protected IGridApi api;

        static RemoteGridApi()
        {
            RemotingTypeConverters.RegisterTypeConverter(new UgridGlobalMetaDataToProtoConverter());
        }

        public GridApiDataSet.DataSetConventions GetConvention(string file)
        {
            return api != null ? api.GetConvention(file) : GridApiDataSet.DataSetConventions.IONC_CONV_NULL;
        }

        public bool adherestoConventions(GridApiDataSet.DataSetConventions convtype)
        {
            return api != null && api.adherestoConventions(convtype);
        }

        public void CreateFile(string c_path, UGridGlobalMetaData uGridGlobalMetaData, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_write)
        {
            if(api != null)
                api.CreateFile(c_path, uGridGlobalMetaData, mode);
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

        public int GetNumberOfNetworks(out int numberOfNetworks)
        {
            numberOfNetworks = 0;
            return api != null ? api.GetNumberOfNetworks(out numberOfNetworks) : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetNetworkIds(out int[] networkIds)
        {
            networkIds = new int[0];
            return api != null ? api.GetNetworkIds(out networkIds) : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }


        ~RemoteGridApi()
        {
            // in case someone forgets to dispose..
            Dispose(false);
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (api != null)
                    {
                        Close();
                        RemoteInstanceContainer.RemoveInstance(api);
                        Thread.Sleep(100); // wait for process to truly exit
                    }
                    api = null;
                }
                disposed = true;
            }
        }
       
    }
}