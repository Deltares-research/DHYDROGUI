namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRPaved: ISobekCatchment
    {
        public SobekRRPaved()
        {
            AreaAjustmentFactor = 1.0;
        }

        public string Id { get; set; }

        public double Area { get; set; }

        public double StreetLevel { get; set; }

        public string StorageId { get; set; }

        public SewerSystemType SewerSystem { get; set; }

        public double CapacitySewerConstantRainfallInM3S { get; set; }

        public double CapacitySewerConstantDWAInM3S { get; set; }

        public string CapacitySewerTableId { get; set; }

        /// <summary>
        /// The discharge target for mixed/rainfall sewer pumps.
        /// </summary>
        public SewerDischargeType MixedAndOrRainfallSewerPumpDischarge { get; set; }

        /// <summary>
        /// The discharge target for dry weather flow sewer pumps.
        /// </summary>
        public SewerDischargeType DryWeatherFlowSewerPumpDischarge { get; set; }

        public double SewerOverflowLevelRWAMixed { get; set; }

        public double SewerOverFlowLevelDWA { get; set; }

        public bool SewerInflowRWAMixed { get; set; }

        public bool SewerInflowDWA { get; set; }

        public string MeteoStationId { get; set; }

        public double InitialSaltConcentration { get; set; }

        public int NumberOfPeople { get; set; }

        public string DryWeatherFlowId { get; set; }

        public double AreaAjustmentFactor { get; set; }

        public SpillingOption SpillingOption { get; set; }

        public double SpillingRunoffCoefficient { get; set; }
        
        public string QHTableId { get; set; }
    }

    public enum SewerSystemType
    {
        Mixed = 0,
        Separated = 1,
        ImprovedSeparated = 2
    }

    //qo  =          1 1       =          both sewer pumps discharge to open water (=default)
    //                  0 0       =          both sewer pumps discharge to boundary
    //                  0 1       =          rainfall or mixed part of the sewer pumps to open water, 
    //                                          DWA-part (if separated) to boundary
    //                  1 0       =          rainfall or mixed part of the sewer discharges to boundary, 
    //                                          DWA-part (if separated) to open water
    //                  2 2       =          both sewer pumps discharge to WWTP
    //                  2 1       =          rainfall or mixed part of the sewer pumps to open water, 
    //                                          DWA-part (if separated) to WWTP
    public enum SewerDischargeType
    {
        BoundaryNode=0,
        WWTP=2
    }

    public enum SpillingOption
    {
        NoDelay = 0,
        UsingCoefficient = 1,
        UsingQHRelation =2
    }
}
