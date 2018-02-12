using System;
using System.IO;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Grid
{
    public abstract class AGrid<T> : IDisposable where T : class, IGridApi
    {
        protected readonly string filename;
        private readonly GridApiDataSet.NetcdfOpenMode mode;
        private bool disposed;
        private T gridApi;

        public AGrid()
        {
            GlobalMetaData = new UGridGlobalMetaData();
        }

        public AGrid(string filename, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : this()
        {
            this.filename = filename;
            this.mode = mode;
        }

        public AGrid(string filename, UGridGlobalMetaData globalMetaData, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : this(filename, mode)
        {
            if(GlobalMetaData != null) GlobalMetaData = globalMetaData;
        }

        public virtual T GridApi
        {
            get { return gridApi; }
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
            return GridApi != null && (GridApi.GetConvention() == GridApiDataSet.DataSetConventions.CONV_UGRID &&
                                       GridApi.GetVersion() >= GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION);
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
                var ierr = GridApi.Open(filename, mode);
                ThrowIfError(ierr, errorMessage);
            
                try
                {
                    int epsg_code;
                    ierr = GridApi.GetCoordinateSystemCode(out epsg_code);
                    if (ierr != GridApiDataSet.GridConstants.NOERR)
                    {
                        CoordinateSystem = null;
                        throw new Exception(Resources.AGrid_Initialize_Couldn_t_get_coordinate_system_code_because_of_err_nr___ + ierr);
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
                var ierr = GridApi.CreateFile(filename, GlobalMetaData);
                ThrowIfError(ierr, Resources.AGrid_CreateFile_Couldn_t_create_new_NetCDF_file_at_location_ + filename);
            }
        }

        public virtual GridApiDataSet.DataSetConventions GetDataSetConvention()
        {
            if(!IsInitialized()) Initialize();
            return GridApi.GetConvention();
        }

        public int GetNumberOfNetworks()
        {
            if (!IsInitialized()) Initialize(); 
            int numberOfNetworks;
            var ierr = GridApi.GetNumberOfNetworks(out numberOfNetworks);
            ThrowIfError(ierr, Resources.AGrid_Couldn_t_get_the_number_of_networks);
            return numberOfNetworks;
        }

        public int[] GetNetworkIds()
        {
            if (!IsInitialized()) Initialize();
            int[] networkIds;
            var ierr = GridApi.GetNetworkIds(out networkIds);
            ThrowIfError(ierr, Resources.AGrid_Couldn_t_get_the_network_ids);
            return networkIds;
        }

        protected T GetValidGridApi(string errormessage)
        {
            if (!IsInitialized()) Initialize();
            var uGridApi = GridApi as T;
            var isValid = uGridApi != null && IsValid();
            if (!isValid)
                throw new Exception(errormessage + Resources.AGrid___because_the_API_was_not_instantiated_);
            return uGridApi;
        }

        protected void DoWithValidGridApi(Func<T, int> function, string errorMessage)
        {
            var uGridNetworkApi = GetValidGridApi(errorMessage);
            var ierr = function(uGridNetworkApi);
            ThrowIfError(ierr, errorMessage);
        }

        protected void DoWithValidGridApi(Action<T> action, string errorMessage)
        {
            var uGridApi = GetValidGridApi(errorMessage);
            action(uGridApi);
        }

        protected TValue GetFromValidGridApi<TValue>(Func<T,TValue> function, TValue defaultValue, string errorMessage)
        {
            var uGridApi = GetValidGridApi(errorMessage);
            return uGridApi != null ? function(uGridApi) : defaultValue;
        }

        protected void ThrowIfError(int ierr, string exceptionText)
        {
            if (ierr != GridApiDataSet.GridConstants.NOERR)
            {
                throw new Exception(string.Format(exceptionText + Resources.AGrid_ThrowIfError__because_of_error_number___0_, ierr));
            }
        }

        public virtual void Dispose()
        {
            if (disposed) return;
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

        ~AGrid()
        {
            CleanUp();
        }
        

        private void CleanUp()
        {
            if (disposed) return;

            try
            {
                if (GridApi != null)
                {
                    var ierr = GridApiDataSet.GridConstants.NOERR;

                    if (IsInitialized())
                    {
                        ierr = GridApi.Close();
                    }

                    ThrowIfError(ierr, Resources.AGrid_CleanUp_Couldn_t_close_grid_nc_file);
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                // dispose GridApi if IDisposable
                var disposableGridApi = GridApi as IDisposable;
                if (disposableGridApi != null)
                {
                    disposableGridApi.Dispose();
                }
                GridApi = null;
            }

            disposed = true;
        }
    }
}