
namespace DelftTools.Hydro.Structures
{
    public static class KnownStructureProperties
    {
        #region General structure properties
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
        public const string GateSillLevel = "sill_level";
        public const string GateOpeningWidth = "opening_width";
        public const string GateLowerEdgeLevel = "lower_edge_level";
        public const string GateDoorHeight = "door_height";
        public const string GateHorizontalOpeningDirection = "horizontal_opening_direction";
        public const string GateSillWidth = "sill_width";
        #endregion Gate
    }
}
