using System;
using System.Collections.Generic;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelPropertySetterTest
    {
        [Test]
        public void GivenDataModelWithCategoryThatHasAnUnknownHeader_WhenSettingProperties_ThenLogMessageIsReturned()
        {
            // Given
            var unkownHeader = "Unkown Header";
            var unknownCategory = new DelftIniCategory(unkownHeader);
            var categories = new List<DelftIniCategory> { unknownCategory };

            var errorReport = new List<string>();
            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add($"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            // When
            Action setWaterFlowModelProperties = () => WaterFlowModelPropertySetter.SetWaterFlowModelProperties(categories, new WaterFlowModel1D(), CreateAndAddErrorReport);

            // Then
            var expectedMessage = string.Format(Resources.WaterFlowModelPropertySetter_SetWaterFlowModelProperties_There_is_unrecognized_data_read_from_the_md1d_file_with_header___0___, unkownHeader);
            TestHelper.AssertAtLeastOneLogMessagesContains(setWaterFlowModelProperties, expectedMessage);
        }

        /// <summary>
        /// GIVEN a WaterFlow1DOutputSettingData with all EngineParameters set to None
        ///   AND a dataAccessModel describing multiple EngineParameter with some Aggregate options not None
        /// WHEN WaterFlowModelPropertySetter SetOutputProperties is called with these parameters
        /// THEN These engine property is set to the specified aggregate option
        ///  AND all other engine parameters are None
        /// </summary>
        [Test]
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


            var validCategoriesSet = new List<DelftIniCategory> { new DelftIniCategory(GeneralRegion.IniHeader) };
            validCategoriesSet.Add(GetResultsCategory(ElementSet.GridpointsOnBranches, resultsNodesProperties));
            validCategoriesSet[0].AddProperty("Lateral1D2D", (int)AggregationOptions.None, "Tenderloin");

            validCategoriesSet.Add(GetResultsCategory(ElementSet.ReachSegElmSet, resultsBranchesProperties));
            validCategoriesSet[1].AddProperty(QuantityType.Dispersion.ToString(), (int)AggregationOptions.None, "Dispersion is a bit weird");

            validCategoriesSet.Add(GetResultsCategory(ElementSet.Structures, resultsStructuresProperties));
            validCategoriesSet.Add(GetResultsCategory(ElementSet.Observations, resultsObservationProperties));
            validCategoriesSet.Add(GetResultsCategory(ElementSet.Retentions, resultsRetentionsProperties));
            validCategoriesSet.Add(GetResultsCategory(ElementSet.Laterals, resultsLateralsProperties));
            validCategoriesSet.Add(GetResultsCategory(ElementSet.ModelWide, resultsWaterBalanceProperties));
            validCategoriesSet.Add(GetResultsCategory(ElementSet.Pumps, resultsPumps));

            var model = new WaterFlowModel1D();
            var outputSettings = model.OutputSettings;

            foreach (var eParam in outputSettings.EngineParameters)
                eParam.AggregationOptions = AggregationOptions.None;

            // When
            WaterFlowModelPropertySetter.SetWaterFlowModelProperties(validCategoriesSet, model, (s, list) => {});

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
        }

        private static DelftIniCategory GetResultsCategory(ElementSet resultsHeader, Dictionary<QuantityType, AggregationOptions> keyValuePairs)
        {
            var category = new DelftIniCategory(MappingElementSetToRegionHeader[resultsHeader]);

            foreach (var keyValuePair in keyValuePairs)
                category.AddProperty(keyValuePair.Key.ToString(), (int) keyValuePair.Value, "Some really interesting comment.");

            return category;
        }

        private static readonly IDictionary<ElementSet, string> MappingElementSetToRegionHeader =
            new Dictionary<ElementSet, string>
            {
                [ElementSet.GridpointsOnBranches] = ModelDefinitionsRegion.ResultsNodesHeader,
                [ElementSet.ReachSegElmSet] = ModelDefinitionsRegion.ResultsBranchesHeader,
                [ElementSet.Structures] = ModelDefinitionsRegion.ResultsStructuresHeader,
                [ElementSet.Pumps] = ModelDefinitionsRegion.ResultsPumpsHeader,
                [ElementSet.Observations] = ModelDefinitionsRegion.ResultsObservationsPointsHeader,
                [ElementSet.Retentions] = ModelDefinitionsRegion.ResultsRetentionsHeader,
                [ElementSet.Laterals] = ModelDefinitionsRegion.ResultsLateralsHeader,
                [ElementSet.ModelWide] = ModelDefinitionsRegion.ResultsWaterBalanceHeader,
                [ElementSet.FiniteVolumeGridOnGridPoints] = ModelDefinitionsRegion.FiniteVolumeGridOnGridPoints
            };
    }
}