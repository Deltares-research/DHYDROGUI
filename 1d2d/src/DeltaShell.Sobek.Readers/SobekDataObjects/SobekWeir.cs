namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekWeir : ISobekStructureDefinition
    {
        public float CrestWidth { get; set; }
        public float CrestLevel { get; set; }
        public float DischargeCoefficient { get; set; }
        public float LateralContractionCoefficient { get; set; }
        public int FlowDirection { get; set; }
    }
}