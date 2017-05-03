using System;
using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IGrid : IDisposable
    {
        void Initialize(string filename, GridApiDataSet.NetcdfOpenMode model);
        bool IsValid();
        GridApiDataSet.DataSetConventions GetDataSetConvention();
        bool IsInitialized();
        ICoordinateSystem CoordinateSystem { get; }
        IGridApi GridApi { get; set; }
    }
}