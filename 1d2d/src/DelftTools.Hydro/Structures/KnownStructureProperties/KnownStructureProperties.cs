namespace DelftTools.Hydro.Structures.KnownStructureProperties
{
    public static partial class KnownStructureProperties
    {
        #region Common structure properties
        public const string Type = "type";
        public const string Name = "id";
        public const string X = "x";
        public const string Y = "y";
        public const string PolylineFile = "polylinefile";
        #endregion

        #region Weir properties
        public const string CrestLevel = "crest_level";
        public const string CrestWidth = "crest_width";
        public const string LateralContractionCoefficient = "lat_contr_coeff";
        #endregion

        #region Pump
        public const string Capacity = "capacity";
        public const string StartSuctionSide = "start_level_suction_side";
        public const string StopSuctionSide = "stop_level_suction_side";
        public const string StartDeliverySide = "start_level_delivery_side";
        public const string StopDeliverySide = "stop_level_delivery_side";
        public const string NrOfReductionFactors = "reduction_factor_no_levels";
        public const string ReductionFactor = "reduction_factor";
        public const string Head = "head";
        #endregion

        #region Gate
        public const string GateSillLevel = "sillLevel";
        public const string GateCrestLevel = "crestLevel";
        public const string GateOpeningWidth = "gateOpeningWidth";
        public const string GateLowerEdgeLevel = "gateLowerEdgeLevel";
        public const string GateDoorHeight = "gateHeight";
        public const string GateHorizontalOpeningDirection = "gateOpeningHorizontalDirection";
        public const string GateSillWidth = "sill_width";
        #endregion Gate

        #region Levee breach

        public const string BreachLocationX = "StartLocationX";
        public const string BreachLocationY = "StartLocationY";
        public const string Algorithm = "Algorithm";
        public const string InitialCrestLevel = "CrestLevelIni";
        public const string InitalBreachWidth = "BreachWidthIni";
        public const string MinimumCrestLevel = "CrestLevelMin";
        public const string TimeToReachMinimumCrestLevel = "TimeToBreachToMaximumDepth";
        public const string Factor1 = "F1";
        public const string Factor2 = "F2";
        public const string CriticalFlowVelocity = "Ucrit";
        public const string StartTimeBreachGrowth = "T0";
        public const string BreachGrowthActivated = "State";
        public const string TimeFilePath = "DambreakLevelsAndWidths";

        #endregion
    }
}