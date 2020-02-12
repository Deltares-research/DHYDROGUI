using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class FmModelSettings
    {
        public string ModelName { get; set; } = "FM model";

        public ICoordinateSystem CoordinateSystem { get; set; }
    }
}