using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.NGHS.IO.DataObjects.Model1D
{
    public static class EngineParameters
    {
        /// <summary>
        /// Returns the parameters the 'Sobek' engine supports. 
        /// If the unit is dimensionless, both name and symbol are set to ""
        /// </summary>
        /// <returns></returns>
        public static IEventedList<EngineParameter> EngineMapping()
        {
            return new EventedList<EngineParameter>
            {
                // Grid points
                new EngineParameter(QuantityType.WaterLevel, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationWaterLevel,
                    new Unit("meter above reference level", "m AD")),
                new EngineParameter(QuantityType.WaterDepth, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationWaterDepth,
                    new Unit("meter", "m")),
                new EngineParameter(QuantityType.Volume, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationVolume,
                    new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.TotalArea, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationTotalArea,
                    new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.TotalWidth, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationTotalWidth,
                    new Unit("meter", "m")),
                new EngineParameter(QuantityType.LateralAtNodes, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationLateralAtNodes,
                    new Unit("cubic meter per second", "m³/s")),
                new EngineParameter(QuantityType.Salinity, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationSaltConcentration,
                    new Unit("parts per thousand", "ppt")),
                new EngineParameter(QuantityType.Density, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationDensity,
                    new Unit("kilo per cubic meter", "kg/m3")), //or kg/l
                new EngineParameter(QuantityType.QTotal_1d2d, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationQTotal_1d2d,
                    new Unit("cubic meter per second", "m³/s")),
                new EngineParameter(QuantityType.NegativeDepth, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoNegativeDepthDisplayName,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.NoIteration, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoNumberOfIterationsDisplayName,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.Temperature, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationTemperature,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.TotalHeatFlux, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationTotalHeatFlux,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.RadFluxClearSky, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationRadFluxClearSky,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.HeatLossConv, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationHeatLossConv,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.NetSolarRad, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationNetSolarRad,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.EffectiveBackRad, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationEffectiveBackRad,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.HeatLossEvap, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationHeatLossEvap,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.HeatLossForcedEvap, ElementSet.GridpointsOnBranches,
                    DataItemRole.Output,
                    Model1DParameterNames.LocationHeatLossForcedEvap,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.HeatLossFreeEvap, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationHeatLossFreeEvap,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.HeatLossForcedConv, ElementSet.GridpointsOnBranches,
                    DataItemRole.Output,
                    Model1DParameterNames.LocationHeatLossForcedConv,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.HeatLossFreeConv, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.LocationHeatLossFreeConv,
                    new Unit(string.Empty, string.Empty)),


                // Roughness section 'Main' at gridpoints
                new EngineParameter(QuantityType.TimeStepEstimation, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoTimeStepEstimationDisplayName,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.DischargeMain, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.MainChannel +
                    Model1DParameterNames.SubSectionDischarge,
                    new Unit("cubic meter per second", "m³/s")),
                new EngineParameter(QuantityType.ChezyMain, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.MainChannel +
                    Model1DParameterNames.SubSectionRoughness,
                    new Unit("", "m^1/2*s^-1")),
                new EngineParameter(QuantityType.AreaMain, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.MainChannel +
                    Model1DParameterNames.SubSectionFlowArea,
                    new Unit("square meter", "m²")),
                new EngineParameter(QuantityType.WidthMain, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.MainChannel +
                    Model1DParameterNames.SubSectionFlowWidth,
                    new Unit("meter", "m")),
                new EngineParameter(QuantityType.HydradMain, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.MainChannel +
                    Model1DParameterNames.SubSectionHydraulicRadius,
                    new Unit("meter", "m")),

                // Roughness section 'Floodplain1' at gridpoints
                new EngineParameter(QuantityType.DischargeFP1, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.FloodPlain1 +
                    Model1DParameterNames.SubSectionDischarge,
                    new Unit("cubic meter per second", "m³/s")),
                new EngineParameter(QuantityType.ChezyFP1, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.FloodPlain1 +
                    Model1DParameterNames.SubSectionRoughness, new Unit("", "m^1/2*s^-1")),
                new EngineParameter(QuantityType.AreaFP1, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.FloodPlain1 +
                    Model1DParameterNames.SubSectionFlowArea, new Unit("square meter", "m²")),
                new EngineParameter(QuantityType.WidthFP1, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.FloodPlain1 +
                    Model1DParameterNames.SubSectionFlowWidth, new Unit("meter", "m")),
                new EngineParameter(QuantityType.HydradFP1, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.FloodPlain1 +
                    Model1DParameterNames.SubSectionHydraulicRadius,
                    new Unit("meter", "m")),

                // Roughness section 'Floodplain2' at gridpoints
                new EngineParameter(QuantityType.DischargeFP2, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.FloodPlain2 +
                    Model1DParameterNames.SubSectionDischarge,
                    new Unit("cubic meter per second", "m³/s")),
                new EngineParameter(QuantityType.ChezyFP2, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.FloodPlain2 +
                    Model1DParameterNames.SubSectionRoughness,
                    new Unit("", "m^1/2*s^-1")),
                new EngineParameter(QuantityType.AreaFP2, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.FloodPlain2 +
                    Model1DParameterNames.SubSectionFlowArea,
                    new Unit("square meter", "m²")),
                new EngineParameter(QuantityType.WidthFP2, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.FloodPlain2 +
                    Model1DParameterNames.SubSectionFlowWidth,
                    new Unit("meter", "m")),
                new EngineParameter(QuantityType.HydradFP2, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.FloodPlain2 +
                    Model1DParameterNames.SubSectionHydraulicRadius,
                    new Unit("meter", "m")),

                // Reach segments ( = the staggered grid points)
                new EngineParameter(QuantityType.Discharge, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.BranchDischarge,
                    new Unit("cubic meter per second", "m³/s")),
                new EngineParameter(QuantityType.Velocity, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.BranchVelocity,
                    new Unit("meter per second", "m/s")),
                new EngineParameter(QuantityType.Dispersion, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                    Model1DParameterNames.BranchSaltDispersion,
                    new Unit("square meter per second", "m2/s")),
                new EngineParameter(QuantityType.FlowArea, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.BranchFlowArea,
                    new Unit("square meter", "m²")),
                new EngineParameter(QuantityType.FlowHydrad, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.BranchHydraulicRadius,
                    new Unit("meter", "m")),
                new EngineParameter(QuantityType.FlowConv, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.BranchConveyance,
                    new Unit("cubic meter per second", "m³/s")),
                new EngineParameter(QuantityType.FlowChezy, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.BranchRoughness,
                    new Unit("", "m^1/2*s^-1")),
                new EngineParameter(QuantityType.WaterLevelGradient, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.BranchWaterLevelGradient,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.Froude, ElementSet.ReachSegElmSet, DataItemRole.Output,
                    Model1DParameterNames.BranchFroudeNumber,
                    new Unit(string.Empty, string.Empty)),

                // Structures (not pumps)
                new EngineParameter(QuantityType.Discharge, ElementSet.Structures, DataItemRole.Output,
                    Model1DParameterNames.StructureDischarge,
                    new Unit("cubic meter per second", "m³/s")),
                new EngineParameter(QuantityType.Velocity, ElementSet.Structures, DataItemRole.Output,
                    Model1DParameterNames.StructureVelocity,
                    new Unit("meter per second", "m/s")),
                new EngineParameter(QuantityType.FlowArea, ElementSet.Structures, DataItemRole.Output,
                    Model1DParameterNames.StructureFlowArea,
                    new Unit("square meter", "m²")),
                new EngineParameter(QuantityType.CrestLevel, ElementSet.Structures,
                    DataItemRole.Output | DataItemRole.Input,
                    Model1DParameterNames.StructureCrestLevel,
                    new Unit("meter above reference level", "m AD")),
                new EngineParameter(QuantityType.CrestWidth, ElementSet.Structures,
                    DataItemRole.Output | DataItemRole.Input,
                    Model1DParameterNames.StructureCrestWidth,
                    new Unit("meter", "m")),
                new EngineParameter(QuantityType.GateLowerEdgeLevel, ElementSet.Structures,
                    DataItemRole.Output | DataItemRole.Input,
                    Model1DParameterNames.StructureGateLevel,
                    new Unit("meter above reference level", "m AD")),
                new EngineParameter(QuantityType.GateOpeningWidth, ElementSet.Structures,
                    DataItemRole.Output | DataItemRole.Input,
                    Model1DParameterNames.StructureGateOpeningWidth,
                    new Unit("meter", "m AD")),
                new EngineParameter(QuantityType.GateOpeningHorizontalDirection, ElementSet.Structures,
                    DataItemRole.Output | DataItemRole.Input,
                    Model1DParameterNames.StructureGateOpeningHorizontalDirection,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.GateOpeningHeight, ElementSet.Structures,
                    DataItemRole.Output | DataItemRole.Input,
                    Model1DParameterNames.StructureOpeningHeight,
                    new Unit("meter", "m")),
                new EngineParameter(QuantityType.ValveOpening, ElementSet.Structures,
                    DataItemRole.Output | DataItemRole.Input,
                    Model1DParameterNames.StructureValveOpening,
                    new Unit(string.Empty, string.Empty)),
                new EngineParameter(QuantityType.WaterlevelUp, ElementSet.Structures, DataItemRole.Output,
                    Model1DParameterNames.StructureWaterlevelUp,
                    new Unit("meter above reference level", "m AD")),
                new EngineParameter(QuantityType.WaterlevelDown, ElementSet.Structures, DataItemRole.Output,
                    Model1DParameterNames.StructureWaterlevelDown,
                    new Unit("meter above reference level", "m AD")),
                new EngineParameter(QuantityType.Head, ElementSet.Structures, DataItemRole.Output,
                    Model1DParameterNames.StructureHeadDifference,
                    new Unit("meter", "m")),
                new EngineParameter(QuantityType.PressureDifference, ElementSet.Structures, DataItemRole.Output,
                    Model1DParameterNames.StructurePressureDifference,
                    new Unit("pascal", "Pa")),
                new EngineParameter(QuantityType.WaterLevelAtCrest, ElementSet.Structures, DataItemRole.Output,
                    Model1DParameterNames.StructureWaterLevelAtCrest,
                    new Unit("meter above reference level", "m AD")),

                // Pumps
                new EngineParameter(QuantityType.SuctionSideLevel, ElementSet.Pumps, DataItemRole.Output,
                    Model1DParameterNames.PumpSuctionSide,
                    new Unit("meter above reference level", "m AD")),

                new EngineParameter(QuantityType.DeliverySideLevel, ElementSet.Pumps, DataItemRole.Output,
                    Model1DParameterNames.PumpDeliverySide,
                    new Unit("meter above reference level", "m AD")),

                new EngineParameter(QuantityType.PumpHead, ElementSet.Pumps, DataItemRole.Output,
                    Model1DParameterNames.PumpHead,
                    new Unit("meter", "m")),

                new EngineParameter(QuantityType.ActualPumpStage, ElementSet.Pumps, DataItemRole.Output,
                    Model1DParameterNames.PumpStage,
                    new Unit(string.Empty, string.Empty)),

                new EngineParameter(QuantityType.PumpCapacity, ElementSet.Pumps, DataItemRole.Input | DataItemRole.Output,
                    Model1DParameterNames.PumpCapacity,
                    new Unit("cubic meter per second", "m³/s")),

                new EngineParameter(QuantityType.ReductionFactor, ElementSet.Pumps, DataItemRole.Output,
                    Model1DParameterNames.PumpReductionFactor,
                    new Unit(string.Empty, string.Empty)),

                new EngineParameter(QuantityType.PumpDischarge, ElementSet.Pumps, DataItemRole.Output,
                    Model1DParameterNames.PumpDischarge,
                    new Unit("cubic meter per second", "m³/s")),

                // Observation points
                new EngineParameter(QuantityType.WaterLevel, ElementSet.Observations, DataItemRole.Output,
                    Model1DParameterNames.ObservationPointWaterLevel,
                    new Unit("meter above reference level", "m AD")),
                new EngineParameter(QuantityType.WaterDepth, ElementSet.Observations, DataItemRole.Output,
                    Model1DParameterNames.ObservationPointWaterDepth,
                    new Unit("meter", "m")),
                new EngineParameter(QuantityType.Discharge, ElementSet.Observations, DataItemRole.Output,
                    Model1DParameterNames.ObservationPointDischarge,
                    new Unit("cubic meter per second", "m³/s")),
                new EngineParameter(QuantityType.Velocity, ElementSet.Observations, DataItemRole.Output,
                    Model1DParameterNames.ObservationPointVelocity,
                    new Unit("meter per second", "m/s")),
                new EngineParameter(QuantityType.Salinity, ElementSet.Observations, DataItemRole.Output,
                    Model1DParameterNames.ObservationPointSaltConcentration,
                    new Unit("parts per thousand", "ppt")),
                new EngineParameter(QuantityType.Dispersion, ElementSet.Observations, DataItemRole.Output,
                    Model1DParameterNames.ObservationPointSaltDispersion,
                    new Unit("square meter per second", "m2/s")),
                new EngineParameter(QuantityType.Volume, ElementSet.Observations, DataItemRole.Output,
                    Model1DParameterNames.ObservationPointVolume,
                    new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.Temperature, ElementSet.Observations, DataItemRole.Output,
                    Model1DParameterNames.ObservationPointTemperature,
                    new Unit("parts per thousand", "ppt")),
                // Laterals
                new EngineParameter(QuantityType.ActualDischarge, ElementSet.Laterals,
                    DataItemRole.Output | DataItemRole.Input,
                    Model1DParameterNames.LateralActualDischarge,
                    new Unit("cubic meter per second", "m³/s")),
                new EngineParameter(QuantityType.DefinedDischarge, ElementSet.Laterals, DataItemRole.Output,
                    Model1DParameterNames.LateralDefinedDischarge,
                    new Unit("cubic meter per second", "m³/s")),
                new EngineParameter(QuantityType.LateralDifference, ElementSet.Laterals, DataItemRole.Output,
                    Model1DParameterNames.LateralDifference,
                    new Unit("cubic meter per second", "m³/s")),
                new EngineParameter(QuantityType.WaterLevel, ElementSet.Laterals, DataItemRole.Output,
                    Model1DParameterNames.LateralWaterLevel,
                    new Unit("meter above reference level", "m AD")),

                // Retentions
                new EngineParameter(QuantityType.WaterLevel, ElementSet.Retentions, DataItemRole.Output,
                    Model1DParameterNames.RetentionWaterLevel,
                    new Unit("meter above reference level", "m AD")),
                new EngineParameter(QuantityType.Volume, ElementSet.Retentions, DataItemRole.Output,
                    Model1DParameterNames.RetentionVolume, new Unit("cubic meter", "m³")),

                // Water balance 
                new EngineParameter(QuantityType.BalVolume, ElementSet.ModelWide,
                    DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoWaterBalanceTotalVolume, new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.BalError, ElementSet.ModelWide,
                    DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoWaterBalanceVolumeError, new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.BalStorage, ElementSet.ModelWide,
                    DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoWaterBalanceTotalStorage, new Unit("cubic meter", "m³")),

                // Water balance for 1d2d 
                new EngineParameter(QuantityType.BalBoundariesIn, ElementSet.ModelWide, DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoWaterBalanceBoundariesIn, new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.BalBoundariesOut, ElementSet.ModelWide, DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoWaterBalanceBoundariesOut,
                    new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.BalBoundariesTot, ElementSet.ModelWide, DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoWaterBalanceBoundariesTotal,
                    new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.BalLatIn, ElementSet.ModelWide, DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoWaterBalanceLateralDischargeIn,
                    new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.BalLatOut, ElementSet.ModelWide, DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoWaterBalanceLateralDischargeOut,
                    new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.BalLatTot, ElementSet.ModelWide, DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoWaterBalanceLateralDischargeTotal,
                    new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.Bal2d1dIn, ElementSet.ModelWide, DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeIn,
                    new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.Bal2d1dOut, ElementSet.ModelWide, DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeOut,
                    new Unit("cubic meter", "m³")),
                new EngineParameter(QuantityType.Bal2d1dTot, ElementSet.ModelWide, DataItemRole.Output,
                    Model1DParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeTotal,
                    new Unit("cubic meter", "m³")),

                // available as simulation info = ElementSet varies
                // available simulation info
                //QuantityType.TimeStepEstimation, DataItemRole.Output, "Timestep estimation"	Nodes	Onder kopje: Simulation info
                //QuantityType.NoIteration, DataItemRole.Output, "No iteration"	Nodes	Onder kopje: Simulation info

                // Finite volume grid ( = output for D-Water Quality / DELWAQ) 
                new EngineParameter(QuantityType.FiniteGridType, ElementSet.FiniteVolumeGridOnGridPoints,
                    DataItemRole.Output,
                    Model1DParameterNames.FiniteVolumeGridType,
                    new Unit("type of grid", "type of grid")),
            };
        }

        /// <summary>
        /// Gets the engine parameters for the given feature that can be used in RTC. 
        /// </summary>
        /// <param name="mapping">Collection of engine parameters supported by SOBEK.</param>
        /// <param name="feature">The feature to get the engine parameters for.</param>
        /// <param name="useSalinity">Whether or not salinity is used.</param>
        /// <param name="useTemperature">Whether or not temperature is used.</param>
        /// <returns></returns>
        public static IEnumerable<EngineParameter> GetExchangeableParameters(IEnumerable<EngineParameter> mapping,
            IFeature feature, bool useSalinity, bool useTemperature)
        {
            var elementSet = GetElementSet(feature);
            if (feature == null)
            {
                yield break;
            }

            foreach (var engineParameter in mapping.Where(m => m.ElementSet == elementSet))
            {
                if (!AllowedAsQuantityTypeForFeature(feature, engineParameter, useSalinity, useTemperature))
                {
                    continue;
                }

                yield return engineParameter;
            }
        }

        /// <summary>
        /// Checks whether the given engine parameter is allowed for the given feature.
        /// </summary>
        /// <param name="feature">The feature to check.</param>
        /// <param name="engineParameter">The engine parameter to check.</param>
        /// <param name="useSalinity">Whether or not salinity is used.</param>
        /// <param name="useTemperature">Whether or not temperature is used.</param>
        /// <returns>Whether or not the engine parameter is allowed for the feature.</returns>
        public static bool AllowedAsQuantityTypeForFeature(
            IFeature feature, EngineParameter engineParameter, bool useSalinity, bool useTemperature)
        {
            switch (feature)
            {
                case IPump _:
                    return IsAllowedForPump(engineParameter);
                case ICulvert culvert:
                    return IsAllowedForCulvert(engineParameter, culvert);
                case IWeir weir:
                    return IsAllowedForWeir(weir.WeirFormula, engineParameter);
                case IObservationPoint _:
                    return IsAllowedForObservationPoint(engineParameter, useSalinity, useTemperature);
            }

            return false;
        }

        private static bool IsAllowedForPump(EngineParameter engineParameter)
        {
            return engineParameter.QuantityType == QuantityType.PumpCapacity;
        }

        private static bool IsAllowedForWeir(IWeirFormula weirFormula, EngineParameter engineParameter)
        {
            switch (weirFormula)
            {
                case SimpleWeirFormula _:
                    return engineParameter.QuantityType == QuantityType.CrestLevel;
                case GatedWeirFormula _:
                    return engineParameter.QuantityType == QuantityType.GateLowerEdgeLevel;
                case GeneralStructureWeirFormula _:
                    return engineParameter.QuantityType == QuantityType.CrestLevel
                           || engineParameter.QuantityType == QuantityType.GateOpeningHeight
                           || engineParameter.QuantityType == QuantityType.GateLowerEdgeLevel
                           || engineParameter.QuantityType == QuantityType.GateOpeningWidth;
                default:
                    return false;
            }
        }

        private static bool IsAllowedForObservationPoint(EngineParameter engineParameter, bool useSalinity, bool useTemperature)
        {
            bool isAllowed = engineParameter.QuantityType == QuantityType.WaterLevel
                             || engineParameter.QuantityType == QuantityType.WaterDepth
                             || engineParameter.QuantityType == QuantityType.Discharge
                             || engineParameter.QuantityType == QuantityType.Velocity;

            if (useSalinity)
            {
                isAllowed = isAllowed || engineParameter.QuantityType == QuantityType.Salinity;
            }

            if (useTemperature)
            {
                isAllowed = isAllowed || engineParameter.QuantityType == QuantityType.Temperature;
            }

            return isAllowed;
        }

        private static bool IsAllowedForCulvert(EngineParameter engineParameter, ICulvert culvert)
        {
            return culvert.IsGated && engineParameter.QuantityType == QuantityType.ValveOpening;
        }

        public static ElementSet? GetElementSet(IFeature feature)
        {
            if (feature is IObservationPoint)
            {
                return ElementSet.Observations;
            }

            if (feature is ILateralSource)
            {
                return ElementSet.Laterals;
            }

            if (feature is IPump)
            {
                return ElementSet.Pumps;
            }

            if (feature is IStructure1D)
            {
                return ElementSet.Structures;
            }

            if (feature is IRetention)
            {
                return ElementSet.Retentions;
            }

            return null;
        }

        /// <summary>
        /// Returns the initialvalue for settable EngineParameters (Role.Input)
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static double GetInitialValue(IFeature feature, string parameter)
        {
            if (feature is IWeir weir)
            {
                switch (parameter)
                {
                    case Model1DParameterNames.StructureCrestLevel:
                        return weir.CrestLevel;
                    case Model1DParameterNames.StructureCrestWidth:
                        return weir.CrestWidth;
                }

                if (weir.IsGated)
                {
                    var formula = (IGatedWeirFormula) weir.WeirFormula;
                    switch (parameter)
                    {
                        case Model1DParameterNames.StructureGateLevel:
                            return formula.LowerEdgeLevel;
                        case Model1DParameterNames.StructureOpeningHeight:
                            return formula.GateOpening;
                    }
                }
            }
            else if ((feature is ICulvert culvert) && (parameter == Model1DParameterNames.StructureValveOpening))
            {
                return culvert.GateInitialOpening;
            }
            else if (feature is IPump pump)
            {
                switch (parameter)
                {
                    case Model1DParameterNames.PumpCapacity:
                        return pump.Capacity; // or 0
                }
            }
            else if (feature is ILateralSource)
            {
                return -100.0;
            }

            return 0.0;
        }
    }
}