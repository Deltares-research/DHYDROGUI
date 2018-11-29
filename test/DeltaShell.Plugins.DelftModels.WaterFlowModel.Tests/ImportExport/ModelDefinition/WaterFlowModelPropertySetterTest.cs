using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelPropertySetterTest
    {
        // SetTimeProperties
        private readonly DateTime defaultStartTime = new DateTime(2018, 11, 30, 15, 15, 0); // 2018-11-30 15:15:00
        private readonly DateTime defaultStopTime = new DateTime(2018, 12, 4, 21, 0, 0); // 2018-12-04 21:00:00
        private readonly TimeSpan defaultTimeStep = new TimeSpan(0, 0, 15, 0); // 15 minutes
        private readonly TimeSpan defaultGridPointsTimeStep = new TimeSpan(0, 0, 10, 0); // 10 minutes
        private readonly TimeSpan defaultStructuresTimeStep = new TimeSpan(0, 0, 5, 0); // 5 minutes

        [Test]
        public void GivenTimeSettingsDataModel_WhenSettingWaterFlowModelProperties_ThenTimeSettingsAreCorrect()
        {
            // Given
            var timeSettingsCategories = GetCorrectTimeSettingsDataModel();

            // When
            var model = new WaterFlowModel1D();
            WaterFlowModelPropertySetter.SetTimeProperties(timeSettingsCategories, model);

            // Then
            Assert.That(model.StartTime, Is.EqualTo(defaultStartTime));
            Assert.That(model.StopTime, Is.EqualTo(defaultStopTime));
            Assert.That(model.TimeStep, Is.EqualTo(defaultTimeStep));

            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.GridOutputTimeStep, Is.EqualTo(defaultGridPointsTimeStep));
            Assert.That(modelOutputSettings.StructureOutputTimeStep, Is.EqualTo(defaultStructuresTimeStep));
        }

        private IEnumerable<DelftIniCategory> GetCorrectTimeSettingsDataModel()
        {
            var timeSettingsCategory = new DelftIniCategory(ModelDefinitionsRegion.TimeHeader);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.StartTime.Key, defaultStartTime);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.StopTime.Key, defaultStopTime);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.TimeStep.Key, defaultTimeStep.TotalSeconds);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.OutTimeStepGridPoints.Key, defaultGridPointsTimeStep.TotalSeconds);
            timeSettingsCategory.AddProperty(ModelDefinitionsRegion.OutTimeStepStructures.Key, defaultStructuresTimeStep.TotalSeconds);
            return new List<DelftIniCategory> {timeSettingsCategory};
        }

        [Test]
        public void GivenDataModelWithoutTimeCategory_WhenSettingWaterFlowModelProperties_ThenTimeSettingsHaveNotChanged()
        {
            // Given
            var notTimeCategory = new DelftIniCategory(ModelDefinitionsRegion.AdvancedOptionsHeader);
            notTimeCategory.AddProperty(ModelDefinitionsRegion.StartTime.Key, defaultStartTime);
            var categories = new List<DelftIniCategory> {notTimeCategory};

            // When
            var model = new WaterFlowModel1D();
            var startTimeBefore = model.StartTime;
            var stopTimeBefore = model.StopTime;
            var timeStepBefore = model.TimeStep;
            var gridPointsTimeStepBefore = model.OutputSettings.GridOutputTimeStep;
            var structuresTimeStepBefore = model.OutputSettings.StructureOutputTimeStep;

            WaterFlowModelPropertySetter.SetTimeProperties(categories, model);

            // Then
            Assert.That(model.StartTime, Is.EqualTo(startTimeBefore));
            Assert.That(model.StopTime, Is.EqualTo(stopTimeBefore));
            Assert.That(model.TimeStep, Is.EqualTo(timeStepBefore));

            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.GridOutputTimeStep, Is.EqualTo(gridPointsTimeStepBefore));
            Assert.That(modelOutputSettings.StructureOutputTimeStep, Is.EqualTo(structuresTimeStepBefore));
        }

        [Test]
        public void WhenSettingWaterFlowModelPropertiesWithCategoriesEqualToNull_ThenTimeSettingsHaveNotChanged()
        {
            // When
            var model = new WaterFlowModel1D();
            var startTimeBefore = model.StartTime;
            var stopTimeBefore = model.StopTime;
            var timeStepBefore = model.TimeStep;
            var gridPointsTimeStepBefore = model.OutputSettings.GridOutputTimeStep;
            var structuresTimeStepBefore = model.OutputSettings.StructureOutputTimeStep;

            WaterFlowModelPropertySetter.SetTimeProperties(null, model);

            // Then
            Assert.That(model.StartTime, Is.EqualTo(startTimeBefore));
            Assert.That(model.StopTime, Is.EqualTo(stopTimeBefore));
            Assert.That(model.TimeStep, Is.EqualTo(timeStepBefore));

            var modelOutputSettings = model.OutputSettings;
            Assert.That(modelOutputSettings.GridOutputTimeStep, Is.EqualTo(gridPointsTimeStepBefore));
            Assert.That(modelOutputSettings.StructureOutputTimeStep, Is.EqualTo(structuresTimeStepBefore));
        }

        [Test]
        public void WhenSettingWaterFlowModelPropertiesWithoutAModel_ThenNoException()
        {
            try
            {
                WaterFlowModelPropertySetter.SetTimeProperties(null, null);
            }
            catch (Exception e)
            {
                Assert.Fail("No exception was expected, but got: " + e.Message);
            }
        }

        // SetOutputProperties
        /// <summary>
        /// GIVEN some simple model with WaterFlow1DOutputSettingData all set to None
        ///   AND a dataAccessModel describing a single EngineParameter with some Aggregate option not None
        /// WHEN WaterFlowModelPropertySetter SetOutputProperties is called with these parameters
        /// THEN This engine property is set to the specified aggregate option
        ///  AND all other engine parameters are None
        /// </summary>
        // ElementSet::BranchNodes - GridpointsOnBranches - ResultsNodes
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.AdaptedCrossSec)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.BedLevel)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.ChangeArea)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.CumChangeArea)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.Density)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.EffectiveBackRad)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.GrainSizeD50)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.GrainSizeD90)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.HeatLossConv)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.HeatLossEvap)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.HeatLossForcedConv)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.HeatLossForcedEvap)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.HeatLossFreeConv)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.HeatLossFreeEvap)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.IntSediTrans)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.Lateral1d2d)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.LateralOnNodes)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.MeanBedLevelMain)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.MorDepth)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.MorVelocity)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.MorWaterLevel)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.MorWidth)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.NegativeDepth)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.NetSolarRad)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.NoIteration)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.RadFluxClearSky)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.Salinity)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.SedimentTransport)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.SedimentTransportLeft)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.SedimentTransportRight)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.ShieldsParameter)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.TotalArea)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.Temperature)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.TotalHeatFlux)]
      //[TestCase(ElementSet.GridpointsOnBranches, QuantityType.Totalwidth)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.Volume)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.WaterDepth)]
        [TestCase(ElementSet.GridpointsOnBranches, QuantityType.WaterLevel)]

        // ElementSet::ReachSegElmSet - GridpointsOnBranches - ResultsBranches
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.AreaFP1              )]      
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.AreaFP2              )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.AreaMain             )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.ChezyFP1             )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.ChezyFP2             )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.ChezyMain            )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.Discharge            )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.DischargeFP1         )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.DischargeFP2         )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.DischargeMain        )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.Dispersion           )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.EnergyLevels         )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.F1                   )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.F3                   )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.F4                   )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.FlowArea             )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.FlowChezy            )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.FlowConv             )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.FlowHydrad           )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.FlowWidth            )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.Froude               )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.HydradFP1            )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.HydradFP2            )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.HydradMain           )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomAcceleration      )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomAdvection         )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomBedStress         )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomLateralCorrection )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomLosses            )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomPressure          )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomWindStress        )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.TimeStepEstimation   )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.Velocity             )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.WaterLevelGradient   )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.WidthFP1             )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.WidthFP2             )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.WidthMain            )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.WindVelocity         )]
      //[TestCase(ElementSet.ReachSegElmSet, QuantityType.WindDirection        )]

        // ElementSet::Structures - Structures - ResultsStructures
        [TestCase(ElementSet.Structures, QuantityType.CrestLevel          )]
        [TestCase(ElementSet.Structures, QuantityType.CrestWidth          )]
        [TestCase(ElementSet.Structures, QuantityType.Discharge           )]
        [TestCase(ElementSet.Structures, QuantityType.FlowArea            )]
        [TestCase(ElementSet.Structures, QuantityType.GateLowerEdgeLevel  )]
        [TestCase(ElementSet.Structures, QuantityType.GateOpeningHeight   )]
        [TestCase(ElementSet.Structures, QuantityType.Head                )]
        [TestCase(ElementSet.Structures, QuantityType.PressureDifference  )]
      //[TestCase(ElementSet.Structures, QuantityType.State               )]
        [TestCase(ElementSet.Structures, QuantityType.Velocity            )]
        [TestCase(ElementSet.Structures, QuantityType.WaterLevelAtCrest   )]
        //[TestCase(ElementSet.Structures, QuantityType.WaterLevelDown      )]
        //[TestCase(ElementSet.Structures, QuantityType.WaterLevelUp        )]

        // ElementSet::Pumps - Pumps - ResultsPumps
        [TestCase(ElementSet.Pumps, QuantityType.ActualPumpStage   )] 
        [TestCase(ElementSet.Pumps, QuantityType.DeliverySideLevel )]
        [TestCase(ElementSet.Pumps, QuantityType.PumpCapacity      )]
        [TestCase(ElementSet.Pumps, QuantityType.PumpDischarge     )]
        [TestCase(ElementSet.Pumps, QuantityType.PumpHead          )]
        [TestCase(ElementSet.Pumps, QuantityType.ReductionFactor   )]
        [TestCase(ElementSet.Pumps, QuantityType.SuctionSideLevel  )]

        // ElementSet::Observations - ResultsObeservationpoints
        [TestCase(ElementSet.Observations, QuantityType.Discharge  )]
        [TestCase(ElementSet.Observations, QuantityType.Salinity   )]
        [TestCase(ElementSet.Observations, QuantityType.Velocity   )]
        [TestCase(ElementSet.Observations, QuantityType.WaterDepth )]
        [TestCase(ElementSet.Observations, QuantityType.WaterLevel )]

        // ElementSet::Retentions - ResultsRetentions
        [TestCase(ElementSet.Retentions, QuantityType.Volume     )]
        [TestCase(ElementSet.Retentions, QuantityType.WaterLevel )]

        // ElementSet::Laterals - ResultLaterals
        [TestCase(ElementSet.Laterals, QuantityType.ActualDischarge   )]
        [TestCase(ElementSet.Laterals, QuantityType.DefinedDischarge  )]
        [TestCase(ElementSet.Laterals, QuantityType.LateralDifference )] 
        [TestCase(ElementSet.Laterals, QuantityType.WaterLevel        )]

        // ElementSet::ModelWide - ResultsWaterBalance
        [TestCase(ElementSet.ModelWide, QuantityType.Bal2d1dIn        )]
        [TestCase(ElementSet.ModelWide, QuantityType.Bal2d1dOut       )]
        [TestCase(ElementSet.ModelWide, QuantityType.Bal2d1dTot       )]
        [TestCase(ElementSet.ModelWide, QuantityType.BalBoundariesIn  )]
        [TestCase(ElementSet.ModelWide, QuantityType.BalBoundariesOut )] 
        [TestCase(ElementSet.ModelWide, QuantityType.BalBoundariesTot )]
        [TestCase(ElementSet.ModelWide, QuantityType.BalError         )]
        [TestCase(ElementSet.ModelWide, QuantityType.BalLatIn         )]
        [TestCase(ElementSet.ModelWide, QuantityType.BalLatOut        )]
        [TestCase(ElementSet.ModelWide, QuantityType.BalLatTot        )]
        [TestCase(ElementSet.ModelWide, QuantityType.BalStorage       )]
        [TestCase(ElementSet.ModelWide, QuantityType.BalVolume        )]
        public void GivenSomeSimpleModelWithWaterFlow1DOutputSettingDataAllSetToNoneAndADataAccessModelDescribingASingleEngineParameterWithSomeAggregateOptionNotNone_WhenWaterFlowModelPropertySetterSetOutputPropertiesIsCalledWithTheseParameters_ThenThisEnginePropertyIsSetToTheSpecifiedAggregateOptionAndAllOtherEngineParametersAreNone(ElementSet elementSet, QuantityType qType)
        {
            // Given
            var validCategoriesSet = new List<DelftIniCategory>() {new DelftIniCategory(GeneralRegion.IniHeader)};
            
            var resultHeader = new DelftIniCategory(mappingElementSetToRegionHeader[elementSet]);
            resultHeader.AddProperty(qType.ToString(), (int)AggregationOptions.Average, "Tenderloin");
            validCategoriesSet.Add(resultHeader);

            var model = new WaterFlowModel1D();

            foreach (var eParam in model.OutputSettings.EngineParameters)
                eParam.AggregationOptions = AggregationOptions.None;

            // When
            WaterFlowModelPropertySetter.SetOutputProperties(validCategoriesSet, model.OutputSettings);

            // Then
            foreach (var eParam in model.OutputSettings.EngineParameters)
            {
                var expectedAggregationOption = eParam.QuantityType == qType && eParam.ElementSet == elementSet
                    ? AggregationOptions.Average
                    : AggregationOptions.None;
                Assert.That(eParam.AggregationOptions, Is.EqualTo(expectedAggregationOption));
            }
        }

        private static readonly IDictionary<ElementSet, string> mappingElementSetToRegionHeader =
            new Dictionary<ElementSet, string>()
            {
                [ElementSet.GridpointsOnBranches] = ModelDefinitionsRegion.ResultsNodesHeader,
                [ElementSet.ReachSegElmSet] = ModelDefinitionsRegion.ResultsBranchesHeader,
                [ElementSet.Structures] = ModelDefinitionsRegion.ResultsStructuresHeader,
                [ElementSet.Pumps] = ModelDefinitionsRegion.ResultsPumpsHeader,
                [ElementSet.Observations] = ModelDefinitionsRegion.ResultsObservationsPointsHeader,
                [ElementSet.Retentions] = ModelDefinitionsRegion.ResultsRetentionsHeader,
                [ElementSet.Laterals] = ModelDefinitionsRegion.ResultsLateralsHeader,
                [ElementSet.ModelWide] = ModelDefinitionsRegion.ResultsWaterBalanceHeader,
            };

        /// <summary>
        /// GIVEN some simple model with WaterFlow1DOutputSettingData all set to None
        ///   AND a dataAccessModel describing multiple EngineParameter with some Aggregate options not None
        /// WHEN WaterFlowModelPropertySetter SetOutputProperties is called with these parameters
        /// THEN These engine property is set to the specified aggregate option
        ///  AND all other engine parameters are None
        /// </summary>
        [Test]
        public void GivenSomeSimpleModelWithWaterFlow1DOutputSettingDataAllSetToNoneAndADataAccessModelDescribingMultipleEngineParameterWithSomeAggregateOptionsNotNone_WhenWaterFlowModelPropertySetterSetOutputPropertiesIsCalledWithTheseParameters_ThenTheseEnginePropertyIsSetToTheSpecifiedAggregateOptionAndAllOtherEngineParametersAreNone()
        {
            // Given 
            // This DelftIniCategory set is equal to the provided RMM model: Epic: Import of SOBEK3 models

        }

    }
}