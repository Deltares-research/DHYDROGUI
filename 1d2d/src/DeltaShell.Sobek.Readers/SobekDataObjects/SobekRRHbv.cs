namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRHbv : ISobekCatchment
    {
        public SobekRRHbv()
        {
            AreaAdjustmentFactor = 1;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public double Area { get; set; }

        public double AreaAdjustmentFactor { get; set; }

        public double SurfaceLevel { get; set; }

        public string SoilId { get; set; }

        public string FlowId { get; set; }

        public string HiniId { get; set; }

        public string SnowId { get; set; }

        public string MeteoStationId { get; set; }

        public string TemperatureStationId { get; set; }
    }
}
