using GeoAPI.Extensions.CoordinateSystems;

namespace DelftTools.Hydro
{
    public class ModelSettings
    {
        public string ModelName { get; set; } = "FM_model";

        public ICoordinateSystem CoordinateSystem { get; set; }
    }
}