using System;
using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IGrid : IDisposable
    {
        void CreateFile();
        void Initialize();
        bool IsValid();
        GridApiDataSet.DataSetConventions GetDataSetConvention();
        bool IsInitialized();
        ICoordinateSystem CoordinateSystem { get; }
        IGridApi GridApi { get; set; }
        UGridGlobalMetaData GlobalMetaData { get; }
    }
}