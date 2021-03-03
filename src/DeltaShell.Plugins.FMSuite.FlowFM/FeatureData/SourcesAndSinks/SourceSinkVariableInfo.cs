namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks
{
    /// <summary>
    /// Contains constant variable descriptions contained in a <see cref="SourceAndSinkFunction"/>.
    /// </summary>
    public static class SourceSinkVariableInfo
    {
        public const string TimeVariableName = "Time";

        public const string DischargeVariableName = "Discharge";
        public const string DischargeUnitDescription = "cubic meters per second";
        public const string DischargeUnitSymbol = "m3/s";

        public const string SalinityVariableName = "Salinity";
        public const string SalinityUnitDescription = "parts per trillion";
        public const string SalinityUnitSymbol = "ppt";

        public const string TemperatureVariableName = "Temperature";
        public const string TemperatureUnitDescription = "degree celsius";
        public const string TemperatureUnitSymbol = "°C";

        public const string SedimentFractionUnitDescription = "";
        public const string SedimentFractionUnitSymbol = "";

        public const string SecondaryFlowVariableName = "Secondary Flow";
        public const string SecondaryFlowUnitDescription = "meters per second";
        public const string SecondaryFlowUnitSymbol = "m/s";

        public const string TracersUnitDescription = "kilograms per cubic meter";
        public const string TracerUnitSymbol = "kg/m3";
    }
}