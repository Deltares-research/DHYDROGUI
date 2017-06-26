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

        public int GetConvention(string file, out GridApiDataSet.DataSetConventions convention)
        {
            convention = GridApiDataSet.DataSetConventions.IONC_CONV_NULL;
            return api != null ? api.GetConvention(file, out convention) : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public bool adherestoConventions(GridApiDataSet.DataSetConventions convtype)
        {
            return api != null && api.adherestoConventions(convtype);
        }

        public int CreateFile(string filePath, UGridGlobalMetaData uGridGlobalMetaData, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_write)
        {
            return api != null
                ? api.CreateFile(filePath, uGridGlobalMetaData, mode)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int Open(string filePath, GridApiDataSet.NetcdfOpenMode mode)
        {
            return api != null ? api.Open(filePath, mode) : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public bool Initialized
        {
            get{ return api != null && api.Initialized;}
        }

        public int Close()
        {
            return api != null ? api.Close() : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetMeshCount(out int numberOfMeshes)
        {
            numberOfMeshes = 0;
            return api != null ? api.GetMeshCount(out numberOfMeshes) : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetCoordinateSystemCode(out int coordinateSystemCode)
        {
            coordinateSystemCode = 0;
            return api != null ? api.GetCoordinateSystemCode(out coordinateSystemCode) : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
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

        public int GetNumberOfMeshByType(UGridMeshType meshType, out int numberOfMesh)
        {
            numberOfMesh = 0;
            return api != null
                ? api.GetNumberOfMeshByType(meshType, out numberOfMesh)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetMeshIdsByType(UGridMeshType meshType, int numberOfMeshes, out int[] meshIds)
        {
            meshIds = new int[0];
            return api != null
                ? api.GetMeshIdsByType(meshType, numberOfMeshes, out meshIds)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
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