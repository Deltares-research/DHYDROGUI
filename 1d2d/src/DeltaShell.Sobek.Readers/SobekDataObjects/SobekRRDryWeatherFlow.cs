namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRDryWeatherFlow
    {
        public SobekRRDryWeatherFlow()
        {
            SaltConcentration = 400.0;
        }
        public string Id { get; set; }

        public string Name { get; set; }

        public DWAComputationOption ComputationOption { get; set; }

        public double WaterUsePerHourForConstant { get; set; }

        public double WaterUsePerDayForVariable { get; set; }

        public double[] WaterCapacityPerHour { get; set; }

        public double SaltConcentration { get; set; }
    }

    public enum DWAComputationOption
    {
        NrPeopleTimesConstantPerHour = 1,
        NrPeopleTimesVariablePerHour = 2,
        ConstantDWAPerHour = 3,
        VariablePerHour = 4,
        UseTable = 5
    }
}