using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
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
                                        WaterFlowModelParameterNames.LocationWaterLevel,
                                        new Unit("meter above reference level", "m AD")),
                    new EngineParameter(QuantityType.WaterDepth, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationWaterDepth,
                                        new Unit("meter", "m")),
                    new EngineParameter(QuantityType.Volume, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationVolume,
                                        new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.TotalArea, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationTotalArea,
                                        new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.TotalWidth, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationTotalWidth,
                                        new Unit("meter", "m")),
                     new EngineParameter(QuantityType.LateralAtNodes, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationLateralAtNodes,
                                        new Unit("cubic meter per second", "m³/s")),
                    new EngineParameter(QuantityType.Salinity, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationSaltConcentration,
                                        new Unit("parts per thousand", "ppt")),
                    new EngineParameter(QuantityType.Density, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationDensity,
                                        new Unit("kilo per cubic meter", "kg/m3")), //or kg/l
                    new EngineParameter(QuantityType.QTotal_1d2d, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationQTotal_1d2d,
                                        new Unit("cubic meter per second", "m³/s")),
                    new EngineParameter(QuantityType.NegativeDepth, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoNegativeDepthDisplayName,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.NoIteration, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoNumberOfIterationsDisplayName,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.Temperature, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationTemperature,
                                        new Unit("", "")),
                                        new EngineParameter(QuantityType.TotalHeatFlux, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationTotalHeatFlux,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.RadFluxClearSky, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationRadFluxClearSky,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.HeatLossConv, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationHeatLossConv,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.NetSolarRad, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationNetSolarRad,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.EffectiveBackRad, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationEffectiveBackRad,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.HeatLossEvap, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationHeatLossEvap,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.HeatLossForcedEvap, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationHeatLossForcedEvap,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.HeatLossFreeEvap, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationHeatLossFreeEvap,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.HeatLossForcedConv, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationHeatLossForcedConv,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.HeatLossFreeConv, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LocationHeatLossFreeConv,
                                        new Unit("", "")),


                    // Roughness section 'Main' at gridpoints
                    new EngineParameter(QuantityType.TimeStepEstimation, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoTimeStepEstimationDisplayName,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.DischargeMain, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.MainChannel +
                                        WaterFlowModelParameterNames.SubSectionDischarge,
                                        new Unit("cubic meter per second", "m³/s")),
                    new EngineParameter(QuantityType.ChezyMain, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.MainChannel +
                                        WaterFlowModelParameterNames.SubSectionRoughness, 
                                        new Unit("", "m^1/2*s^-1")),
                    new EngineParameter(QuantityType.AreaMain, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.MainChannel +
                                        WaterFlowModelParameterNames.SubSectionFlowArea, 
                                        new Unit("square meter", "m²")),
                    new EngineParameter(QuantityType.WidthMain, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.MainChannel +
                                        WaterFlowModelParameterNames.SubSectionFlowWidth, 
                                        new Unit("meter", "m")),
                    new EngineParameter(QuantityType.HydradMain, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.MainChannel +
                                        WaterFlowModelParameterNames.SubSectionHydraulicRadius,
                                        new Unit("meter", "m")),

                    // Roughness section 'Floodplain1' at gridpoints
                    new EngineParameter(QuantityType.DischargeFP1, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.FloodPlain1 +
                                        WaterFlowModelParameterNames.SubSectionDischarge,
                                        new Unit("cubic meter per second", "m³/s")),
                    new EngineParameter(QuantityType.ChezyFP1, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.FloodPlain1 +
                                        WaterFlowModelParameterNames.SubSectionRoughness, new Unit("", "m^1/2*s^-1")),
                    new EngineParameter(QuantityType.AreaFP1, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.FloodPlain1 +
                                        WaterFlowModelParameterNames.SubSectionFlowArea, new Unit("square meter", "m²")),
                    new EngineParameter(QuantityType.WidthFP1, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.FloodPlain1 +
                                        WaterFlowModelParameterNames.SubSectionFlowWidth, new Unit("meter", "m")),
                    new EngineParameter(QuantityType.HydradFP1, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.FloodPlain1 +
                                        WaterFlowModelParameterNames.SubSectionHydraulicRadius,
                                        new Unit("meter", "m")),

                    // Roughness section 'Floodplain2' at gridpoints
                    new EngineParameter(QuantityType.DischargeFP2, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.FloodPlain2 +
                                        WaterFlowModelParameterNames.SubSectionDischarge,
                                        new Unit("cubic meter per second", "m³/s")),
                    new EngineParameter(QuantityType.ChezyFP2, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.FloodPlain2 +
                                        WaterFlowModelParameterNames.SubSectionRoughness,
                                        new Unit("", "m^1/2*s^-1")),
                    new EngineParameter(QuantityType.AreaFP2, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.FloodPlain2 +
                                        WaterFlowModelParameterNames.SubSectionFlowArea,
                                        new Unit("square meter", "m²")),
                    new EngineParameter(QuantityType.WidthFP2, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.FloodPlain2 +
                                        WaterFlowModelParameterNames.SubSectionFlowWidth,
                                        new Unit("meter", "m")),
                    new EngineParameter(QuantityType.HydradFP2, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.FloodPlain2 +
                                        WaterFlowModelParameterNames.SubSectionHydraulicRadius,
                                        new Unit("meter", "m")),

                    // Reach segments ( = the staggered grid points)
                    new EngineParameter(QuantityType.Discharge, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.BranchDischarge,
                                        new Unit("cubic meter per second", "m³/s")),
                    new EngineParameter(QuantityType.Velocity, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.BranchVelocity,
                                        new Unit("meter per second", "m/s")),
                    new EngineParameter(QuantityType.Dispersion, ElementSet.GridpointsOnBranches, DataItemRole.Output,
                                        WaterFlowModelParameterNames.BranchSaltDispersion,
                                        new Unit("square meter per second", "m2/s")),
                    new EngineParameter(QuantityType.FlowArea, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.BranchFlowArea,
                                        new Unit("square meter", "m²")),
                    new EngineParameter(QuantityType.FlowHydrad, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.BranchHydraulicRadius,
                                        new Unit("meter", "m")),
                    new EngineParameter(QuantityType.FlowConv, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.BranchConveyance,
                                        new Unit("cubic meter per second", "m³/s")),
                    new EngineParameter(QuantityType.FlowChezy, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.BranchRoughness,
                                        new Unit("", "m^1/2*s^-1")),
                    new EngineParameter(QuantityType.WaterLevelGradient, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.BranchWaterLevelGradient,
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.Froude, ElementSet.ReachSegElmSet, DataItemRole.Output,
                                        WaterFlowModelParameterNames.BranchFroudeNumber,
                                        new Unit("", "")),

                    // Structures (not pumps)
                    new EngineParameter(QuantityType.Discharge, ElementSet.Structures, DataItemRole.Output,
                                        WaterFlowModelParameterNames.StructureDischarge,
                                        new Unit("cubic meter per second", "m³/s")),
                    new EngineParameter(QuantityType.Velocity, ElementSet.Structures, DataItemRole.Output,
                                        WaterFlowModelParameterNames.StructureVelocity,
                                        new Unit("meter per second", "m/s")),
                    new EngineParameter(QuantityType.FlowArea, ElementSet.Structures, DataItemRole.Output,
                                        WaterFlowModelParameterNames.StructureFlowArea,
                                        new Unit("square meter", "m²")),
                    new EngineParameter(QuantityType.CrestLevel, ElementSet.Structures,
                                        DataItemRole.Output | DataItemRole.Input,
                                        WaterFlowModelParameterNames.StructureCrestLevel,
                                        new Unit("meter above reference level", "m AD")),
                    new EngineParameter(QuantityType.CrestWidth, ElementSet.Structures,
                                        DataItemRole.Output | DataItemRole.Input,
                                        WaterFlowModelParameterNames.StructureCrestWidth,
                                        new Unit("meter", "m")),
                    new EngineParameter(QuantityType.GateLowerEdgeLevel, ElementSet.Structures,
                                        DataItemRole.Output | DataItemRole.Input,
                                        WaterFlowModelParameterNames.StructureGateLevel,
                                        new Unit("meter above reference level", "m AD")),
                    new EngineParameter(QuantityType.GateOpeningHeight, ElementSet.Structures,
                                        DataItemRole.Output | DataItemRole.Input,
                                        WaterFlowModelParameterNames.StructureOpeningHeight,
                                        new Unit("meter", "m")),
                    new EngineParameter(QuantityType.ValveOpening, ElementSet.Structures,
                                        DataItemRole.Output | DataItemRole.Input,
                                        WaterFlowModelParameterNames.StructureValveOpening, 
                                        new Unit("", "")),
                    new EngineParameter(QuantityType.WaterlevelUp, ElementSet.Structures, DataItemRole.Output,
                                        WaterFlowModelParameterNames.StructureWaterlevelUp,
                                        new Unit("meter above reference level", "m AD")),
                    new EngineParameter(QuantityType.WaterlevelDown, ElementSet.Structures, DataItemRole.Output,
                                        WaterFlowModelParameterNames.StructureWaterlevelDown,
                                        new Unit("meter above reference level", "m AD")),
                    new EngineParameter(QuantityType.Head, ElementSet.Structures, DataItemRole.Output,
                                        WaterFlowModelParameterNames.StructureHeadDifference, 
                                        new Unit("meter", "m")),
                    new EngineParameter(QuantityType.PressureDifference, ElementSet.Structures, DataItemRole.Output,
                                        WaterFlowModelParameterNames.StructurePressureDifference,
                                        new Unit("pascal", "Pa")),
                    new EngineParameter(QuantityType.WaterLevelAtCrest, ElementSet.Structures, DataItemRole.Output,
                                        WaterFlowModelParameterNames.StructureWaterLevelAtCrest,
                                        new Unit("meter above reference level", "m AD")),
                    new EngineParameter(QuantityType.Setpoint, ElementSet.Structures, DataItemRole.Input,
                                        WaterFlowModelParameterNames.StructureSetPoint,
                                        new Unit("cubic meter per second", "m³/s")),
          
                    // Pumps
                    new EngineParameter(QuantityType.SuctionSideLevel, ElementSet.Pumps, DataItemRole.Output,
                                        WaterFlowModelParameterNames.PumpSuctionSide,
                                        new Unit("meter above reference level", "m AD")),

                    new EngineParameter(QuantityType.DeliverySideLevel, ElementSet.Pumps, DataItemRole.Output,
                                        WaterFlowModelParameterNames.PumpDeliverySide,
                                        new Unit("meter above reference level", "m AD")),

                    new EngineParameter(QuantityType.PumpHead, ElementSet.Pumps, DataItemRole.Output,
                                        WaterFlowModelParameterNames.PumpHead,
                                        new Unit("meter", "m")),

                    new EngineParameter(QuantityType.ActualPumpStage, ElementSet.Pumps, DataItemRole.Output,
                                        WaterFlowModelParameterNames.PumpStage,
                                        new Unit("", "")),

                    new EngineParameter(QuantityType.PumpCapacity, ElementSet.Pumps, DataItemRole.Output,
                                        WaterFlowModelParameterNames.PumpCapacity,
                                        new Unit("cubic meter per second", "m³/s")),

                    new EngineParameter(QuantityType.ReductionFactor, ElementSet.Pumps, DataItemRole.Output,
                                        WaterFlowModelParameterNames.PumpReductionFactor,
                                        new Unit("", "")),

                    new EngineParameter(QuantityType.PumpDischarge, ElementSet.Pumps, DataItemRole.Output,
                                        WaterFlowModelParameterNames.PumpDischarge,
                                        new Unit("cubic meter per second", "m³/s")),

                    // Observation points
                    new EngineParameter(QuantityType.WaterLevel, ElementSet.Observations, DataItemRole.Output,
                                        WaterFlowModelParameterNames.ObservationPointWaterLevel,
                                        new Unit("meter above reference level", "m AD")),
                    new EngineParameter(QuantityType.WaterDepth, ElementSet.Observations, DataItemRole.Output,
                                        WaterFlowModelParameterNames.ObservationPointWaterDepth, 
                                        new Unit("meter", "m")),
                    new EngineParameter(QuantityType.Discharge, ElementSet.Observations, DataItemRole.Output,
                                        WaterFlowModelParameterNames.ObservationPointDischarge,
                                        new Unit("cubic meter per second", "m³/s")),
                    new EngineParameter(QuantityType.Velocity, ElementSet.Observations, DataItemRole.Output,
                                        WaterFlowModelParameterNames.ObservationPointVelocity,
                                        new Unit("meter per second", "m/s")),
                    new EngineParameter(QuantityType.Salinity, ElementSet.Observations, DataItemRole.Output,
                                        WaterFlowModelParameterNames.ObservationPointSaltConcentration,
                                        new Unit("parts per thousand", "ppt")),
                    new EngineParameter(QuantityType.Dispersion, ElementSet.Observations, DataItemRole.Output,
                                        WaterFlowModelParameterNames.ObservationPointSaltDispersion,
                                        new Unit("square meter per second", "m2/s")),
                    new EngineParameter(QuantityType.Volume, ElementSet.Observations, DataItemRole.Output,
                                        WaterFlowModelParameterNames.ObservationPointVolume,
                                        new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.Temperature, ElementSet.Observations, DataItemRole.Output,
                                        WaterFlowModelParameterNames.ObservationPointTemperature,
                                        new Unit("parts per thousand", "ppt")),
                    // Laterals
                    new EngineParameter(QuantityType.Discharge, ElementSet.Laterals,
                                        DataItemRole.Input,
                                        WaterFlowModelParameterNames.LateralDischarge,
                                        new Unit("cubic meter per second", "m³/s")),
                    new EngineParameter(QuantityType.Discharge, ElementSet.Laterals,
                                        DataItemRole.Output,
                                        WaterFlowModelParameterNames.LateralDischarge,
                                        new Unit("cubic meter per second", "m³/s")),
                    new EngineParameter(QuantityType.WaterLevel, ElementSet.Laterals, DataItemRole.Output,
                                        WaterFlowModelParameterNames.LateralWaterLevel,
                                        new Unit("meter above reference level", "m AD")),

                    // Retentions
                    new EngineParameter(QuantityType.WaterLevel, ElementSet.Retentions, DataItemRole.Output,
                                        WaterFlowModelParameterNames.RetentionWaterLevel,
                                        new Unit("meter above reference level", "m AD")),
                    new EngineParameter(QuantityType.Volume, ElementSet.Retentions, DataItemRole.Output,
                                        WaterFlowModelParameterNames.RetentionVolume, new Unit("cubic meter", "m³")),

                    // Water balance 
                    new EngineParameter(QuantityType.BalVolume, ElementSet.ModelWide, 
                                        DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoWaterBalanceTotalVolume, new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.BalError, ElementSet.ModelWide, 
                                        DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoWaterBalanceVolumeError, new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.BalStorage, ElementSet.ModelWide, 
                                        DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoWaterBalanceTotalStorage, new Unit("cubic meter", "m³")),

                    // Water balance for 1d2d 
                    new EngineParameter(QuantityType.BalBoundariesIn, ElementSet.ModelWide, DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoWaterBalanceBoundariesIn, new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.BalBoundariesOut, ElementSet.ModelWide, DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoWaterBalanceBoundariesOut, new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.BalBoundariesTot, ElementSet.ModelWide, DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoWaterBalanceBoundariesTotal, new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.BalLatIn, ElementSet.ModelWide, DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateralDischargeIn, new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.BalLatOut, ElementSet.ModelWide, DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateralDischargeOut, new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.BalLatTot, ElementSet.ModelWide, DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateralDischargeTotal, new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.Bal2d1dIn, ElementSet.ModelWide, DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeIn, new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.Bal2d1dOut, ElementSet.ModelWide, DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeOut, new Unit("cubic meter", "m³")),
                    new EngineParameter(QuantityType.Bal2d1dTot, ElementSet.ModelWide, DataItemRole.Output,
                                        WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeTotal, new Unit("cubic meter", "m³")),

                    // available as simulation info = ElementSet varies
                    // available simulation info
                    //QuantityType.TimeStepEstimation, DataItemRole.Output, "Timestep estimation"	Nodes	Onder kopje: Simulation info
                    //QuantityType.NoIteration, DataItemRole.Output, "No iteration"	Nodes	Onder kopje: Simulation info

                    // Finite volume grid ( = output for D-Water Quality / DELWAQ) 
                    new EngineParameter(QuantityType.FiniteGridType, ElementSet.FiniteVolumeGridOnGridPoints, DataItemRole.Output,
                                        WaterFlowModelParameterNames.FiniteVolumeGridType,
                                        new Unit("type of grid", "type of grid")),
                };
        }

        // used for RTC and for OpenMI
        public static IEnumerable<EngineParameter> GetExchangableParameters(IList<EngineParameter> mapping, IFeature feature)
        {
            var elementSet = GetElementSet(feature);
            if (feature == null)
            {
                yield break;
            }
            foreach (var engineParameter in mapping.Where(m => m.ElementSet == elementSet))
            {
                if (!AllowedAsQuantityTypeForFeature(feature, engineParameter))
                {
                    continue;
                }
                yield return engineParameter;
            }
        }

        /// <summary>
        /// Extra filter for EngineMapping
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="engineParameter"></param>
        /// <returns></returns>
        public static bool AllowedAsQuantityTypeForFeature(IFeature feature, EngineParameter engineParameter)
        {
            if (feature is IWeir)
            {
                var weir = (IWeir) feature;
                if ((engineParameter.QuantityType == QuantityType.ValveOpening) || 
                    (engineParameter.QuantityType == QuantityType.Setpoint))
                {
                    return false;
                }
                if ((!weir.IsGated) && ((engineParameter.QuantityType == QuantityType.GateLowerEdgeLevel) ||
                     (engineParameter.QuantityType == QuantityType.GateOpeningHeight)))
                {
                    return false;
                }
            }
            if (feature is ICulvert)
            {
                var culvert = (ICulvert) feature;
                if ((engineParameter.QuantityType == QuantityType.CrestLevel) ||
                    (engineParameter.QuantityType == QuantityType.CrestWidth) ||
                    (engineParameter.QuantityType == QuantityType.GateLowerEdgeLevel) ||
                    (engineParameter.QuantityType == QuantityType.GateOpeningHeight) ||
                    (engineParameter.QuantityType == QuantityType.Setpoint))
                {
                    return false;
                }
                if ((!culvert.IsGated) && (engineParameter.QuantityType == QuantityType.ValveOpening))
                {
                    return false;
                }
            }
            if (feature is IPump)
            {
                if ((engineParameter.QuantityType == QuantityType.CrestLevel) ||
                    (engineParameter.QuantityType == QuantityType.CrestWidth) ||
                    (engineParameter.QuantityType == QuantityType.GateLowerEdgeLevel) ||
                    (engineParameter.QuantityType == QuantityType.GateOpeningHeight) ||
                    (engineParameter.QuantityType == QuantityType.ValveOpening))
                {
                    return false;
                }
            }
            return true;
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
        /// <param name="quantityType"></param>
        /// <returns></returns>
        public static double GetInitialValue(IFeature feature, QuantityType quantityType)
        {
            if (feature is IWeir) 
            {
                var weir = (IWeir)feature;
                switch (quantityType)
                {
                    case QuantityType.CrestLevel:
                        return weir.CrestLevel;
                    case QuantityType.CrestWidth:
                        return weir.CrestWidth;
                }

                if (weir.IsGated)
                {
                    var formula = (IGatedWeirFormula)weir.WeirFormula;
                    switch (quantityType)
                    {
                        case QuantityType.GateLowerEdgeLevel:
                            return weir.CrestLevel + formula.GateOpening;
                        case QuantityType.GateOpeningHeight:
                            return formula.GateOpening;
                    }
                }
            }
            else if ((feature is ICulvert) && (quantityType == QuantityType.ValveOpening))
            {
                var culvert = (ICulvert) feature;
                return culvert.GateInitialOpening;
            }
            else if (feature is IPump)
            {
                var pump = (IPump) feature;
                switch (quantityType)
                {
                    case QuantityType.Setpoint:
                        return pump.Capacity; // or 0
                }
            }
            else if (feature is ILateralSource)
            {
                return -100.0;
            }
            return 0.0;
        }
        /// <summary>
        /// Returns the initialvalue for settable EngineParameters (Role.Input)
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static double GetInitialValue(IFeature feature, string parameter)
        {
            if (feature is IWeir) 
            {
                var weir = (IWeir)feature;
                switch (parameter)
                {
                    case WaterFlowModelParameterNames.StructureCrestLevel:
                        return weir.CrestLevel;
                    case WaterFlowModelParameterNames.StructureCrestWidth:
                        return weir.CrestWidth;
                }

                if (weir.IsGated)
                {
                    var formula = (IGatedWeirFormula)weir.WeirFormula;
                    switch (parameter)
                    {
                        case WaterFlowModelParameterNames.StructureGateLevel:
                            return weir.CrestLevel + formula.GateOpening;
                        case WaterFlowModelParameterNames.StructureOpeningHeight:
                            return formula.GateOpening;
                    }
                }
            }
            else if ((feature is ICulvert) && (parameter == WaterFlowModelParameterNames.StructureValveOpening))
            {
                var culvert = (ICulvert) feature;
                return culvert.GateInitialOpening;
            }
            else if (feature is IPump)
            {
                var pump = (IPump) feature;
                switch (parameter)
                {
                    case WaterFlowModelParameterNames.StructureSetPoint:
                        return pump.Capacity; // or 0
                }
            }
            else if (feature is ILateralSource)
            {
                return -100.0;
            }
            return 0.0;
        }
        
        public static string GetStandardName(QuantityType qt, ElementSet es)
        {
            if (qt == QuantityType.WaterLevel && es == ElementSet.GridpointsOnBranches) return FunctionAttributes.StandardNames.WaterLevel;
            if (qt == QuantityType.WaterDepth && es == ElementSet.GridpointsOnBranches) return FunctionAttributes.StandardNames.WaterDepth;
            if (qt == QuantityType.Volume && es == ElementSet.GridpointsOnBranches) return FunctionAttributes.StandardNames.WaterVolume;
            if (qt == QuantityType.Salinity && es == ElementSet.GridpointsOnBranches) return FunctionAttributes.StandardNames.WaterSalinity;
            if (qt == QuantityType.Density && es == ElementSet.GridpointsOnBranches) return FunctionAttributes.StandardNames.WaterDensity;
            if (qt == QuantityType.QTotal_1d2d && es == ElementSet.GridpointsOnBranches) return FunctionAttributes.StandardNames.WaterQTotal1D2D;
            if (qt == QuantityType.Dispersion && es == ElementSet.GridpointsOnBranches) return FunctionAttributes.StandardNames.SaltDispersion;
            if (qt == QuantityType.EnergyLevels && es == ElementSet.ReachSegElmSet) return "nonstandard_energy_head";
            if (qt == QuantityType.TotalArea && es == ElementSet.GridpointsOnBranches) return "nonstandard_total_area";
            if (qt == QuantityType.TotalWidth && es == ElementSet.GridpointsOnBranches) return "nonstandard_water_width";
            if (qt == QuantityType.LateralAtNodes && es == ElementSet.GridpointsOnBranches) return "lateral_at_nodes";
            if (qt == QuantityType.Discharge && es == ElementSet.ReachSegElmSet) return FunctionAttributes.StandardNames.WaterDischarge;
            if (qt == QuantityType.Velocity && es == ElementSet.ReachSegElmSet) return FunctionAttributes.StandardNames.WaterVelocity;
            if (qt == QuantityType.FlowArea && es == ElementSet.ReachSegElmSet) return FunctionAttributes.StandardNames.WaterFlowArea;
            if (qt == QuantityType.FlowHydrad && es == ElementSet.ReachSegElmSet) return FunctionAttributes.StandardNames.WaterHydraulicRadius;
            if (qt == QuantityType.FlowConv && es == ElementSet.ReachSegElmSet) return FunctionAttributes.StandardNames.WaterConveyance;
            if (qt == QuantityType.FlowChezy && es == ElementSet.ReachSegElmSet) return "nonstandard_water_chezy";
            if (qt == QuantityType.WaterLevelGradient && es == ElementSet.ReachSegElmSet) return FunctionAttributes.StandardNames.WaterLevelGradient;
            if (qt == QuantityType.Froude && es == ElementSet.ReachSegElmSet) return FunctionAttributes.StandardNames.FroudeNumber;
            if (qt == QuantityType.DischargeMain && es == ElementSet.ReachSegElmSet) return "nonstandard_water_discharge_main";
            if (qt == QuantityType.ChezyMain && es == ElementSet.ReachSegElmSet) return "nonstandard_water_chezy_main";
            if (qt == QuantityType.AreaMain && es == ElementSet.ReachSegElmSet) return "nonstandard_water_area_main";
            if (qt == QuantityType.WidthMain && es == ElementSet.ReachSegElmSet) return "nonstandard_water_width_main";
            if (qt == QuantityType.HydradMain && es == ElementSet.ReachSegElmSet) return "nonstandard_water_hydraulic_radius_main";
            if (qt == QuantityType.DischargeFP1 && es == ElementSet.ReachSegElmSet) return "nonstandard_water_discharge_fp1";
            if (qt == QuantityType.ChezyFP1 && es == ElementSet.ReachSegElmSet) return "nonstandard_water_chezy_fp1";
            if (qt == QuantityType.AreaFP1 && es == ElementSet.ReachSegElmSet) return "nonstandard_water_area_fp1";
            if (qt == QuantityType.WidthFP1 && es == ElementSet.ReachSegElmSet) return "nonstandard_water_width_fp1";
            if (qt == QuantityType.HydradFP1 && es == ElementSet.ReachSegElmSet) return "nonstandard_hydraulic_radius_fp1";
            if (qt == QuantityType.DischargeFP2 && es == ElementSet.ReachSegElmSet) return "nonstandard_water_discharge_fp2";
            if (qt == QuantityType.ChezyFP2 && es == ElementSet.ReachSegElmSet) return "nonstandard_water_chezy_fp2";
            if (qt == QuantityType.AreaFP2 && es == ElementSet.ReachSegElmSet) return "nonstandard_water_area_fp2";
            if (qt == QuantityType.WidthFP2 && es == ElementSet.ReachSegElmSet) return "nonstandard_water_width_fp2";
            if (qt == QuantityType.HydradFP2 && es == ElementSet.ReachSegElmSet) return "nonstandard_water_hydraulic_radius_fp2";
            if (qt == QuantityType.Discharge && es == ElementSet.Structures) return FunctionAttributes.StandardNames.WaterDischarge;
            if (qt == QuantityType.Velocity && es == ElementSet.Structures) return FunctionAttributes.StandardNames.WaterVelocity;
            if (qt == QuantityType.FlowArea && es == ElementSet.Structures) return FunctionAttributes.StandardNames.WaterFlowArea;
            if (qt == QuantityType.CrestLevel && es == ElementSet.Structures) return FunctionAttributes.StandardNames.StructureCrestLevel;
            if (qt == QuantityType.CrestWidth && es == ElementSet.Structures) return FunctionAttributes.StandardNames.StructureCrestWidth;
            if (qt == QuantityType.GateLowerEdgeLevel && es == ElementSet.Structures) return FunctionAttributes.StandardNames.StructureGateLowerEdgeLevel;
            if (qt == QuantityType.GateOpeningHeight && es == ElementSet.Structures) return FunctionAttributes.StandardNames.StructureGateOpeningHeight;
            if (qt == QuantityType.ValveOpening && es == ElementSet.Structures) return FunctionAttributes.StandardNames.StructureValveOpening;
            if (qt == QuantityType.WaterlevelUp && es == ElementSet.Structures) return FunctionAttributes.StandardNames.StructureWaterLevelUpstream;
            if (qt == QuantityType.WaterlevelDown && es == ElementSet.Structures) return FunctionAttributes.StandardNames.StructureWaterLevelDownstream;
            if (qt == QuantityType.Head && es == ElementSet.Structures) return FunctionAttributes.StandardNames.StructureWaterHead;
            if (qt == QuantityType.PressureDifference && es == ElementSet.Structures) return FunctionAttributes.StandardNames.StructurePressureDifference;
            if (qt == QuantityType.WaterLevelAtCrest && es == ElementSet.Structures) return FunctionAttributes.StandardNames.StructureWaterLevelAtCrest;
            if (qt == QuantityType.Setpoint && es == ElementSet.Structures) return FunctionAttributes.StandardNames.StructureSetPoint;
            if (qt == QuantityType.SuctionSideLevel && es == ElementSet.Pumps) return "nonstandard_pump_suction_side";
            if (qt == QuantityType.DeliverySideLevel && es == ElementSet.Pumps) return "nonstandard_pump_delivery_side";
            if (qt == QuantityType.PumpHead && es == ElementSet.Pumps) return "nonstandard_pump_head";
            if (qt == QuantityType.ActualPumpStage && es == ElementSet.Pumps) return "nonstandard_pump_stage";
            if (qt == QuantityType.PumpCapacity && es == ElementSet.Pumps) return "nonstandard_pump_capacity";
            if (qt == QuantityType.ReductionFactor && es == ElementSet.Pumps) return "nonstandard_pump_reduction_factor";
            if (qt == QuantityType.PumpDischarge && es == ElementSet.Pumps) return "nonstandard_pump_discharge";
            if (qt == QuantityType.WaterLevel && es == ElementSet.Observations) return FunctionAttributes.StandardNames.WaterLevel;
            if (qt == QuantityType.WaterDepth && es == ElementSet.Observations) return FunctionAttributes.StandardNames.WaterDepth;
            if (qt == QuantityType.Discharge && es == ElementSet.Observations) return FunctionAttributes.StandardNames.WaterDischarge;
            if (qt == QuantityType.Velocity && es == ElementSet.Observations) return FunctionAttributes.StandardNames.WaterVelocity;
            if (qt == QuantityType.Salinity && es == ElementSet.Observations) return FunctionAttributes.StandardNames.WaterSalinity;
            if (qt == QuantityType.Dispersion && es == ElementSet.Observations) return FunctionAttributes.StandardNames.SaltDispersion;
            if (qt == QuantityType.Volume && es == ElementSet.Observations) return FunctionAttributes.StandardNames.WaterVolume;
            if (qt == QuantityType.WaterLevel && es == ElementSet.Retentions) return FunctionAttributes.StandardNames.WaterLevel;
            if (qt == QuantityType.Volume && es == ElementSet.Retentions) return FunctionAttributes.StandardNames.WaterVolume;
            if (qt == QuantityType.NegativeDepth && es == ElementSet.GridpointsOnBranches) return "nonstandard_negative_depth";
            if (qt == QuantityType.NoIteration && es == ElementSet.GridpointsOnBranches) return "nonstandard_no_iteration";
            if (qt == QuantityType.TimeStepEstimation && es == ElementSet.ReachSegElmSet) return "nonstandard_timestep_estimation";
            if (qt == QuantityType.Discharge && es == ElementSet.Laterals) return FunctionAttributes.StandardNames.WaterDischarge;
            if (qt == QuantityType.WaterLevel && es == ElementSet.Laterals) return FunctionAttributes.StandardNames.WaterLevel;
            if (qt == QuantityType.FiniteGridType && es == ElementSet.FiniteVolumeGridOnGridPoints) return "nonstandard_finite_grid_type";
            if (qt == QuantityType.Discharge && es == ElementSet.FiniteVolumeGridOnGridPoints) return FunctionAttributes.StandardNames.WaterDischarge;
            if (qt == QuantityType.Discharge && es == ElementSet.FiniteVolumeGridOnReachSegments) return FunctionAttributes.StandardNames.WaterDischarge;
            if (qt == QuantityType.Volume && es == ElementSet.FiniteVolumeGridOnGridPoints) return FunctionAttributes.StandardNames.WaterVolume;
            if (qt == QuantityType.Volume && es == ElementSet.FiniteVolumeGridOnReachSegments) return FunctionAttributes.StandardNames.WaterVolume;
            if (qt == QuantityType.SurfaceArea && es == ElementSet.FiniteVolumeGridOnGridPoints) return FunctionAttributes.StandardNames.WaterSurfaceArea;
            if (qt == QuantityType.SurfaceArea && es == ElementSet.FiniteVolumeGridOnReachSegments) return FunctionAttributes.StandardNames.WaterSurfaceArea;
            if (qt == QuantityType.Velocity && es == ElementSet.FiniteVolumeGridOnGridPoints) return FunctionAttributes.StandardNames.WaterVelocity;
            if (qt == QuantityType.Velocity && es == ElementSet.FiniteVolumeGridOnReachSegments) return FunctionAttributes.StandardNames.WaterVelocity;
            if (qt == QuantityType.FlowChezy && es == ElementSet.FiniteVolumeGridOnGridPoints) return "nonstandard_flow_chezy";
            if (qt == QuantityType.FlowChezy && es == ElementSet.FiniteVolumeGridOnReachSegments) return "nonstandard_flow_chezy";
            if (qt == QuantityType.QLat && es == ElementSet.LateralsOnGridPoints) return "nonstandard_water_discharge_per_lateral";
            if (qt == QuantityType.QLat && es == ElementSet.LateralsOnReachSegments) return "nonstandard_water_discharge_per_lateral";

            if (qt == QuantityType.BalVolume && es == ElementSet.ModelWide) return "water_balance_total_volume";
            if (qt == QuantityType.BalError && es == ElementSet.ModelWide) return "water_balance_volume_error";
            if (qt == QuantityType.BalStorage && es == ElementSet.ModelWide) return "water_balance_total_storage";
            
            if (qt == QuantityType.BalBoundariesIn && es == ElementSet.ModelWide) return "water_balance_boundaries_in";
            if (qt == QuantityType.BalBoundariesOut && es == ElementSet.ModelWide) return "water_balance_boundaries_out";
            if (qt == QuantityType.BalBoundariesTot && es == ElementSet.ModelWide) return "water_balance_boundaries_total";

            if (qt == QuantityType.BalLatIn && es == ElementSet.ModelWide) return "water_balance_lateral_discharge_in";
            if (qt == QuantityType.BalLatOut && es == ElementSet.ModelWide) return "water_balance_lateral_discharge_out";
            if (qt == QuantityType.BalLatTot && es == ElementSet.ModelWide) return "water_balance_lateral_discharge_total";

            if (qt == QuantityType.Bal2d1dIn && es == ElementSet.ModelWide) return  "water_balance_lateral_1d2d_discharge_in";
            if (qt == QuantityType.Bal2d1dOut && es == ElementSet.ModelWide) return "water_balance_lateral_1d2d_discharge_out";
            if (qt == QuantityType.Bal2d1dTot && es == ElementSet.ModelWide) return "water_balance_lateral_1d2d_discharge_total";

            if (qt == QuantityType.Temperature && es == ElementSet.Observations) return "temperature_observations";
            if (qt == QuantityType.Temperature && es == ElementSet.GridpointsOnBranches) return "temperature_gridpointsonbranches";

            if (qt == QuantityType.TotalHeatFlux && es == ElementSet.GridpointsOnBranches) return "totalheatflux_gridpointsonbranches";
            if (qt == QuantityType.RadFluxClearSky && es == ElementSet.GridpointsOnBranches) return "radfluxclearsky_gridpointsonbranches";
            if (qt == QuantityType.HeatLossConv && es == ElementSet.GridpointsOnBranches) return "heatlossconv_gridpointsonbranches";
            if (qt == QuantityType.NetSolarRad && es == ElementSet.GridpointsOnBranches) return "netsolarred_gridpointsonbranches";
            if (qt == QuantityType.EffectiveBackRad && es == ElementSet.GridpointsOnBranches) return "effectivebackrad_gridpointsonbranches";
            if (qt == QuantityType.HeatLossEvap && es == ElementSet.GridpointsOnBranches) return "heatlossevap_gridpointsonbranches";
            if (qt == QuantityType.HeatLossForcedEvap && es == ElementSet.GridpointsOnBranches) return "heatlossforcedevap_gridpointsonbranches";
            if (qt == QuantityType.HeatLossFreeEvap && es == ElementSet.GridpointsOnBranches) return "heatlossfreeevap_gridpointsonbranches";
            if (qt == QuantityType.HeatLossForcedConv && es == ElementSet.GridpointsOnBranches) return "heatlossforcedconv_gridpointsonbranches";
            if (qt == QuantityType.HeatLossFreeConv && es == ElementSet.GridpointsOnBranches) return "heatlossfreeconv_gridpointsonbranches";

            throw new ArgumentException("Unknown Quantity/Element set combination");
        }

        public static string GetStandardFeatureName(ElementSet es)
        {
            if (es == ElementSet.Structures) return FunctionAttributes.StandardFeatureNames.Structure;
            if (es == ElementSet.Retentions) return FunctionAttributes.StandardFeatureNames.Retention;
            if (es == ElementSet.ReachSegElmSet) return FunctionAttributes.StandardFeatureNames.ReachSegment;
            if (es == ElementSet.Observations) return FunctionAttributes.StandardFeatureNames.ObservationPoint;
            if (es == ElementSet.Laterals) return FunctionAttributes.StandardFeatureNames.LateralSource;
            if (es == ElementSet.GridpointsOnBranches) return FunctionAttributes.StandardFeatureNames.GridPoint;

            throw new ArgumentException("Method not implemented (yet) for ElementSet type: e" + es);
        }
    }
}