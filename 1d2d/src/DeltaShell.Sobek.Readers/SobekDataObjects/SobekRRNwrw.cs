namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRNwrw : ISobekCatchment
    {
        public string Id { get; set; }
        public double Area { get; set; }
        public double SurfaceLevel { get; set; }
        public double[] Areas { get; set; }
        public string InhabitantDwaId { get; set; }
        public string CompanyDwaId { get; set; }
        public string MeteoStationId { get; set; }
        public double NumberOfPeople { get; set; }
        public double NumberOfUnits { get; set; }
        public string[] SpecialAreaNames { get; set; }
        public double[] SpecialAreaValues { get; set; }
    }
}
