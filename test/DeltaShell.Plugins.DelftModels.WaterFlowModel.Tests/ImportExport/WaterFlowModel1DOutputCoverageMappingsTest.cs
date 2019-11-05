using System;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class WaterFlowModel1DOutputCoverageMappingsTest
    {
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevel, Model1DParameterNames.LocationWaterLevel)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile, WaterFlowModel1DOutputFileConstants.VariableNames.LateralActualDischarge, Model1DParameterNames.LateralActualDischarge)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile, WaterFlowModel1DOutputFileConstants.VariableNames.LateralDefinedDischarge, Model1DParameterNames.LateralDefinedDischarge)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile, WaterFlowModel1DOutputFileConstants.VariableNames.LateralDifference, Model1DParameterNames.LateralDifference)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile, WaterFlowModel1DOutputFileConstants.VariableNames.LateralWaterLevel, Model1DParameterNames.LateralWaterLevel)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.ObservationsFile, WaterFlowModel1DOutputFileConstants.VariableNames.WaterDepth, Model1DParameterNames.ObservationPointWaterDepth)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.ReachSegmentsFile, WaterFlowModel1DOutputFileConstants.VariableNames.Froude, Model1DParameterNames.BranchFroudeNumber)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.WaterTemperature, Model1DParameterNames.LocationTemperature)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.TotalHeatFlux, Model1DParameterNames.LocationTotalHeatFlux)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.RadFluxClearSky, Model1DParameterNames.LocationRadFluxClearSky)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.HeatLossConv, Model1DParameterNames.LocationHeatLossConv)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.NetSolarRad, Model1DParameterNames.LocationNetSolarRad)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.EffectiveBackRad, Model1DParameterNames.LocationEffectiveBackRad)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.HeatLossEvap, Model1DParameterNames.LocationHeatLossEvap)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossForcedEvap, Model1DParameterNames.LocationHeatLossForcedEvap)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossFreeEvap, Model1DParameterNames.LocationHeatLossFreeEvap)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossForcedConv, Model1DParameterNames.LocationHeatLossForcedConv)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossFreeConv, Model1DParameterNames.LocationHeatLossFreeConv)]
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

        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, Model1DParameterNames.LocationWaterLevel, WaterFlowModel1DOutputFileConstants.VariableNames.WaterLevel)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile, Model1DParameterNames.LateralActualDischarge, WaterFlowModel1DOutputFileConstants.VariableNames.LateralActualDischarge)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile, Model1DParameterNames.LateralDefinedDischarge, WaterFlowModel1DOutputFileConstants.VariableNames.LateralDefinedDischarge)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile, Model1DParameterNames.LateralDifference, WaterFlowModel1DOutputFileConstants.VariableNames.LateralDifference)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.LateralsFile, Model1DParameterNames.LateralWaterLevel, WaterFlowModel1DOutputFileConstants.VariableNames.LateralWaterLevel)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.ObservationsFile, Model1DParameterNames.ObservationPointWaterDepth, WaterFlowModel1DOutputFileConstants.VariableNames.WaterDepth)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.ReachSegmentsFile, Model1DParameterNames.BranchFroudeNumber, WaterFlowModel1DOutputFileConstants.VariableNames.Froude)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, Model1DParameterNames.LocationTemperature, WaterFlowModel1DOutputFileConstants.VariableNames.WaterTemperature)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.ObservationsFile, Model1DParameterNames.ObservationPointTemperature, WaterFlowModel1DOutputFileConstants.VariableNames.WaterTemperature)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, Model1DParameterNames.LocationTotalHeatFlux, WaterFlowModel1DOutputFileConstants.VariableNames.TotalHeatFlux)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, Model1DParameterNames.LocationRadFluxClearSky, WaterFlowModel1DOutputFileConstants.VariableNames.RadFluxClearSky)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, Model1DParameterNames.LocationHeatLossConv, WaterFlowModel1DOutputFileConstants.VariableNames.HeatLossConv)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, Model1DParameterNames.LocationNetSolarRad, WaterFlowModel1DOutputFileConstants.VariableNames.NetSolarRad)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, Model1DParameterNames.LocationEffectiveBackRad, WaterFlowModel1DOutputFileConstants.VariableNames.EffectiveBackRad)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, Model1DParameterNames.LocationHeatLossEvap, WaterFlowModel1DOutputFileConstants.VariableNames.HeatLossEvap)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, Model1DParameterNames.LocationHeatLossForcedEvap, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossForcedEvap)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, Model1DParameterNames.LocationHeatLossFreeEvap, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossFreeEvap)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, Model1DParameterNames.LocationHeatLossForcedConv, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossForcedConv)]
        [TestCase(WaterFlowModel1DOutputFileConstants.FileNames.GridPointsFile, Model1DParameterNames.LocationHeatLossFreeConv, WaterFlowModel1DOutputFileConstants.VariableNames.HeatlossFreeConv)]
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
