using System;
using System.Threading;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Grid
{
    public abstract class AGrid : IGrid
    {
        public virtual IGridApi GridApi { get; set; }
        private bool disposed;
        

        public virtual bool IsValid()
        {
            return GridApi != null && (GridApi.GetConvention() == GridApiDataSet.DataSetConventions.IONC_CONV_UGRID &&
                                       GridApi.GetVersion() >= GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION);
        }

        public virtual void Initialize(string filename, GridApiDataSet.NetcdfOpenMode mode)
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

        public virtual ICoordinateSystem CoordinateSystem { get; private set; }
        
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