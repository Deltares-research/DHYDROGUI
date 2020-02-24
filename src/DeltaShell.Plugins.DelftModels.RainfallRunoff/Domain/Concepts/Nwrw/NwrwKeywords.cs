namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwKeywords
    {
        // general
        public const string IdKey = "id";
        public const string NameKey = "nm";

        // .3b
        public const string NwrwOpeningKey = "NWRW";
        public const string NwrwClosingKey = "nwrw";
        public const string SurfaceLevelKey = "sl";
        public const string AreaKey = "ar";
        public const string FirstNumberOfUnitsKey = "np";
        public const string FirstDryWeatherFlowIdKey = "dw";
        public const string SecondNumberOfUnitsKey = "np2";
        public const string SecondDryWeatherFlowIdKey = "dw2";
        public const string MeteostationIdKey = "ms";
        public const string NumberOfSpecialAreasKey = "na";
        public const string SpecialAreaKey = "aa";
        public const string SpecialInflowReferenceKey = "nw";
        public const string AreaAdjustmentFactorKey = "aaf";

        // .alg
        public const string RunoffDelayFactor = "rf";
        public const string MaximumStorage = "ms";
        public const string MaximumInfiltrationCapacity = "ix";
        public const string MinimumInfiltrationCapacity = "im";
        public const string DecreaseInInfiltrationCapacity = "ic";
        public const string IncreaseInInfiltrationCapacity = "dc";
        public const string InfiltrationFromDepressions = "od";
        public const string InfiltrationFromRunoff = "or";

        // .dwa
        public const string DwaOpeningKey = "DWA";
        public const string DwaClosingKey = "dwa";
        public const string DwaComputationOptionKey = "do";
        public const string DwaWaterUsePerCapitaConstantValuePerHourKey = "wc";
        public const string DwaWaterUsePerCapitaPerDayKey = "wd";
        public const string DwaWaterUsePerCapitaPerHourKey = "wh";
        public const string DwaTableIdKey = "dt";

        // .tp
        public const string TpOpeningKey = "NODE";
        public const string TpBranchIdKey = "ri";
        public const string TpModelNodeType = "mt";
        public const string TpNetterNodeType = "nt";
        public const string TpObjectId = "ObID";
        public const string TpPositionX = "px";
        public const string TpPositionY = "py";
        public const string TpClosingKey = "node";
    }
}
