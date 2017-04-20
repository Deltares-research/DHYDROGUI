using System;
using System.Threading;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Grid
{
    public abstract class AGrid : IGrid, IDisposable
    {
        protected IGridApi GridApi;
        private bool disposed;

        public abstract bool IsValid();

        public virtual void Initialize(string filename, GridApiDataSet.NetcdfOpenMode mode)
        {
            if (IsInitialized())
            {
                CleanUp();
            }
            GridApi.Open(filename, mode);
            
            if (!GridApi.Initialized())
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

        public virtual GridApiDataSet.DataSetConventions GetDataSetConvention()
        {
            return GridApi.GetConvention();
        }

        public virtual bool IsInitialized()
        {
            return GridApi.Initialized();
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
            if (!IsInitialized()) return;
            try
            {
                GridApi.Close();
                GridApi.Dispose();
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