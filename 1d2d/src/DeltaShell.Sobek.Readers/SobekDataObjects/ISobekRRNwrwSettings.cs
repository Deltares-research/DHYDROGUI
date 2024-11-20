namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public interface ISobekRRNwrwSettings
    {
        double[] RunoffDelayFactors { get; }
        double[] MaximumStorages { get; }
        double[] MaximumInfiltrationCapacities { get; }
        double[] MinimumInfiltrationCapacities { get; }
        double[] InfiltrationCapacityDecreases { get; }
        double[] InfiltrationCapacityIncreases { get; }
        bool InfiltrationFromDepressions { get; }
        bool InfiltrationFromRunoff { get; }

        /// <summary>
        /// Indicator to determine whether the data is based on new tag (rf)
        /// or old tag (ru) for RunoffDelayFactors.
        /// </summary>
        bool IsOldFormatData { get; }
    }
}