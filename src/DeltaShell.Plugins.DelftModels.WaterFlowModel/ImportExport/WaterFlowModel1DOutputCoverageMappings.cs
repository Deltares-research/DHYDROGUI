using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using AggregationOptions = DeltaShell.NGHS.IO.DataObjects.Model1D.AggregationOptions;

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
                {WaterFlowModel1DOutputFileConstants.VariableNames.NegativeDepthCount,                  Model1DParameterNames.SimulationInfoNegativeDepthDisplayName},
                {WaterFlowModel1DOutputFileConstants.VariableNames.NoInteration,                        Model1DParameterNames.SimulationInfoNumberOfIterationsDisplayName},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDensity,                        Model1DParameterNames.LocationDensity},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDepth,                          Model1DParameterNames.LocationWaterDepth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLateralFlow2D1D,                Model1DParameterNames.LocationQTotal_1d2d},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevel,                          Model1DParameterNames.LocationWaterLevel},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterSalinity,                       Model1DParameterNames.LocationSaltConcentration},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterTotalArea,                      Model1DParameterNames.LocationTotalArea},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterTotalWidth,                     Model1DParameterNames.LocationTotalWidth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterVolume,                         Model1DParameterNames.LocationVolume},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLateralFlowAtNode,              Model1DParameterNames.LocationLateralAtNodes},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterTemperature,                    Model1DParameterNames.LocationTemperature},
                {WaterFlowModel1DOutputFileConstants.VariableNames.TotalHeatFlux,                       Model1DParameterNames.LocationTotalHeatFlux},
                {WaterFlowModel1DOutputFileConstants.VariableNames.RadFluxClearSky,                     Model1DParameterNames.LocationRadFluxClearSky},
                {WaterFlowModel1DOutputFileConstants.VariableNames.HeatLossConv,                        Model1DParameterNames.LocationHeatLossConv},
                {WaterFlowModel1DOutputFileConstants.VariableNames.NetSolarRad,                         Model1DParameterNames.LocationNetSolarRad},
                {WaterFlowModel1DOutputFileConstants.VariableNames.EffectiveBackRad,                    Model1DParameterNames.LocationEffectiveBackRad},
                {WaterFlowModel1DOutputFileConstants.VariableNames.HeatLossEvap,                        Model1DParameterNames.LocationHeatLossEvap},
                {WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossForcedEvap,                  Model1DParameterNames.LocationHeatLossForcedEvap},
                {WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossFreeEvap,                    Model1DParameterNames.LocationHeatLossFreeEvap},
                {WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossForcedConv,                  Model1DParameterNames.LocationHeatLossForcedConv},
                {WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossFreeConv,                    Model1DParameterNames.LocationHeatLossFreeConv}

            }),

            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.LateralActualDischarge,              Model1DParameterNames.LateralActualDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.LateralDefinedDischarge,             Model1DParameterNames.LateralDefinedDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.LateralDifference,                   Model1DParameterNames.LateralDifference},
                {WaterFlowModel1DOutputFileConstants.VariableNames.LateralWaterLevel,                   Model1DParameterNames.LateralWaterLevel},
            }),

            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.ObservationsFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDepth,                          Model1DParameterNames.ObservationPointWaterDepth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischarge,                      Model1DParameterNames.ObservationPointDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDispersion,                     Model1DParameterNames.ObservationPointSaltDispersion},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevel,                          Model1DParameterNames.ObservationPointWaterLevel},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterSalinity,                       Model1DParameterNames.ObservationPointSaltConcentration},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterVelocity,                       Model1DParameterNames.ObservationPointVelocity},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterVolume,                         Model1DParameterNames.ObservationPointVolume},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterTemperature,                    Model1DParameterNames.ObservationPointTemperature}
            }),

            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.ReachSegmentsFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.Froude,                              Model1DParameterNames.BranchFroudeNumber},
                {WaterFlowModel1DOutputFileConstants.VariableNames.TimeStepEstimation,                  Model1DParameterNames.SimulationInfoTimeStepEstimationDisplayName},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterChezy,                          Model1DParameterNames.BranchRoughness},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterChezyFP1,                       Model1DParameterNames.FloodPlain1 + Model1DParameterNames.BranchRoughness},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterChezyFP2,                       Model1DParameterNames.FloodPlain2 + Model1DParameterNames.BranchRoughness},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterChezyMain,                      Model1DParameterNames.MainChannel + Model1DParameterNames.BranchRoughness},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterConveyance,                     Model1DParameterNames.BranchConveyance},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischarge,                      Model1DParameterNames.BranchDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischargeFP1,                   Model1DParameterNames.FloodPlain1 + Model1DParameterNames.BranchDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischargeFP2,                   Model1DParameterNames.FloodPlain2 + Model1DParameterNames.BranchDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischargeMain,                  Model1DParameterNames.MainChannel + Model1DParameterNames.BranchDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDispersion,                     Model1DParameterNames.BranchSaltDispersion},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterEnergyLevel,                    Model1DParameterNames.BranchEnergyHeadLevel},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowArea,                       Model1DParameterNames.BranchFlowArea},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowAreaFP1,                    Model1DParameterNames.FloodPlain1 + Model1DParameterNames.BranchFlowArea},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowAreaFP2,                    Model1DParameterNames.FloodPlain2 + Model1DParameterNames.BranchFlowArea},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowAreaMain,                   Model1DParameterNames.MainChannel + Model1DParameterNames.BranchFlowArea},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowWidthFP1,                   Model1DParameterNames.FloodPlain1 + Model1DParameterNames.SubSectionFlowWidth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowWidthFP2,                   Model1DParameterNames.FloodPlain2 + Model1DParameterNames.SubSectionFlowWidth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowWidthMain,                  Model1DParameterNames.MainChannel + Model1DParameterNames.SubSectionFlowWidth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterHydraulicRadius,                Model1DParameterNames.SubSectionHydraulicRadius},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterHydraulicRadiusFP1,             Model1DParameterNames.FloodPlain1 + Model1DParameterNames.SubSectionHydraulicRadius},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterHydraulicRadiusFP2,             Model1DParameterNames.FloodPlain2 + Model1DParameterNames.SubSectionHydraulicRadius},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterHydraulicRadiusMain,            Model1DParameterNames.MainChannel + Model1DParameterNames.SubSectionHydraulicRadius},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevelGradient,                  Model1DParameterNames.BranchWaterLevelGradient},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterVelocity,                       Model1DParameterNames.BranchVelocity},
            }),

            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.RetentionsFile, new Dictionary<string, string>()
            {
                // {[variable name in file],                                                            [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevel,                          Model1DParameterNames.RetentionWaterLevel},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterVolume,                         Model1DParameterNames.RetentionVolume},
            }),
            
            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.StructuresFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.PressureDifference,                  Model1DParameterNames.StructurePressureDifference},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureCrestLevel,                 Model1DParameterNames.StructureCrestLevel},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureCrestWidth,                 Model1DParameterNames.StructureCrestWidth},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureGateLowerEdgeLevel,         Model1DParameterNames.StructureGateLevel},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureGateOpeningHeight,          Model1DParameterNames.StructureOpeningHeight},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureSetPoint,                   Model1DParameterNames.StructureSetPoint},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureValveOpening,               Model1DParameterNames.StructureValveOpening},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureWaterHead,                  Model1DParameterNames.StructureHeadDifference},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureWaterLevelAtCrest,          Model1DParameterNames.StructureWaterLevelAtCrest},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureWaterLevelDown,             Model1DParameterNames.StructureWaterlevelDown},
                {WaterFlowModel1DOutputFileConstants.VariableNames.StructureWaterLevelUp,               Model1DParameterNames.StructureWaterlevelUp},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischarge,                      Model1DParameterNames.StructureDischarge},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterFlowArea,                       Model1DParameterNames.StructureFlowArea},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterVelocity,                       Model1DParameterNames.StructureVelocity},
            }),

            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.PumpsFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpSuctionSide,                     Model1DParameterNames.PumpSuctionSide},
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpDeliverySide,                    Model1DParameterNames.PumpDeliverySide},
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpHead,                            Model1DParameterNames.PumpHead},
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpStage,                           Model1DParameterNames.PumpStage},
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpReductionFactor,                 Model1DParameterNames.PumpReductionFactor},
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpCapacity,                        Model1DParameterNames.PumpCapacity},
                {WaterFlowModel1DOutputFileConstants.VariableNames.PumpDischarge,                       Model1DParameterNames.PumpDischarge}
            }),

            new LookupTable(WaterFlowModel1DOutputFileConstants.FileNames.WaterBalanceFile, new Dictionary<string, string>()
            {
             // {[variable name in file],                                                               [existing coverage tag]}
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalance2D1DIn,                  Model1DParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeIn},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalance2D1DOut,                 Model1DParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeOut},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalance2D1DTotal,               Model1DParameterNames.SimulationInfoWaterBalanceLateral1D2DDischargeTotal},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceBoundariesIn,            Model1DParameterNames.SimulationInfoWaterBalanceBoundariesIn},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceBoundariesOut,           Model1DParameterNames.SimulationInfoWaterBalanceBoundariesOut},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceBoundariesTotal,         Model1DParameterNames.SimulationInfoWaterBalanceBoundariesTotal},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceError,                   Model1DParameterNames.SimulationInfoWaterBalanceVolumeError},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceLateralIn,               Model1DParameterNames.SimulationInfoWaterBalanceLateralDischargeIn},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceLateralOut,              Model1DParameterNames.SimulationInfoWaterBalanceLateralDischargeOut},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceLateralTotal,            Model1DParameterNames.SimulationInfoWaterBalanceLateralDischargeTotal},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceStorage,                 Model1DParameterNames.SimulationInfoWaterBalanceTotalStorage},
                {WaterFlowModel1DOutputFileConstants.VariableNames.WaterBalanceVolume,                  Model1DParameterNames.SimulationInfoWaterBalanceTotalVolume},
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
