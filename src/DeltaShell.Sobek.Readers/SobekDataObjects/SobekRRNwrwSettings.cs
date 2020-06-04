namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRNwrwSettings
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double[] RunoffDelayFactors { get; set; } // rf
        public double[] RunoffDelayFactorsOldTag { get; set; } // ru
        public double [] MaximumStorages { get; set; }
        public double[] MaximumInfiltrationCapcaties { get; set; }
        public double[] MinimumInfiltrationCapcaties { get; set; }
        public double[] InfiltrationCapacityDecreases { get; set; }
        public double[] InfiltrationCapacityIncreases { get; set; }
        public bool InfiltrationFromDepressions { get; set; }
        public bool InfiltrationFromRunoff { get; set; }


    }
}
