namespace DeltaShell.NGHS.IO.DataObjects.Model1D
{
    public static class Model1DParameterNames
    {

        /// <summary>
        /// Location parameter names
        /// </summary>        
        public const string LocationWaterLevel = "Water level";
        public const string LocationWaterDepth = "Water depth";
        public const string LocationSurfaceArea = "Surface area";
        
        public const string LocationVolume = "Water volume";
        public const string LocationTotalArea = "Total area";
        public const string LocationTotalWidth = "Total width";
        public const string LocationSaltConcentration = "Salt concentration";
        public const string LocationTemperature = "Temperature";
        public const string LocationDensity = "Density";
        public const string LocationQTotal_1d2d = "Lateral Discharge from 2d to 1d";
        public const string LocationLateralAtNodes = "Lateral at nodes";
        public const string LocationTotalHeatFlux = "Total heat flux";
        public const string LocationRadFluxClearSky = "Radiation flux for clear sky condition";
        public const string LocationHeatLossConv = "Heat loss due to convection";
        public const string LocationNetSolarRad = "Net incident solar radiation";
        public const string LocationEffectiveBackRad = "Effective back radiation";
        public const string LocationHeatLossEvap = "Heat loss due to evaporation";
        public const string LocationHeatLossForcedEvap = "Heat loss due to forced evaporation";
        public const string LocationHeatLossFreeEvap = "Heat loss due to free evaporation";
        public const string LocationHeatLossForcedConv = "Heat loss due to forced convection";
        public const string LocationHeatLossFreeConv = "Heat loss due to free convection";

        /// <summary>
        /// Branch parameter names
        /// </summary>        
        public const string BranchDischarge = "Discharge";
        public const string BranchVelocity = "Velocity";
        public const string BranchSaltDispersion = "Salt dispersion";
        public const string BranchEnergyHeadLevel = "Energy head";
        public const string BranchFlowArea = "Flow area";
        public const string BranchHydraulicRadius = "Hydraulic radius";
        public const string BranchConveyance = "Conveyance";
        public const string BranchRoughness = "Chezy values";
        public const string BranchWaterLevelGradient = "Water level gradient";
        public const string BranchFroudeNumber = "Froude number";
        public const string BranchSubsectionParameters = "Subsection parameters";

        /// <summary>
        /// subsectie parameters; only for coverages; in propertygrid BranchSubsectionParameters is used
        /// </summary>
        public const string MainChannel = "Main ";
        public const string FloodPlain1 = "FloodPlain1 ";
        public const string FloodPlain2 = "FloodPlain2 ";
        public const string SubSectionDischarge = "Discharge";
        public const string SubSectionFlowArea = "Flow area";
        public const string SubSectionFlowWidth = "Flow width";
        public const string SubSectionHydraulicRadius = "Hydraulic radius";
        public const string SubSectionRoughness = "Chezy values";

        /// <summary>
        /// Structure parameter names
        /// </summary>        
        public const string StructureDischarge = "Discharge (s)";
        public const string StructureVelocity = "Velocity (s)";
        public const string StructureFlowArea = "Flow area (s)";
        public const string StructurePressureDifference = "Pressure difference (s)";
        public const string StructureCrestLevel = "Crest level (s)";
        public const string StructureCrestWidth = "Crest width (s)";
        public const string StructureGateLevel = "Gate lower edge level (s)";
        public const string StructureGateOpeningWidth = "Gate opening width (s)";
        public const string StructureGateOpeningHorizontalDirection = "Gate horizontal direction (s)";
        public const string StructureOpeningHeight = "Gate height (s)";
        public const string StructureValveOpening = "Valve opening (s)";
        public const string StructureWaterlevelUp = "Water level up (s)";
        public const string StructureWaterlevelDown = "Water level down (s)";
        public const string StructureHeadDifference = "Head Difference (s)";
        public const string StructureWaterLevelAtCrest = "Water level at crest (s)";

