using System;
using System.IO;
using System.Threading;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Grid
{
    [Serializable]
    public class UGridGlobalMetaData
    {
        public UGridGlobalMetaData()
        {
            Modelname = "Unknown model";
            Source = "Unknown Source";
            Version = "-";
        }
        public UGridGlobalMetaData(string modelName, string source, string version)
        {
            Modelname = modelName;
            Source = source;
            Version = version;
        }
        public string Modelname { get; private set; }

        public string Source { get; private set; }

        public string Version { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            var gmd = (UGridGlobalMetaData) obj;
            return Modelname == gmd.Modelname && Source == gmd.Source && Version == gmd.Version;
        }

        public override int GetHashCode()
        {
            return Modelname.GetHashCode();
        }
    }

    public abstract class AGrid : IGrid
    {
        protected readonly string filename;
        private readonly GridApiDataSet.NetcdfOpenMode mode;
        private bool disposed;

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

        public virtual IGridApi GridApi { get; set; }
        public virtual ICoordinateSystem CoordinateSystem { get; private set; }
        public UGridGlobalMetaData GlobalMetaData { get; private set; }

        public virtual bool IsValid()
        {
            return GridApi != null && (GridApi.GetConvention() == GridApiDataSet.DataSetConventions.IONC_CONV_UGRID &&
                                       GridApi.GetVersion() >= GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION);
        }

        public virtual void CreateFile()
        {
            if (filename != null && !File.Exists(filename))
            {
                var ierr = GridApi.CreateFile(filename, GlobalMetaData);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new Exception("Couldn't create new NetCDF file at location " + filename + " because of error number " + ierr); 
                }
            }
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
                var ierr = GridApi.Open(filename, mode);
                if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                {
                    throw new Exception("Couldn't open grid nc file: " + filename + " because of error number: " + ierr);
                }
            
                if (!GridApi.Initialized)
                    throw new Exception("Couldn't open grid nc file : " + filename);
            
                try
                {
                    int epsg_code;
                    ierr = GridApi.GetCoordinateSystemCode(out epsg_code);
                    if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                    {
                        CoordinateSystem = null;
                        throw new Exception("Couldn't get coordinate system code because of err nr : " + ierr);
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

        public virtual GridApiDataSet.DataSetConventions GetDataSetConvention()
        {
            if (GridApi != null) return GridApi.GetConvention();
            return GridApiDataSet.DataSetConventions.IONC_CONV_OTHER;
        }

        public virtual bool IsInitialized()
        {
            return GridApi != null && GridApi.Initialized;
        }

        public int GetNumberOfNetworks()
        {
            if (GridApi == null) throw new InvalidOperationException("Communication with netCDF file was unsuccessful, API is not set"); ; 
            int numberOfNetworks;
            var ierr = GridApi.GetNumberOfNetworks(out numberOfNetworks);
            ThrowIfError(ierr, "Couldn't get the number of networks");
            return numberOfNetworks;
        }

        public int[] GetNetworkIds()
        {
            if (GridApi == null) throw new InvalidOperationException("Communication with netCDF file was unsuccessful, API is not set");

            int[] networkIds;
            var ierr = GridApi.GetNetworkIds(out networkIds);
            ThrowIfError(ierr, "Couldn't get the network ids");
            return networkIds;
        }

        protected T GetValidGridApi<T>(string errormessage) where T: class
        {
            if (!IsInitialized()) Initialize();
            var uGridApi = GridApi as T;
            var isValid = uGridApi != null && IsValid();
            if (!isValid)
                throw new Exception(errormessage + ", because the API was not instantiated.");
            return uGridApi;
        }

        protected void ThrowIfError(int ierr, string exceptionText)
        {
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new Exception(string.Format(exceptionText + " because of error number: {0}", ierr));
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
            if (!IsInitialized())
            {
                disposed = true;
                return;
            }
            try
            {
                if (GridApi != null)
                {
                    var ierr = GridApi.Close();
                    if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
                    {
                        throw new Exception("Couldn't close grid nc file because of err nr : " + ierr);
                    }
                    GridApi = null;
                }
            }
            catch
            {
                // ignored
            }
            disposed = true;
            Thread.Sleep(100); // wait for process to truly exit
        }
    }
}