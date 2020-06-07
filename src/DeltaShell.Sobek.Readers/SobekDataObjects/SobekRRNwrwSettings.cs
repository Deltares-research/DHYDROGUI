namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRNwrwSettings
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double[] RunoffDelayFactors { get; set; }
        public double [] MaximumStorages { get; set; }
        public double[] MaximumInfiltrationCapcaties { get; set; }
        public double[] MinimumInfiltrationCapcaties { get; set; }
        public double[] InfiltrationCapacityDecreases { get; set; }
        public double[] InfiltrationCapacityIncreases { get; set; }
        public bool InfiltrationFromDepressions { get; set; }
        public bool InfiltrationFromRunoff { get; set; }

        /// <summary>
        /// Indicator to determine whether the data is based on new tag (rf)
        /// or old tag (ru) for RunoffDelayFactors.
        /// </summary>
        public bool IsOldFormatData { get; set; }
    }
}
