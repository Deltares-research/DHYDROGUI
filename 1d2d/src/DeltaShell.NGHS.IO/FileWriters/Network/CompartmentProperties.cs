using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public class CompartmentProperties
    {
        public string CompartmentId { get; set; }

        public string Name { get; set; }

        public string NodeId { get; set; }

        public string ManholeId { get; set; }

        public bool UseTable { get; set; }

        public double BedLevel { get; set; }

        public double Area { get; set; }

        public double StreetLevel { get; set; }

        public double StreetStorageArea { get; set; }

        public CompartmentShape CompartmentShape { get; set; }

        public CompartmentStorageType CompartmentStorageType { get; set; }

        public int NumberOfLevels { get; set; }

        public double[] Levels { get; set; }

        public double[] StorageAreas { get; set; }

        public InterpolationType Interpolation { get; set; } = InterpolationType.Linear;
    }
}