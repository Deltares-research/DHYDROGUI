namespace DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties
{
    public static class KnownStructureProperties
    {
        #region Common structure properties

        public const string Type = "type";
        public const string Name = "id";
        public const string X = "x";
        public const string Y = "y";
        public const string PolylineFile = "polylinefile";

        #endregion

        #region Weir properties

        public const string CrestLevel = "CrestLevel";
        public const string CrestWidth = "CrestWidth";
        public const string LateralContractionCoefficient = "lat_contr_coeff";

        #endregion

        #region Pump

        public const string Capacity = "capacity";
        public const string NrOfReductionFactors = "reduction_factor_no_levels";
        public const string ReductionFactor = "reduction_factor";
        public const string Head = "head";

        #endregion

        #region Gate

        public const string GateOpeningWidth = "GateOpeningWidth";
        public const string GateLowerEdgeLevel = "GateLowerEdgeLevel";
        public const string GateHeight = "GateHeight";
        public const string GateOpeningHorizontalDirection = "GateOpeningHorizontalDirection";

        #endregion Gate
    }
}