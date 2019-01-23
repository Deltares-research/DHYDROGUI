using System;
using System.Collections.Generic;
using System.Reflection;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class ModelDefinitionFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenDataModelWithCategoryThatHasAnUnknownHeader_WhenSettingProperties_ThenLogMessageIsReturned()
        {
            // Given
            var md1dFilePath = TestHelper.GetTestDataPath(Assembly.GetExecutingAssembly(), @"ModelDefinitionFileReaderTest\modelDefinitionWithUnknownCategory.md1d");
            var testFilePath = TestHelper.CreateLocalCopy(md1dFilePath);

            var errorReport = new List<string>();
            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add($"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            try
            {
                // When
                ModelDefinitionFileReader.SetWaterFlowModelProperties(testFilePath, new WaterFlowModel1D(), CreateAndAddErrorReport);

                // Then
                var expectedMessage = string.Format(Resources.WaterFlowModelPropertySetter_SetWaterFlowModelProperties_There_is_unrecognized_data_read_from_the_md1d_file_with_header___0___, "Unknown Header");
                Assert.That(errorReport.Count, Is.EqualTo(1));
                Assert.That(errorReport[0].Contains(expectedMessage));
            }
            finally
            {
                FileUtils.DeleteIfExists(testFilePath);
            }
        }


        /// <summary>
        /// GIVEN a WaterFlow1DOutputSettingData with all EngineParameters set to None
        ///   AND a dataAccessModel describing multiple EngineParameter with some Aggregate options not None
        /// WHEN ModelDefinitionFileReader SetOutputProperties is called with these parameters
        /// THEN These engine property is set to the specified aggregate option
        ///  AND all other engine parameters are None
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAWaterFlow1DOutputSettingDataWithAllEngineParametersSetToNoneAndADataAccessModelDescribingMultipleEngineParameterWithSomeAggregateOptionsNotNone_WhenWaterFlowModelPropertySetterSetOutputPropertiesIsCalledWithTheseParameters_ThenTheseEnginePropertyIsSetToTheSpecifiedAggregateOptionAndAllOtherEngineParametersAreNone()
        {
            // Given 
            // This DelftIniCategory set is equal to the provided RMM model: Epic: Import of SOBEK3 models
            var resultsNodesProperties = new Dictionary<QuantityType, AggregationOptions>
            {
                {QuantityType.Density            , AggregationOptions.None},
                {QuantityType.EffectiveBackRad   , AggregationOptions.None},
                {QuantityType.HeatLossConv       , AggregationOptions.None},
                {QuantityType.HeatLossEvap       , AggregationOptions.None},
                {QuantityType.HeatLossForcedConv , AggregationOptions.None},
                {QuantityType.HeatLossForcedEvap , AggregationOptions.None},
                {QuantityType.HeatLossFreeConv   , AggregationOptions.None},
                {QuantityType.HeatLossFreeEvap   , AggregationOptions.None},
                {QuantityType.LateralAtNodes     , AggregationOptions.None},
                {QuantityType.NegativeDepth      , AggregationOptions.None},
                {QuantityType.NetSolarRad        , AggregationOptions.None},
                {QuantityType.NoIteration        , AggregationOptions.None},
                {QuantityType.RadFluxClearSky    , AggregationOptions.None},
                {QuantityType.Salinity           , AggregationOptions.Current},
                {QuantityType.Temperature        , AggregationOptions.None},
                {QuantityType.TotalArea          , AggregationOptions.None},
                {QuantityType.TotalHeatFlux      , AggregationOptions.None},
                {QuantityType.TotalWidth         , AggregationOptions.None},
                {QuantityType.Volume             , AggregationOptions.None},
                {QuantityType.WaterDepth         , AggregationOptions.None},
                {QuantityType.WaterLevel         , AggregationOptions.Current}
            };

            var resultsBranchesProperties = new Dictionary<QuantityType, AggregationOptions>
            {
                {QuantityType.AreaFP1            , AggregationOptions.None},
                {QuantityType.AreaFP2            , AggregationOptions.None},
                {QuantityType.AreaMain           , AggregationOptions.None},
                {QuantityType.ChezyFP1           , AggregationOptions.None},
                {QuantityType.ChezyFP2           , AggregationOptions.None},
                {QuantityType.ChezyMain          , AggregationOptions.None},
                {QuantityType.Discharge          , AggregationOptions.Current},
                {QuantityType.DischargeFP1       , AggregationOptions.None},
                {QuantityType.DischargeFP2       , AggregationOptions.None},
                {QuantityType.DischargeMain      , AggregationOptions.None},
                {QuantityType.FlowArea           , AggregationOptions.None},
                {QuantityType.FlowChezy          , AggregationOptions.None},
                {QuantityType.FlowConv           , AggregationOptions.None},
                {QuantityType.FlowHydrad         , AggregationOptions.None},
                {QuantityType.Froude             , AggregationOptions.None},
                {QuantityType.HydradFP1          , AggregationOptions.None},
                {QuantityType.HydradFP2          , AggregationOptions.None},
                {QuantityType.HydradMain         , AggregationOptions.None},
                {QuantityType.TimeStepEstimation , AggregationOptions.None},
                {QuantityType.Velocity           , AggregationOptions.Current},
                {QuantityType.WaterLevelGradient , AggregationOptions.None},
                {QuantityType.WidthFP1           , AggregationOptions.None},
                {QuantityType.WidthFP2           , AggregationOptions.None},
                {QuantityType.WidthMain          , AggregationOptions.None}
            };

            var resultsStructuresProperties = new Dictionary<QuantityType, AggregationOptions>
            {
                {QuantityType.CrestLevel         , AggregationOptions.Current},
                {QuantityType.CrestWidth         , AggregationOptions.Current},
                {QuantityType.Discharge          , AggregationOptions.Current},
                {QuantityType.FlowArea           , AggregationOptions.None},
                {QuantityType.GateLowerEdgeLevel , AggregationOptions.Current},
                {QuantityType.GateOpeningHeight  , AggregationOptions.Current},
                {QuantityType.Head               , AggregationOptions.Current},
                {QuantityType.PressureDifference , AggregationOptions.Current},
                {QuantityType.ValveOpening       , AggregationOptions.None},
                {QuantityType.Velocity           , AggregationOptions.None},
                {QuantityType.WaterLevelAtCrest  , AggregationOptions.None},
                {QuantityType.WaterlevelDown     , AggregationOptions.Current},
                {QuantityType.WaterlevelUp       , AggregationOptions.Current}
            };

            var resultsObservationProperties = new Dictionary<QuantityType, AggregationOptions>
            {
                {QuantityType.Discharge   , AggregationOptions.Current},
                {QuantityType.Dispersion  , AggregationOptions.None},
                {QuantityType.Salinity    , AggregationOptions.Current},
                {QuantityType.Temperature , AggregationOptions.None},
                {QuantityType.Velocity    , AggregationOptions.Current},
                {QuantityType.Volume      , AggregationOptions.None},
                {QuantityType.WaterDepth  , AggregationOptions.None},
                {QuantityType.WaterLevel  , AggregationOptions.Current}
            };

            var resultsRetentionsProperties = new Dictionary<QuantityType, AggregationOptions>
            {
                {QuantityType.Volume     , AggregationOptions.None},
                {QuantityType.WaterLevel , AggregationOptions.None}
            };

            var resultsLateralsProperties = new Dictionary<QuantityType, AggregationOptions>
            {
                {QuantityType.ActualDischarge   , AggregationOptions.None},
                {QuantityType.DefinedDischarge  , AggregationOptions.None},
                {QuantityType.LateralDifference , AggregationOptions.None},
                {QuantityType.WaterLevel        , AggregationOptions.None}
            };

            var resultsWaterBalanceProperties = new Dictionary<QuantityType, AggregationOptions>
            {
                {QuantityType.Bal2d1dIn        , AggregationOptions.None},
                {QuantityType.Bal2d1dOut       , AggregationOptions.None},
                {QuantityType.Bal2d1dTot       , AggregationOptions.None},
                {QuantityType.BalBoundariesIn  , AggregationOptions.None},
                {QuantityType.BalBoundariesOut , AggregationOptions.None},
                {QuantityType.BalBoundariesTot , AggregationOptions.None},
                {QuantityType.BalError         , AggregationOptions.None},
                {QuantityType.BalLatIn         , AggregationOptions.None},
                {QuantityType.BalLatOut        , AggregationOptions.None},
                {QuantityType.BalLatTot        , AggregationOptions.None},
                {QuantityType.BalStorage       , AggregationOptions.None},
                {QuantityType.BalVolume        , AggregationOptions.None}
            };

            var resultsPumps = new Dictionary<QuantityType, AggregationOptions>
            {
                {QuantityType.ActualPumpStage   , AggregationOptions.None},
                {QuantityType.DeliverySideLevel , AggregationOptions.None},
                {QuantityType.PumpCapacity      , AggregationOptions.None},
                {QuantityType.PumpDischarge     , AggregationOptions.None},
                {QuantityType.PumpHead          , AggregationOptions.None},
                {QuantityType.ReductionFactor   , AggregationOptions.None},
                {QuantityType.SuctionSideLevel  , AggregationOptions.None}
            };

            // Error handling
            var errorHandlingHasBeenCalled = false;
            var loggedErrors = new List<string>();
            var errorHeader = "";

            Action<string, IList<string>> someErrorReportFunction =
                (_, msgs) =>
                {
                    errorHandlingHasBeenCalled = true;
                    loggedErrors.AddRange(msgs);
                };


            var model = new WaterFlowModel1D();
            var outputSettings = model.OutputSettings;

            foreach (var eParam in outputSettings.EngineParameters)
                eParam.AggregationOptions = AggregationOptions.None;

            // When
            var md1dFilePath = TestHelper.GetTestDataPath(Assembly.GetExecutingAssembly(), @"ModelDefinitionFileReaderTest\rmm_model.md1d");
            var testFilePath = TestHelper.CreateLocalCopy(md1dFilePath);
            ModelDefinitionFileReader.SetWaterFlowModelProperties(testFilePath, model, someErrorReportFunction);

            // Then
            foreach (var t in resultsNodesProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Key, ElementSet.GridpointsOnBranches).AggregationOptions,
                    Is.EqualTo(t.Value), $"Property name: {t.Key.ToString()}");

            // Check corner case: Lateral1D2D
            Assert.That(outputSettings.GetEngineParameter(QuantityType.QTotal_1d2d, ElementSet.GridpointsOnBranches).AggregationOptions,
                Is.EqualTo(AggregationOptions.None));

            foreach (var t in resultsBranchesProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Key, ElementSet.ReachSegElmSet).AggregationOptions,
                    Is.EqualTo(t.Value), $"Property name: {t.Key.ToString()}");

            // Check corner case: Dispersion
            Assert.That(outputSettings.GetEngineParameter(QuantityType.Dispersion, ElementSet.GridpointsOnBranches).AggregationOptions,
                Is.EqualTo(AggregationOptions.None));


            foreach (var t in resultsStructuresProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Key, ElementSet.Structures).AggregationOptions,
                    Is.EqualTo(t.Value), $"Property name: {t.Key.ToString()}");

            foreach (var t in resultsObservationProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Key, ElementSet.Observations).AggregationOptions,
                    Is.EqualTo(t.Value), $"Property name: {t.Key.ToString()}");

            foreach (var t in resultsRetentionsProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Key, ElementSet.Retentions).AggregationOptions,
                    Is.EqualTo(t.Value), $"Property name: {t.Key.ToString()}");

            foreach (var t in resultsLateralsProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Key, ElementSet.Laterals).AggregationOptions,
                    Is.EqualTo(t.Value), $"Property name: {t.Key.ToString()}");

            foreach (var t in resultsWaterBalanceProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Key, ElementSet.ModelWide).AggregationOptions,
                    Is.EqualTo(t.Value), $"Property name: {t.Key.ToString()}");

            foreach (var t in resultsPumps)
                Assert.That(outputSettings.GetEngineParameter(t.Key, ElementSet.Pumps).AggregationOptions,
                    Is.EqualTo(t.Value), $"Property name: {t.Key.ToString()}");

            // Check if no errors are reported.
            Assert.That(errorHandlingHasBeenCalled, Is.False, "Expected no calls to the error handling, but found at least one.");
        }
    }
}