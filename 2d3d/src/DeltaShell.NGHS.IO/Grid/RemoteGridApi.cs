using System;
using System.Threading;
using DelftTools.Utils.Remoting;
using ProtoBufRemote;

namespace DeltaShell.NGHS.IO.Grid
{
    public abstract class RemoteGridApi : IGridApi, IDisposable
    {
        private bool disposed;

        static RemoteGridApi()
        {
            RemotingTypeConverters.RegisterTypeConverter(new UgridGlobalMetaDataToProtoConverter());
        }

        protected IGridApi api;

        public bool Initialized
        {
            get
            {
                return api != null && api.Initialized;
            }
        }

        public int GetConvention(string file, out GridApiDataSet.DataSetConventions convention)
        {
            convention = GridApiDataSet.DataSetConventions.CONV_NULL;
            return api != null ? api.GetConvention(file, out convention) : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public GridApiDataSet.DataSetConventions GetConvention()
        {
            return api?.GetConvention() ?? GridApiDataSet.DataSetConventions.CONV_NULL;
        }

        public bool AdheresToConventions(GridApiDataSet.DataSetConventions convtype)
        {
            return api != null && api.AdheresToConventions(convtype);
        }

        public int CreateFile(string filePath, UGridGlobalMetaData uGridGlobalMetaData, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_write)
        {
            return api?.CreateFile(filePath, uGridGlobalMetaData, mode) ?? GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int Open(string filePath, GridApiDataSet.NetcdfOpenMode mode)
        {
            return api?.Open(filePath, mode) ?? GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int Close()
        {
            return api?.Close() ?? GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetMeshCount(out int numberOfMeshes)
        {
            numberOfMeshes = 0;
            return api != null ? api.GetMeshCount(out numberOfMeshes) : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetCoordinateSystemCode(out int coordinateSystemCode)
        {
            coordinateSystemCode = 0;
            return api != null ? api.GetCoordinateSystemCode(out coordinateSystemCode) : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public double GetVersion()
        {
            return api?.GetVersion() ?? double.NaN;
        }

        public int Initialize()
        {
            return api?.Initialize() ?? 0;
        }

        public int GetNumberOfMeshByType(UGridMeshType meshType, out int numberOfMesh)
        {
            numberOfMesh = 0;
            return api != null
                       ? api.GetNumberOfMeshByType(meshType, out numberOfMesh)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetMeshIdsByMeshType(UGridMeshType meshType, int numberOfMeshes, out int[] meshIds)
        {
            meshIds = new int[0];
            return api != null
                       ? api.GetMeshIdsByMeshType(meshType, numberOfMeshes, out meshIds)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

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

        ~RemoteGridApi()
        {
            // in case someone forgets to dispose..
            Dispose(false);
        }
    }
}