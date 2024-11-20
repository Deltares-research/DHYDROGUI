namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekUniversalWeir : ISobekStructureDefinition
    {
        public string CrossSectionId { get; set; }
        public float CrestLevelShift { get; set; }
        public float DischargeCoefficient { get; set; }
        
        public int FlowDirection { get; set; }
    }
}