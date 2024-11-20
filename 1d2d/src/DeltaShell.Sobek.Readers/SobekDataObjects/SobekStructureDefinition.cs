namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public enum SobekStructureType
    {
        riverWeir = 0,
        riverAdvancedWeir = 1,
        generalStructure = 2,
        riverPump = 3,
        weir = 6,
        orifice = 7,
        pump = 9,
        culvert = 10,
        universalWeir = 11,
        bridge =12
    }

    public class SobekStructureDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public ISobekStructureDefinition Definition { get; set; }
    }
}