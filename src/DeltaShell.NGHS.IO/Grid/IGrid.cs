using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Grid
{
    public interface IGrid
    {
        void Initialize(string filename, GridApiDataSet.NetcdfOpenMode mode);
        bool IsValid();
        GridApiDataSet.DataSetConventions GetDataSetConvention();
        bool IsInitialized();
        ICoordinateSystem CoordinateSystem { get; }
    }
}