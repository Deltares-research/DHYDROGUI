using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class WaterFlowModel1DOutputCoverageMappings
    {
        #region public accessor methods

        public static string GetMappingForVariable(string fileName, string variableName, AggregationOptions aggregationOption)
        {
            var fileMappings = LookupTable.FirstOrDefault(lut => lut.FileName == fileName);
            if (fileMappings == null) return null;

            string coverageName;
            return fileMappings.Mappings.TryGetValue(variableName, out coverageName) ? aggregationOption == AggregationOptions.Current ? coverageName : string.Format("{0} ({1})", coverageName, Enum.GetName(typeof(AggregationOptions), aggregationOption))  : null;
        }

        public static string GetMappingForCoverage(string fileName, string coverageName)
        {
            var fileMappings = LookupTable.FirstOrDefault(lut => lut.FileName == fileName);
            if (fileMappings == null) return null;
            
            var mappings = fileMappings.Mappings;
            var mappedCoverageName = GetCoverageNameWithoutAggregationOptions(coverageName);

            return mappings.FirstOrDefault(m => m.Value == mappedCoverageName).Key;
        }

        #endregion

        #region private helper constructs

        private static string GetCoverageNameWithoutAggregationOptions(string coverageName)
        {
            return coverageName
                .Replace("(Current)", string.Empty)
                .Replace("(Average)", string.Empty)
                .Replace("(Maximum)", string.Empty)
                .Replace("(Minimum)", string.Empty)
                .Trim();
        }

        private static readonly IList<LookupTable> LookupTable = new List<LookupTable>()
        {
            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.NegativeDepthCount,                  WaterFlowModelParameterNames.SimulationInfoNegativeDepthDisplayName},
                {WaterFlowModel1DOutputFileConstants.VariableNames.NoInteration,                        WaterFlowModelParameterNames.SimulationInfoNumberOfIterationsDisplayName},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDensity,                        WaterFlowModelParameterNames.LocationDensity},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDepth,                          WaterFlowModelParameterNames.LocationWaterDepth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLateralFlow2D1D,                WaterFlowModelParameterNames.LocationQTotal_1d2d},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevel,                          WaterFlowModelParameterNames.LocationWaterLevel},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterSalinity,                       WaterFlowModelParameterNames.LocationSaltConcentration},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterTotalArea,                      WaterFlowModelParameterNames.LocationTotalArea},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterTotalWidth,                     WaterFlowModelParameterNames.LocationTotalWidth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterVolume,                         WaterFlowModelParameterNames.LocationVolume},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLateralFlowAtNode,              WaterFlowModelParameterNames.LocationLateralAtNodes},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterTemperature,                    WaterFlowModelParameterNames.LocationTemperature},
                {WaterFlowModel1DOutputFileConstants.VariableNames.TotalHeatFlux,                       WaterFlowModelParameterNames.LocationTotalHeatFlux},
                {WaterFlowModel1DOutputFileConstants.VariableNames.RadFluxClearSky,                     WaterFlowModelParameterNames.LocationRadFluxClearSky},
                {WaterFlowModel1DOutputFileConstants.VariableNames.HeatLossConv,                        WaterFlowModelParameterNames.LocationHeatLossConv},
                {WaterFlowModel1DOutputFileConstants.VariableNames.NetSolarRad,                         WaterFlowModelParameterNames.LocationNetSolarRad},
                {WaterFlowModel1DOutputFileConstants.VariableNames.EffectiveBackRad,                    WaterFlowModelParameterNames.LocationEffectiveBackRad},
                {WaterFlowModel1DOutputFileConstants.VariableNames.HeatLossEvap,                        WaterFlowModelParameterNames.LocationHeatLossEvap},
                {WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossForcedEvap,                  WaterFlowModelParameterNames.LocationHeatLossForcedEvap},
                {WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossFreeEvap,                    WaterFlowModelParameterNames.LocationHeatLossFreeEvap},
                {WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossForcedConv,                  WaterFlowModelParameterNames.LocationHeatLossForcedConv},
                {WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossFreeConv,                    WaterFlowModelParameterNames.LocationHeatLossFreeConv}

            }),

            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischarge,                      WaterFlowModelParameterNames.LateralDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevel,                          WaterFlowModelParameterNames.LateralWaterLevel},
            }),

            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.ObservationsFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDepth,                          WaterFlowModelParameterNames.ObservationPointWaterDepth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischarge,                      WaterFlowModelParameterNames.ObservationPointDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDispersion,                     WaterFlowModelParameterNames.ObservationPointSaltDispersion},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevel,                          WaterFlowModelParameterNames.ObservationPointWaterLevel},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterSalinity,                       WaterFlowModelParameterNames.ObservationPointSaltConcentration},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterVelocity,                       WaterFlowModelParameterNames.ObservationPointVelocity},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterVolume,                         WaterFlowModelParameterNames.ObservationPointVolume},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterTemperature,                    WaterFlowModelParameterNames.ObservationPointTemperature}
            }),

            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.ReachSegmentsFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.Froude,                              WaterFlowModelParameterNames.BranchFroudeNumber},
                {WaterFlowModel1DOutputFileConstants.VariableNames.TimeStepEstimation,                  WaterFlowModelParameterNames.SimulationInfoTimeStepEstimationDisplayName},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterChezy,                          WaterFlowModelParameterNames.BranchRoughness},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterChezyFP1,                       WaterFlowModelParameterNames.FloodPlain1 + WaterFlowModelParameterNames.BranchRoughness},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterChezyFP2,                       WaterFlowModelParameterNames.FloodPlain2 + WaterFlowModelParameterNames.BranchRoughness},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterChezyMain,                      WaterFlowModelParameterNames.MainChannel + WaterFlowModelParameterNames.BranchRoughness},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterConveyance,                     WaterFlowModelParameterNames.BranchConveyance},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischarge,                      WaterFlowModelParameterNames.BranchDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischargeFP1,                   WaterFlowModelParameterNames.FloodPlain1 + WaterFlowModelParameterNames.BranchDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischargeFP2,                   WaterFlowModelParameterNames.FloodPlain2 + WaterFlowModelParameterNames.BranchDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischargeMain,                  WaterFlowModelParameterNames.MainChannel + WaterFlowModelParameterNames.BranchDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDispersion,                     WaterFlowModelParameterNames.BranchSaltDispersion},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterEnergyLevel,                    WaterFlowModelParameterNames.BranchEnergyHeadLevel},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowArea,                       WaterFlowModelParameterNames.BranchFlowArea},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowAreaFP1,                    WaterFlowModelParameterNames.FloodPlain1 + WaterFlowModelParameterNames.BranchFlowArea},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowAreaFP2,                    WaterFlowModelParameterNames.FloodPlain2 + WaterFlowModelParameterNames.BranchFlowArea},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowAreaMain,                   WaterFlowModelParameterNames.MainChannel + WaterFlowModelParameterNames.BranchFlowArea},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowWidthFP1,                   WaterFlowModelParameterNames.FloodPlain1 + WaterFlowModelParameterNames.SubSectionFlowWidth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowWidthFP2,                   WaterFlowModelParameterNames.FloodPlain2 + WaterFlowModelParameterNames.SubSectionFlowWidth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowWidthMain,                  WaterFlowModelParameterNames.MainChannel + WaterFlowModelParameterNames.SubSectionFlowWidth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterHydraulicRadius,                WaterFlowModelParameterNames.SubSectionHydraulicRadius},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterHydraulicRadiusFP1,             WaterFlowModelParameterNames.FloodPlain1 + WaterFlowModelParameterNames.SubSectionHydraulicRadius},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterHydraulicRadiusFP2,             WaterFlowModelParameterNames.FloodPlain2 + WaterFlowModelParameterNames.SubSectionHydraulicRadius},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterHydraulicRadiusMain,            WaterFlowModelParameterNames.MainChannel + WaterFlowModelParameterNames.SubSectionHydraulicRadius},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevelGradient,                  WaterFlowModelParameterNames.BranchWaterLevelGradient},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterVelocity,                       WaterFlowModelParameterNames.BranchVelocity},
            }),

            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.RetentionsFile, new Dictionary<string, string>()
            {
                // {[variable name in file],                                                            [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevel,                          WaterFlowModelParameterNames.RetentionWaterLevel},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterVolume,                         WaterFlowModelParameterNames.RetentionVolume},
            }),
            
            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.StructuresFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.PressureDifference,                  WaterFlowModelParameterNames.StructurePressureDifference},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureCrestLevel,                 WaterFlowModelParameterNames.StructureCrestLevel},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureCrestWidth,                 WaterFlowModelParameterNames.StructureCrestWidth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureGateLowerEdgeLevel,         WaterFlowModelParameterNames.StructureGateLevel},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureGateOpeningHeight,          WaterFlowModelParameterNames.StructureOpeningHeight},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureSetPoint,                   WaterFlowModelParameterNames.StructureSetPoint},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureValveOpening,               WaterFlowModelParameterNames.StructureValveOpening},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureWaterHead,                  WaterFlowModelParameterNames.StructureHeadDifference},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureWaterLevelAtCrest,          WaterFlowModelParameterNames.StructureWaterLevelAtCrest},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureWaterLevelDown,             WaterFlowModelParameterNames.StructureWaterlevelDown},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureWaterLevelUp,               WaterFlowModelParameterNames.StructureWaterlevelUp},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischarge,                      WaterFlowModelParameterNames.StructureDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowArea,                       WaterFlowModelParameterNames.StructureFlowArea},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterVelocity,                       WaterFlowModelParameterNames.StructureVelocity},
            }),

            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.PumpsFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpSuctionSide,                     WaterFlowModelParameterNames.PumpSuctionSide},
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpDeliverySide,                    WaterFlowModelParameterNames.PumpDeliverySide},
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpHead,                            WaterFlowModelParameterNames.PumpHead},
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpStage,                           WaterFlowModelParameterNames.PumpStage},
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpReductionFactor,                 WaterFlowModelParameterNames.PumpReductionFactor},
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpCapacity,                        WaterFlowModelParameterNames.PumpCapacity},
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpDischarge,                       WaterFlowModelParameterNames.PumpDischarge}
            }),

            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.WaterBalanceFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalance2D1DIn,                  WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeIn},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalance2D1DOut,                 WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeOut},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalance2D1DTotal,               WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeTotal},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceBoundariesIn,            WaterFlowModelParameterNames.SimulationInfoWaterBalanceBoundariesIn},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceBoundariesOut,           WaterFlowModelParameterNames.SimulationInfoWaterBalanceBoundariesOut},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceBoundariesTotal,         WaterFlowModelParameterNames.SimulationInfoWaterBalanceBoundariesTotal},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceError,                   WaterFlowModelParameterNames.SimulationInfoWaterBalanceVolumeError},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceLateralIn,               WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateralDischargeIn},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceLateralOut,              WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateralDischargeOut},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceLateralTotal,            WaterFlowModelParameterNames.SimulationInfoWaterBalanceLateralDischargeTotal},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceStorage,                 WaterFlowModelParameterNames.SimulationInfoWaterBalanceTotalStorage},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceVolume,                  WaterFlowModelParameterNames.SimulationInfoWaterBalanceTotalVolume},
            }),
        };
    }

    internal class LookupTable
    {
        public string FileName { get; private set; }
        public Dictionary<string, string> Mappings { get; private set; }

        public LookupTable(string fileName, Dictionary<string, string> mappings)
        {
            FileName = fileName;
            Mappings = mappings;
        }
    }

    #endregion
}
