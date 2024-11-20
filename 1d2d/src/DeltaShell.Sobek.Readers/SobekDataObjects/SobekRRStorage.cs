namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRStorage
    {
        public SobekRRStorage()
        {
            MaxLandStorage = 1.0;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public double MaxLandStorage { get; set; }

        public double InitialLandStorage { get; set; }

        public double MaxStreetStorage { get; set; }

        public double InitialStreetStorage { get; set; }

        public double MaxSewerStorageMixedRainfall { get; set; }

        public double InitialSewerStorageMixedRainfall { get; set; }

        public double MaxSewerStorageDWA { get; set; }

        public double InitialSewerStorageDWA { get; set; }

        public double MaxRoofStorage { get; set; }

        public double InitialRoofStorage { get; set; }
    }
}