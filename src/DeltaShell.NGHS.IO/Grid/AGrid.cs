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
        private readonly string filename;
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
            if (filename != null && !File.Exists(filename)) GridApi.CreateFile(filename, GlobalMetaData);
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
                GridApi.Open(filename, mode);
            
                if (!GridApi.Initialized)
                    throw new Exception("Couldn't open grid nc file : " + filename);
            
                try
                {
                    int epsg_code = GridApi.GetCoordinateSystemCode();
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
                    GridApi.Close();
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