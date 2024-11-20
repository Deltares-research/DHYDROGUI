namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRSacramento : ISobekCatchment
    {
        public SobekRRSacramento()
        {
            AreaAdjustmentFactor = 1;
        }

        public double Area { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CapacityId { get; set; }
        public string UnitHydrographId { get; set; }
        public string OtherParamsId { get; set; }
        public string MeteoStationId { get; set; }
        public double AreaAdjustmentFactor { get; set; }
    }
}
