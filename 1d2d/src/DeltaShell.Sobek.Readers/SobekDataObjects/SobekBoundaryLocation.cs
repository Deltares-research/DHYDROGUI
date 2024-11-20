namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekBoundaryLocation
    {
        public string Id { get; set;}
        public string Name { get; set; }
        public string ConnectionId { get; set; }
        public double Offset { get; set; }
        public SobekBoundaryLocationType SobekBoundaryLocationType { get; set; }
    }
}
