namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRNwrwSettings : ISobekRRNwrwSettings
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double[] RunoffDelayFactors { get; set; }
        public double [] MaximumStorages { get; set; }
        public double[] MaximumInfiltrationCapacities { get; set; }
        public double[] MinimumInfiltrationCapacities { get; set; }
        public double[] InfiltrationCapacityDecreases { get; set; }
        public double[] InfiltrationCapacityIncreases { get; set; }
        public bool InfiltrationFromDepressions { get; set; }
        public bool InfiltrationFromRunoff { get; set; }
        public bool IsOldFormatData { get; set; }
    }
}
