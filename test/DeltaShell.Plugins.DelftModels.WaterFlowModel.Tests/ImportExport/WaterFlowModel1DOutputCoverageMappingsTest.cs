using System;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class WaterFlowModel1DOutputCoverageMappingsTest
    {
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevel, WaterFlowModelParameterNames.LocationWaterLevel)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile, WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischarge, WaterFlowModelParameterNames.LateralDischarge)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.ObservationsFile, WaterFlowModel1DOutputFileConstants.VariableNames.WaterDepth, WaterFlowModelParameterNames.ObservationPointWaterDepth)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.ReachSegmentsFile, WaterFlowModel1DOutputFileConstants.VariableNames.Froude, WaterFlowModelParameterNames.BranchFroudeNumber)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.WaterTemperature, WaterFlowModelParameterNames.LocationTemperature)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.TotalHeatFlux, WaterFlowModelParameterNames.LocationTotalHeatFlux)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.RadFluxClearSky, WaterFlowModelParameterNames.LocationRadFluxClearSky)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.HeatLossConv, WaterFlowModelParameterNames.LocationHeatLossConv)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.NetSolarRad, WaterFlowModelParameterNames.LocationNetSolarRad)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.EffectiveBackRad, WaterFlowModelParameterNames.LocationEffectiveBackRad)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.HeatLossEvap, WaterFlowModelParameterNames.LocationHeatLossEvap)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossForcedEvap, WaterFlowModelParameterNames.LocationHeatLossForcedEvap)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossFreeEvap, WaterFlowModelParameterNames.LocationHeatLossFreeEvap)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossForcedConv, WaterFlowModelParameterNames.LocationHeatLossForcedConv)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossFreeConv, WaterFlowModelParameterNames.LocationHeatLossFreeConv)]
        public void TestGetMappingForVariableInFileReturnsExpectedMapping(string fileName, string variableName, string expectedCoverageName)
        {
            var coverageName = WaterFlowModel1DOutputCoverageMappings.GetMappingForVariable(fileName, variableName, AggregationOptions.Current);
            Assert.AreEqual(coverageName, expectedCoverageName);

            coverageName = WaterFlowModel1DOutputCoverageMappings.GetMappingForVariable(fileName, variableName, AggregationOptions.Maximum);
            Assert.AreEqual(coverageName, string.Format("{0} ({1})", expectedCoverageName , Enum.GetName(typeof(AggregationOptions), AggregationOptions.Maximum)));

            coverageName = WaterFlowModel1DOutputCoverageMappings.GetMappingForVariable(fileName, variableName, AggregationOptions.Minimum);
            Assert.AreEqual(coverageName, string.Format("{0} ({1})", expectedCoverageName , Enum.GetName(typeof(AggregationOptions), AggregationOptions.Minimum)));

            coverageName = WaterFlowModel1DOutputCoverageMappings.GetMappingForVariable(fileName, variableName, AggregationOptions.Average);
            Assert.AreEqual(coverageName, string.Format("{0} ({1})", expectedCoverageName , Enum.GetName(typeof(AggregationOptions), AggregationOptions.Average)));
        }

        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModelParameterNames.LocationWaterLevel, WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevel)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile, WaterFlowModelParameterNames.LateralDischarge, WaterFlowModel1DOutputFileConstants.VariableNames.WaterDischarge)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.ObservationsFile, WaterFlowModelParameterNames.ObservationPointWaterDepth, WaterFlowModel1DOutputFileConstants.VariableNames.WaterDepth)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.ReachSegmentsFile, WaterFlowModelParameterNames.BranchFroudeNumber, WaterFlowModel1DOutputFileConstants.VariableNames.Froude)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModelParameterNames.LocationTemperature, WaterFlowModel1DOutputFileConstants.VariableNames.WaterTemperature)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.ObservationsFile, WaterFlowModelParameterNames.ObservationPointTemperature, WaterFlowModel1DOutputFileConstants.VariableNames.WaterTemperature)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModelParameterNames.LocationTotalHeatFlux, WaterFlowModel1DOutputFileConstants.VariableNames.TotalHeatFlux)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModelParameterNames.LocationRadFluxClearSky, WaterFlowModel1DOutputFileConstants.VariableNames.RadFluxClearSky)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModelParameterNames.LocationHeatLossConv, WaterFlowModel1DOutputFileConstants.VariableNames.HeatLossConv)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModelParameterNames.LocationNetSolarRad, WaterFlowModel1DOutputFileConstants.VariableNames.NetSolarRad)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModelParameterNames.LocationEffectiveBackRad, WaterFlowModel1DOutputFileConstants.VariableNames.EffectiveBackRad)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModelParameterNames.LocationHeatLossEvap, WaterFlowModel1DOutputFileConstants.VariableNames.HeatLossEvap)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModelParameterNames.LocationHeatLossForcedEvap, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossForcedEvap)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModelParameterNames.LocationHeatLossFreeEvap, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossFreeEvap)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModelParameterNames.LocationHeatLossForcedConv, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossForcedConv)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModelParameterNames.LocationHeatLossFreeConv, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossFreeConv)]
        public void TestGetMappingForCoverageReturnsExpectedMapping(string fileName, string coverageName, string expectedVariableName)
        {
            var variableName = WaterFlowModel1DOutputCoverageMappings.GetMappingForCoverage(fileName, coverageName);
            Assert.AreEqual(variableName, expectedVariableName);

            variableName = WaterFlowModel1DOutputCoverageMappings.GetMappingForCoverage(fileName, string.Format("{0} (Average)",coverageName));
            Assert.AreEqual(variableName, expectedVariableName);

            variableName = WaterFlowModel1DOutputCoverageMappings.GetMappingForCoverage(fileName, string.Format("{0} (Maximum)", coverageName));
            Assert.AreEqual(variableName, expectedVariableName);

            variableName = WaterFlowModel1DOutputCoverageMappings.GetMappingForCoverage(fileName, string.Format("{0} (Minimum)", coverageName));
            Assert.AreEqual(variableName, expectedVariableName);
        }
    }
}