        /// <summary>
        /// Pumps parameter names
        /// </summary>
        public const string PumpOutput = "All output (p)";
        public const string PumpSuctionSide = "Suction side (p)";
        public const string PumpDeliverySide = "Delivery side (p)";
        public const string PumpHead = "Pump head (p)";
        public const string PumpStage = "Pump stage (p)";
        public const string PumpReductionFactor = "Reduction factor (p)";
        public const string PumpCapacity = "Capacity (p)";
        public const string PumpDischarge = "Discharge (p)";

        /// <summary>
        /// Observation Point parameter names
        /// </summary>        
        public const string ObservationPointWaterLevel = "Water level (op)";
        public const string ObservationPointWaterDepth = "Water depth (op)";
        public const string ObservationPointSurfaceArea = "Surface area (op)";
        public const string ObservationPointDischarge = "Discharge (op)";
        public const string ObservationPointVelocity = "Velocity (op)";
        public const string ObservationPointSaltConcentration = "Salt concentration (op)";
        public const string ObservationPointSaltDispersion = "Salt dispersion (op)";
        public const string ObservationPointVolume = "Water volume (op)";
        public const string ObservationPointTemperature = "Temperature (op)";

        /// <summary>
        /// Retention parameter names
        /// </summary>
        public const string RetentionWaterLevel = "Water level (rt)";
        public const string RetentionVolume = "Volume (rt)";

        /// <summary>
        /// Lateral Source parameter names
        /// </summary>        
        public const string LateralActualDischarge = "Actual discharge (l)";
        public const string LateralDefinedDischarge = "Defined discharge (l)";
        public const string LateralDifference = "Lateral difference (l)";
        public const string LateralWaterLevel = "Water level (l)";

        /// <summary>
        /// Finite volume (Delwaq) parameter names
        /// </summary>
        public const string FiniteVolumeGridType = "Grid type (finite volume)";
        public const string FiniteVolumeDischarge = "Discharge (finite volume)";
        public const string FiniteVolumeVolume = "Volume (finite volume)";
        public const string FiniteVolumeVelocity = "Velocity (finite volume)";
        public const string FiniteVolumeSurface = "Surface (finite volume)";
        public const string FiniteVolumeChezy = "Chezy (finite volume)";
        public const string FiniteVolumeQLats = "Discharge lateral sources (finite volume)";

        /// <summary>
        /// SimulationInfo string constants
        /// </summary>        
        public const string SimulationInfoNegativeDepthDisplayName = "Negative depth";
        public const string SimulationInfoNumberOfIterationsDisplayName = "Number of iterations";
        public const string SimulationInfoTimeStepEstimationDisplayName = "Time step estimation";

        public const string SimulationInfoWaterBalanceTotalVolume = "Waterbalance1D_TotalVolume"; 
        public const string SimulationInfoWaterBalanceVolumeError = "Waterbalance1D_VolumeError";
        public const string SimulationInfoWaterBalanceTotalStorage = "Waterbalance1D_Storage";

        public const string SimulationInfoWaterBalanceBoundariesIn = "Waterbalance1D_Boundaries_In";
        public const string SimulationInfoWaterBalanceBoundariesOut = "Waterbalance1D_Boundaries_Out";
        public const string SimulationInfoWaterBalanceBoundariesTotal = "Waterbalance1D_Boundaries_Total";

        public const string SimulationInfoWaterBalanceLateralDischargeIn = "Waterbalance1D_LateralDischarge_In";
        public const string SimulationInfoWaterBalanceLateralDischargeOut = "Waterbalance1D_LateralDischarge_Out";
        public const string SimulationInfoWaterBalanceLateralDischargeTotal = "Waterbalance1D_LateralDischarge_Total";

        public const string SimulationInfoWaterBalanceLateral1D2DDischargeIn    = "Waterbalance1D_Lateral1D2DDischarge_In";
        public const string SimulationInfoWaterBalanceLateral1D2DDischargeOut   = "Waterbalance1D_Lateral1D2DDischarge_Out";
        public const string SimulationInfoWaterBalanceLateral1D2DDischargeTotal = "Waterbalance1D_Lateral1D2DDischarge_Total";
    }
}