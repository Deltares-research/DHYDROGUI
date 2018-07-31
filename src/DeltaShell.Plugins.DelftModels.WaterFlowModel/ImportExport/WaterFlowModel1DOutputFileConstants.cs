
namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class WaterFlowModel1DOutputFileConstants
    {
        public static class FileNames
        {
            public const string GridPointsFile = "gridpoints.nc";
            public const string LateralsFile = "laterals.nc";
            public const string ObservationsFile = "observations.nc";
            public const string ReachSegmentsFile = "reachsegments.nc";
            public const string RetentionsFile = "retentions.nc";
            public const string StructuresFile = "structures.nc";
            public const string PumpsFile = "pumps.nc";
            public const string WaterBalanceFile = "waterbalance.nc";
        }

        public static class VariableNames
        {
            public const string NegativeDepthCount = "negative_depth_count";
            public const string NoInteration = "no_solution_iterations";
            public const string WaterDensity = "water_density";
            public const string WaterDepth = "water_depth";
            public const string WaterLateralFlow2D1D = "water_lateral_flow_2d1d";
            public const string WaterLateralFlowAtNode = "water_lateral_flow_at_node";
            public const string WaterLevel = "water_level";
            public const string WaterSalinity = "water_salinity";
            public const string WaterTotalArea = "water_total_area";
            public const string WaterTotalWidth = "water_total_width";
            public const string WaterVolume = "water_volume";
            public const string WaterDischarge = "water_discharge";
            public const string WaterDispersion = "water_dispersion";
            public const string WaterVelocity = "water_velocity";
            public const string Froude = "froude";
            public const string TimeStepEstimation = "time_step_estimation";
            public const string WaterChezy = "water_chezy";
            public const string WaterChezyFP1 = "water_chezy_fp1";
            public const string WaterChezyFP2 = "water_chezy_fp2";
            public const string WaterChezyMain = "water_chezy_main";
            public const string WaterConveyance = "water_conveyance";
            public const string WaterDischargeFP1 = "water_discharge_fp1";
            public const string WaterDischargeFP2 = "water_discharge_fp2";
            public const string WaterDischargeMain = "water_discharge_main";
            public const string WaterEnergyLevel = "water_energy_level";
            public const string WaterFlowArea = "water_flow_area";
            public const string WaterFlowAreaFP1 = "water_flow_area_fp1";
            public const string WaterFlowAreaFP2 = "water_flow_area_fp2";
            public const string WaterFlowAreaMain = "water_flow_area_main";
            public const string WaterFlowWidthFP1 = "water_flow_width_fp1";
            public const string WaterFlowWidthFP2 = "water_flow_width_fp2";
            public const string WaterFlowWidthMain = "water_flow_width_main";
            public const string WaterHydraulicRadius = "water_hydraulic_radius";
            public const string WaterHydraulicRadiusFP1 = "water_hydraulic_radius_fp1";
            public const string WaterHydraulicRadiusFP2 = "water_hydraulic_radius_fp2";
            public const string WaterHydraulicRadiusMain = "water_hydraulic_radius_main";
            public const string WaterLevelGradient = "water_level_gradient";
            public const string PressureDifference = "pressure_difference";
            public const string StructureCrestLevel = "structure_crest_level";
            public const string StructureCrestWidth = "structure_crest_width";
            public const string StructureGateLowerEdgeLevel = "structure_gate_lower_edge_level";
            public const string StructureGateOpeningHeight = "structure_gate_opening_height";
            public const string StructureSetPoint = "structure_set_point";
            public const string StructureValveOpening = "structure_valve_opening";
            public const string StructureWaterHead = "structure_water_head";
            public const string StructureWaterLevelAtCrest = "structure_water_level_at_crest";
            public const string StructureWaterLevelDown = "structure_water_level_down";
            public const string StructureWaterLevelUp = "structure_water_level_up";
            public const string PumpSuctionSide = "suction_side_level";
            public const string PumpDeliverySide = "delivery_side_level";
            public const string PumpHead = "pump_head";
            public const string PumpStage = "actual_pump_stage";
            public const string PumpReductionFactor = "reduction_factor";
            public const string PumpCapacity = "pump_capacity";
            public const string PumpDischarge = "pump_discharge";
            public const string WaterBalance2D1DIn = "water_balance_2d1d_in";
            public const string WaterBalance2D1DOut = "water_balance_2d1d_out";
            public const string WaterBalance2D1DTotal = "water_balance_2d1d_total";
            public const string WaterBalanceBoundariesIn = "water_balance_boundaries_in";
            public const string WaterBalanceBoundariesOut = "water_balance_boundaries_out";
            public const string WaterBalanceBoundariesTotal = "water_balance_boundaries_total";
            public const string WaterBalanceError = "water_balance_error";
            public const string WaterBalanceLateralIn = "water_balance_lateral_in";
            public const string WaterBalanceLateralOut = "water_balance_lateral_out";
            public const string WaterBalanceLateralTotal = "water_balance_lateral_total";
            public const string WaterBalanceStorage = "water_balance_storage";
            public const string WaterBalanceVolume = "water_balance_volume";
            public const string WaterTemperature = "water_temperature";

            public const string EffectiveBackRad = "effective_background_radiation";
            public const string HeatlossForcedConv = "heatloss_forced_convection";
            public const string HeatlossForcedEvap = "heatloss_forced_evaporation";
            public const string HeatlossFreeConv = "heatloss_free_convection";
            public const string HeatlossFreeEvap = "heatloss_free_evaporation";
            public const string HeatLossConv = "heat_loss_convection";
            public const string HeatLossEvap = "heatloss_evaporation";
            public const string NetSolarRad = "netto_solar_radiation";
            public const string RadFluxClearSky = "rad_flux_clear_sky";
            public const string TotalHeatFlux = "total_heat_flux";

            public const string BranchId = "branchid";
            public const string Chainage = "chainage";
            public const string XCoordinate = "x_coordinate";
            public const string YCoordinate = "y_coordinate";
            public const string Time = "time";
        }

        public static class AttributeKeys
        {
            public const string CfRole = "cf_role";
            public const string Units = "units";
            public const string LongName = "long_name";
            public const string AggregationOption = "aggregation_option";
        }

        public static class AttributeValues
        {
            public const string CfRole = "timeseries_id";
        }

        public static class DimensionKeys
        {
            public const string Time = "time";    
        }
        
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string TimeVariableUnitValuePrefix = "seconds since";
    }
}