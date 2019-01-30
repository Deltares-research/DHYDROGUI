using DelftTools.Hydro;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.FileReaders.Retention
{
/// <summary>
/// This class serves as a DTO for the Retention properties.
/// The class holds the properties of Retention during reading and sets the values when converting Retention. 
/// </summary>
    public class RetentionPropertiesDTO
    {
        public string Id { get; set; }
        public string LongName { get; set; }
        public string BranchName { get; set; }
        public IChannel Branch { get; set; }
        public double Chainage { get; set; }
        public RetentionType StorageType { get; set; }
        public bool UseTable { get; set; }
        public double BedLevel { get; set; }
        public double StreetLevel { get; set; }
        public double StorageArea { get; set; }
        public double StreetStorageArea { get; set; }
        public double ResultingChainage { get; set; }
        public Coordinate Coordinate { get; set; }
        public Point Geometry { get; set; }
    }
}
