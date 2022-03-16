using System;
using System.IO;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using log4net;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Grid
{
    public abstract class AGrid<T> : IDisposable where T : class, IGridApi
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AGrid<T>));

        private readonly string filename;
        private readonly GridApiDataSet.NetcdfOpenMode mode;
        private bool disposed;
        private T gridApi;

        protected AGrid()
        {
            GlobalMetaData = new UGridGlobalMetaData();
        }

        protected AGrid(string filename, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : this()
        {
            this.filename = filename;
            this.mode = mode;
        }

        protected AGrid(string filename, UGridGlobalMetaData globalMetaData, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : this(filename, mode)
        {
            if (GlobalMetaData != null)
            {
                GlobalMetaData = globalMetaData;
            }
        }

        public virtual T GridApi
        {
            get
            {
                return gridApi;
            }
            set
            {
                var disposableGridApi = gridApi as IDisposable;
                if (disposableGridApi != null)
                {
                    disposableGridApi.Dispose();
                }

                gridApi = value;
            }
        }

        public virtual ICoordinateSystem CoordinateSystem { get; private set; }
        public UGridGlobalMetaData GlobalMetaData { get; private set; }

        public virtual bool IsValid()
        {
            return GridApi != null && GridApi.GetConvention() == GridApiDataSet.DataSetConventions.CONV_UGRID && GridApi.GetVersion() >= GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION;
        }

        public virtual void Initialize()
        {
            if (IsInitialized())
            {
                CleanUp();
                disposed = false;
            }

            if (GridApi != null)
            {
                string errorMessage = Resources.AGrid_Initialize_Couldn_t_open_grid_nc_file__ + filename;
                int ierr = GridApi.Open(filename, mode);
                ThrowIfError(ierr, errorMessage);

                try
                {
                    int epsg_code;
                    ierr = GridApi.GetCoordinateSystemCode(out epsg_code);
                    if (ierr != GridApiDataSet.GridConstants.NOERR)
                    {
                        CoordinateSystem = null;
                        throw new GridApiException(GridApiExceptionMessage.Format(ierr, Resources.AGrid_Initialize_Couldn_t_get_coordinate_system_code));
                    }

                    CoordinateSystem = epsg_code > 0 ? new OgrCoordinateSystemFactory().CreateFromEPSG(epsg_code) : null;
                }
                catch (Exception)
                {
                    if (IsInitialized())
                    {
                        CleanUp();
                    }
                }
            }
        }

        public virtual bool IsInitialized()
        {
            return GridApi != null && GridApi.Initialized;
        }

        public virtual void CreateFile()
        {
            if (filename != null && !File.Exists(filename))
            {
                int ierr = GridApi.CreateFile(filename, GlobalMetaData);
                ThrowIfError(ierr, Resources.AGrid_CreateFile_Couldn_t_create_new_NetCDF_file_at_location_ + filename);
            }
        }

        public virtual GridApiDataSet.DataSetConventions GetDataSetConvention()
        {
            if (!IsInitialized())
            {
                Initialize();
            }

            return GridApi.GetConvention();
        }

        public virtual void Dispose()
        {
            if (disposed)
            {
                return;
            }

            try
            {
                CleanUp();
            }
            finally
            {
                // Must always ensure this happens to prevent GC deadlock on project close!
                GC.SuppressFinalize(this);
            }
        }

        protected T GetValidGridApi(string errormessage)
        {
            if (!IsInitialized())
            {
                Initialize();
            }

            T uGridApi = GridApi;
            bool isValid = uGridApi != null && IsValid();
            if (!isValid)
            {
                throw new GridApiException(errormessage + Resources.AGrid___because_the_API_was_not_instantiated_);
            }

            return uGridApi;
        }

        protected void DoWithValidGridApi(Func<T, int> function, string errorMessage)
        {
            T uGridNetworkApi = GetValidGridApi(errorMessage);
            int ierr = function(uGridNetworkApi);
            ThrowIfError(ierr, errorMessage);
        }

        protected void DoWithValidGridApi(Action<T> action, string errorMessage)
        {
            T uGridApi = GetValidGridApi(errorMessage);
            action(uGridApi);
        }

        protected TValue GetFromValidGridApi<TValue>(Func<T, TValue> function, TValue defaultValue, string errorMessage)
        {
            T uGridApi = GetValidGridApi(errorMessage);
            return uGridApi != null ? function(uGridApi) : defaultValue;
        }

        protected void ThrowIfError(int ierr, string exceptionText)
        {
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                throw new GridApiException(GridApiExceptionMessage.Format(ierr, exceptionText));
            }
        }

        private static void LogIfError(int ierr, string exceptionText)
        {
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                log.ErrorFormat(exceptionText + Resources.AGrid_ThrowIfError__because_of_error_number___0_, ierr);
            }
        }

        private void CleanUp()
        {
            if (disposed)
            {
                return;
            }

            if (GridApi != null)
            {
                int ierr = GridApiDataSet.GridConstants.NOERR;

                if (IsInitialized())
                {
                    ierr = GridApi.Close();
                }

                LogIfError(ierr, Resources.AGrid_CleanUp_Couldn_t_close_grid_nc_file);

                var disposableGridApi = GridApi as IDisposable;
                disposableGridApi?.Dispose();
                GridApi = null;
            }

            disposed = true;
        }

        ~AGrid()
        {
            CleanUp();
        }
    }
}