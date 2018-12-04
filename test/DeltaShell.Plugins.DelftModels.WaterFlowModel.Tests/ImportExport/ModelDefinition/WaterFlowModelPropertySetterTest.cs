using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;
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
        public void GivenTimeSettingsDataModel_WhenSettingWaterFlowModelTimeProperties_ThenTimeSettingsAreCorrect()
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

        [Test]
        public void GivenTimeSettingsDataModelWithMissingStartTime_WhenSettingWaterFlowModelTimeProperties_ThenDefaultStartTimeIsSetOnModel()
        {
            // Given / When
            var model = SetTimePropertiesWithMissingProperty(ModelDefinitionsRegion.StartTime.Key);

            // Then
            var defaultDateTime = default(DateTime);
            Assert.That(model.StartTime, Is.EqualTo(defaultDateTime));
        }

        [Test]
        public void GivenTimeSettingsDataModelWithMissingStopTime_WhenSettingWaterFlowModelTimeProperties_ThenDefaultStopTimeIsSetOnModel()
        {
            // Given / When
            var model = SetTimePropertiesWithMissingProperty(ModelDefinitionsRegion.StopTime.Key);

            // Then
            var defaultDateTime = default(DateTime);
            Assert.That(model.StopTime, Is.EqualTo(defaultDateTime));
        }

        [Test]
        public void GivenTimeSettingsDataModelWithMissingTimeStep_WhenSettingWaterFlowModelTimeProperties_ThenDefaultTimeStepIsSetOnModel()
        {
            // Given / When
            var model = SetTimePropertiesWithMissingProperty(ModelDefinitionsRegion.TimeStep.Key);

            // Then
            var defaultTimeSpan = default(TimeSpan);
            Assert.That(model.TimeStep, Is.EqualTo(defaultTimeSpan));
        }

        [Test]
        public void GivenTimeSettingsDataModelWithMissingGridOutputTimeStep_WhenSettingWaterFlowModelTimeProperties_ThenDefaultGridOutputTimeStepIsSetOnModel()
        {
            // Given / When
            var model = SetTimePropertiesWithMissingProperty(ModelDefinitionsRegion.OutTimeStepGridPoints.Key);

            // Then
            var defaultTimeSpan = default(TimeSpan);
            Assert.That(model.OutputSettings.GridOutputTimeStep, Is.EqualTo(defaultTimeSpan));
        }

        [Test]
        public void GivenTimeSettingsDataModelWithMissingStructureOutputTimeStep_WhenSettingWaterFlowModelTimeProperties_ThenDefaultStructureOutputTimeStepIsSetOnModel()
        {
            // Given / When
            var model = SetTimePropertiesWithMissingProperty(ModelDefinitionsRegion.OutTimeStepStructures.Key);

            // Then
            var defaultTimeSpan = default(TimeSpan);
            Assert.That(model.OutputSettings.StructureOutputTimeStep, Is.EqualTo(defaultTimeSpan));
        }

        private WaterFlowModel1D SetTimePropertiesWithMissingProperty(string missingPropertyNAme)
        {
            // Given
            var timeSettingsCategories = GetCorrectTimeSettingsDataModel().ToArray();
            timeSettingsCategories.ForEach(c => c.Properties.RemoveAllWhere(p => p.Name == missingPropertyNAme));

            // When
            var model = new WaterFlowModel1D();
            WaterFlowModelPropertySetter.SetTimeProperties(timeSettingsCategories, model);
            return model;
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
        public void GivenDataModelWithoutTimeCategory_WhenSettingWaterFlowModelTimeProperties_ThenTimeSettingsHaveNotChanged()
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
        public void WhenSettingWaterFlowModelTimePropertiesWithCategoriesEqualToNull_ThenTimeSettingsHaveNotChanged()
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
        public void WhenSettingWaterFlowModelTimePropertiesWithoutAModel_ThenNoException()
        {
            TestDelegate testDelegate = () => { WaterFlowModelPropertySetter.SetTimeProperties(null, null); };
            Assert.DoesNotThrow(testDelegate);
        }

        // SetOutputProperties
        /// <summary>
        /// GIVEN a WaterFlow1DOutputSettingData with all EngineParameters set to None
        ///   AND a dataAccessModel describing a single EngineParameter with some Aggregate option not None
        /// WHEN WaterFlowModelPropertySetter SetOutputProperties is called with these parameters
        /// THEN This engine property is set to the specified aggregate option
        ///  AND all other engine parameters are None
        /// </summary>
        // Commented test cases are currently not supported in the GUI, here for visibility
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

        // Output for D-Water Quality / DELWAQ
        [TestCase(ElementSet.FiniteVolumeGridOnGridPoints, QuantityType.FiniteGridType)]
        public void GivenAWaterFlow1DOutputSettingDataWithAllEngineParametersSetToNoneAndADataAccessModelDescribingASingleEngineParameterWithSomeAggregateOptionNotNone_WhenWaterFlowModelPropertySetterSetOutputPropertiesIsCalledWithTheseParameters_ThenThisEnginePropertyIsSetToTheSpecifiedAggregateOptionAndAllOtherEngineParametersAreNone(ElementSet elementSet, QuantityType qType)
        {
            // Given
            var validCategoriesSet = new List<DelftIniCategory>() {new DelftIniCategory(GeneralRegion.IniHeader)};
            
            var resultHeader = new DelftIniCategory(MappingElementSetToRegionHeader[elementSet]);
            resultHeader.AddProperty(qType.ToString(), (int)AggregationOptions.Average, "Tenderloin");
            validCategoriesSet.Add(resultHeader);

            var outputSettings = new WaterFlowModel1DOutputSettingData();

            foreach (var eParam in outputSettings.EngineParameters)
                eParam.AggregationOptions = AggregationOptions.None;

            // When
            WaterFlowModelPropertySetter.SetOutputProperties(validCategoriesSet, outputSettings);

            // Then
            Assert.That(outputSettings.GetEngineParameter(qType, elementSet).AggregationOptions,
                Is.EqualTo(AggregationOptions.Average));

            foreach (var eParam in outputSettings.EngineParameters)
            {
                var expectedAggregationOption = eParam.QuantityType == qType && eParam.ElementSet == elementSet
                    ? AggregationOptions.Average
                    : AggregationOptions.None;
                Assert.That(eParam.AggregationOptions, Is.EqualTo(expectedAggregationOption));
            }
        }

        private static readonly IDictionary<ElementSet, string> MappingElementSetToRegionHeader =
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
                [ElementSet.FiniteVolumeGridOnGridPoints] = ElementSet.FiniteVolumeGridOnGridPoints.ToString(), 
            };

        /// <summary>
        /// GIVEN a WaterFlow1DOutputSettingData with all EngineParameters set to None
        ///   AND a dataAccessModel describing a Dispersion EngineParameter with some Aggregate option not None
        /// WHEN WaterFlowModelPropertySetter SetOutputProperties is called with these parameters
        /// THEN This engine property is set to the specified aggregate option
        ///  AND all other engine parameters are None
        /// </summary>
        [Test]
        public void GivenAWaterFlow1DOutputSettingDataWithAllEngineParametersSetToNoneAndADataAccessModelDescribingADispersionEngineParameterWithSomeAggregateOptionNotNone_WhenWaterFlowModelPropertySetterSetOutputPropertiesIsCalledWithTheseParameters_ThenThisEnginePropertyIsSetToTheSpecifiedAggregateOptionAndAllOtherEngineParametersAreNone()
        {
            // For some reason we keep the QuantityType.Dispersion (Salinity
            // Dispersion) in the ElementSet GridPointsOnBranches (ResultsNodes),
            // however the Kernel expects this to be in ReachSegElmSet
            // (ResultsBranches) hence the need for this test.

            // Given
            var validCategoriesSet = new List<DelftIniCategory>() { new DelftIniCategory(GeneralRegion.IniHeader) };

            var resultHeader = new DelftIniCategory(MappingElementSetToRegionHeader[ElementSet.ReachSegElmSet]);
            resultHeader.AddProperty(QuantityType.Dispersion.ToString(), (int) AggregationOptions.Maximum, "Tenderloin");
            validCategoriesSet.Add(resultHeader);

            var outputSettings = new WaterFlowModel1DOutputSettingData();

            foreach (var eParam in outputSettings.EngineParameters)
                eParam.AggregationOptions = AggregationOptions.None;

            // When
            WaterFlowModelPropertySetter.SetOutputProperties(validCategoriesSet, outputSettings);

            // Then
            Assert.That(outputSettings.GetEngineParameter(QuantityType.Dispersion, ElementSet.GridpointsOnBranches).AggregationOptions,
                Is.EqualTo(AggregationOptions.Maximum));

            foreach (var eParam in outputSettings.EngineParameters)
            {
                var expectedAggregationOption = eParam.QuantityType == QuantityType.Dispersion && eParam.ElementSet == ElementSet.GridpointsOnBranches
                    ? AggregationOptions.Maximum
                    : AggregationOptions.None;
                Assert.That(eParam.AggregationOptions, Is.EqualTo(expectedAggregationOption));
            }
        }

        /// <summary>
        /// GIVEN a WaterFlow1DOutputSettingData with all EngineParameters set to None
        ///   AND a dataAccessModel describing a Lateral1D2D EngineParameter with some Aggregate option not None
        /// WHEN WaterFlowModelPropertySetter SetOutputProperties is called with these parameters
        /// THEN This engine property is set to the specified aggregate option
        ///  AND all other engine parameters are None
        /// </summary>
        [Test]
        public void GivenAWaterFlow1DOutputSettingDataWithAllEngineParametersSetToNoneAndADataAccessModelDescribingALateral1D2DEngineParameterWithSomeAggregateOptionNotNone_WhenWaterFlowModelPropertySetterSetOutputPropertiesIsCalledWithTheseParameters_ThenThisEnginePropertyIsSetToTheSpecifiedAggregateOptionAndAllOtherEngineParametersAreNone()
        {
            // For some reason we keep the property Lateral1D2D as QuantityType.QTotal_1d2d 
            // In order to test this corner case we need to separately test this.
            var validCategoriesSet = new List<DelftIniCategory>() { new DelftIniCategory(GeneralRegion.IniHeader) };

            var resultHeader = new DelftIniCategory(MappingElementSetToRegionHeader[ElementSet.GridpointsOnBranches]);
            resultHeader.AddProperty("Lateral1D2D", (int)AggregationOptions.Maximum, "Tenderloin");
            validCategoriesSet.Add(resultHeader);

            var outputSettings = new WaterFlowModel1DOutputSettingData();

            foreach (var eParam in outputSettings.EngineParameters)
                eParam.AggregationOptions = AggregationOptions.None;

            // When
            WaterFlowModelPropertySetter.SetOutputProperties(validCategoriesSet, outputSettings);

            // Then
            Assert.That(outputSettings.GetEngineParameter(QuantityType.QTotal_1d2d, ElementSet.GridpointsOnBranches).AggregationOptions,
                Is.EqualTo(AggregationOptions.Maximum));

            foreach (var eParam in outputSettings.EngineParameters)
            {
                var expectedAggregationOption = eParam.QuantityType == QuantityType.QTotal_1d2d && eParam.ElementSet == ElementSet.GridpointsOnBranches
                    ? AggregationOptions.Maximum
                    : AggregationOptions.None;
                Assert.That(eParam.AggregationOptions, Is.EqualTo(expectedAggregationOption));
            }
        }

        /// <summary>
        /// GIVEN a WaterFlow1DOutputSettingData with all EngineParameters set to None
        ///   AND a dataAccessModel describing an EnergyLevel EngineParameter with some Aggregate option
        /// WHEN WaterFlowModelPropertySetter SetOutputProperties is called with these parameters
        /// THEN all engine parameters are None
        ///  AND No exception is thrown
        /// </summary>
        /// <remarks>
        /// Currently, we do not put the QuantityType.EnergyLevels within the
        /// OutputSettingData. However we do have a QuantityType.EnergyLevels
        /// within the API, as such, this case needs to be explicitly handled. 
        /// If this ends up being added, this case could just be added to the
        /// main test, with the following line:
        /// [TestCase(ElementSet.ReachSegElmSet, QuantityType.EnergyLevels         )]
        /// </remarks>
        [Test]
        public void GivenAWaterFlow1DOutputSettingDataWithAllEngineParametersSetToNoneAndADataAccessModelDescribingAnEnergyLevelEngineParameterWithSomeAggregateOption_WhenWaterFlowModelPropertySetterSetOutputPropertiesIsCalledWithTheseParameters_ThenAllEngineParametersAreNoneAndNoExceptionIsThrown()
        {
            // For some reason we keep the property Lateral1D2D as QuantityType.QTotal_1d2d 
            // In order to test this corner case we need to separately test this.
            var validCategoriesSet = new List<DelftIniCategory>() { new DelftIniCategory(GeneralRegion.IniHeader) };

            var resultHeader = new DelftIniCategory(MappingElementSetToRegionHeader[ElementSet.ReachSegElmSet]);
            resultHeader.AddProperty(QuantityType.EnergyLevels.ToString(), (int)AggregationOptions.Maximum, "Tenderloin");
            validCategoriesSet.Add(resultHeader);

            var outputSettings = new WaterFlowModel1DOutputSettingData();

            foreach (var eParam in outputSettings.EngineParameters)
                eParam.AggregationOptions = AggregationOptions.None;

            // When
            WaterFlowModelPropertySetter.SetOutputProperties(validCategoriesSet, outputSettings);

            foreach (var eParam in outputSettings.EngineParameters)
            {
                Assert.That(eParam.AggregationOptions, Is.EqualTo(AggregationOptions.None));
            }
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
            var resultsNodesProperties = new List<Tuple<QuantityType, AggregationOptions>>()
            {
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Density            , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.EffectiveBackRad   , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.HeatLossConv       , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.HeatLossEvap       , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.HeatLossForcedConv , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.HeatLossForcedEvap , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.HeatLossFreeConv   , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.HeatLossFreeEvap   , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.LateralAtNodes     , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.NegativeDepth      , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.NetSolarRad        , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.NoIteration        , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.RadFluxClearSky    , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Salinity           , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Temperature        , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.TotalArea          , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.TotalHeatFlux      , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.TotalWidth         , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Volume             , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WaterDepth         , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WaterLevel         , AggregationOptions.Current),
            };

            var resultsBranchesProperties = new List<Tuple<QuantityType, AggregationOptions>>()
            {
                new Tuple<QuantityType, AggregationOptions>(QuantityType.AreaFP1            , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.AreaFP2            , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.AreaMain           , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.ChezyFP1           , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.ChezyFP2           , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.ChezyMain          , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Discharge          , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.DischargeFP1       , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.DischargeFP2       , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.DischargeMain      , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.FlowArea           , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.FlowChezy          , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.FlowConv           , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.FlowHydrad         , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Froude             , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.HydradFP1          , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.HydradFP2          , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.HydradMain         , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.TimeStepEstimation , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Velocity           , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WaterLevelGradient , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WidthFP1           , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WidthFP2           , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WidthMain          , AggregationOptions.None),
            };

            var resultsStructuresProperties = new List<Tuple<QuantityType, AggregationOptions>>()
            {
                new Tuple<QuantityType, AggregationOptions>(QuantityType.CrestLevel         , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.CrestWidth         , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Discharge          , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.FlowArea           , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.GateLowerEdgeLevel , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.GateOpeningHeight  , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Head               , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.PressureDifference , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.ValveOpening       , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Velocity           , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WaterLevelAtCrest  , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WaterlevelDown     , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WaterlevelUp       , AggregationOptions.Current),
            };

            var resultsObservationProperties = new List<Tuple<QuantityType, AggregationOptions>>()
            {
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Discharge   , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Dispersion  , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Salinity    , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Temperature , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Velocity    , AggregationOptions.Current),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Volume      , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WaterDepth  , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WaterLevel  , AggregationOptions.Current),
            };

            var resultsRetentionsProperties = new List<Tuple<QuantityType, AggregationOptions>>()
            {
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Volume     , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WaterLevel , AggregationOptions.None),
            };

            var resultsLateralsProperties = new List<Tuple<QuantityType, AggregationOptions>>()
            {
                new Tuple<QuantityType, AggregationOptions>(QuantityType.ActualDischarge   , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.DefinedDischarge  , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.LateralDifference , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.WaterLevel        , AggregationOptions.None),
            };

            var resultsWaterBalanceProperties = new List<Tuple<QuantityType, AggregationOptions>>()
            {
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Bal2d1dIn        , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Bal2d1dOut       , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.Bal2d1dTot       , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.BalBoundariesIn  , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.BalBoundariesOut , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.BalBoundariesTot , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.BalError         , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.BalLatIn         , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.BalLatOut        , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.BalLatTot        , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.BalStorage       , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.BalVolume        , AggregationOptions.None),
            };

            var resultsPumps = new List<Tuple<QuantityType, AggregationOptions>>()
            {
                new Tuple<QuantityType, AggregationOptions>(QuantityType.ActualPumpStage   , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.DeliverySideLevel , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.PumpCapacity      , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.PumpDischarge     , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.PumpHead          , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.ReductionFactor   , AggregationOptions.None),
                new Tuple<QuantityType, AggregationOptions>(QuantityType.SuctionSideLevel  , AggregationOptions.None),
            };



            var validCategoriesSet = new List<DelftIniCategory>() { new DelftIniCategory(GeneralRegion.IniHeader) };
            validCategoriesSet.Add(getResultsCategory(ElementSet.GridpointsOnBranches, resultsNodesProperties));
            validCategoriesSet[0].AddProperty("Lateral1D2D", (int)AggregationOptions.Maximum, "Tenderloin");

            validCategoriesSet.Add(getResultsCategory(ElementSet.ReachSegElmSet, resultsBranchesProperties));
            validCategoriesSet[1].AddProperty(QuantityType.Dispersion.ToString(), (int)AggregationOptions.None, "Dispersion is a bit weird");

            validCategoriesSet.Add(getResultsCategory(ElementSet.Structures, resultsStructuresProperties));
            validCategoriesSet.Add(getResultsCategory(ElementSet.Observations, resultsObservationProperties));
            validCategoriesSet.Add(getResultsCategory(ElementSet.Retentions, resultsRetentionsProperties));
            validCategoriesSet.Add(getResultsCategory(ElementSet.Laterals, resultsLateralsProperties));
            validCategoriesSet.Add(getResultsCategory(ElementSet.ModelWide, resultsWaterBalanceProperties));
            validCategoriesSet.Add(getResultsCategory(ElementSet.Pumps, resultsPumps));

            var outputSettings = new WaterFlowModel1DOutputSettingData();

            foreach (var eParam in outputSettings.EngineParameters)
                eParam.AggregationOptions = AggregationOptions.None;

            // When
            WaterFlowModelPropertySetter.SetOutputProperties(validCategoriesSet, outputSettings);

            // Then
            foreach (var t in resultsNodesProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Item1, ElementSet.GridpointsOnBranches).AggregationOptions,
                    Is.EqualTo(t.Item2));
            // Check corner case: Lateral1D2D
            Assert.That(outputSettings.GetEngineParameter(QuantityType.QTotal_1d2d, ElementSet.GridpointsOnBranches).AggregationOptions,
                Is.EqualTo(AggregationOptions.None));

            foreach (var t in resultsBranchesProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Item1, ElementSet.ReachSegElmSet).AggregationOptions,
                    Is.EqualTo(t.Item2));
            // Check corner case: Dispersion
            Assert.That(outputSettings.GetEngineParameter(QuantityType.Dispersion, ElementSet.GridpointsOnBranches).AggregationOptions,
                Is.EqualTo(AggregationOptions.None));


            foreach (var t in resultsStructuresProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Item1, ElementSet.Structures).AggregationOptions,
                    Is.EqualTo(t.Item2));

            foreach (var t in resultsObservationProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Item1, ElementSet.Observations).AggregationOptions,
                    Is.EqualTo(t.Item2));

            foreach (var t in resultsRetentionsProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Item1, ElementSet.Retentions).AggregationOptions,
                    Is.EqualTo(t.Item2));

            foreach (var t in resultsLateralsProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Item1, ElementSet.Laterals).AggregationOptions,
                    Is.EqualTo(t.Item2));

            foreach (var t in resultsWaterBalanceProperties)
                Assert.That(outputSettings.GetEngineParameter(t.Item1, ElementSet.ModelWide).AggregationOptions,
                    Is.EqualTo(t.Item2));

            foreach (var t in resultsPumps)
                Assert.That(outputSettings.GetEngineParameter(t.Item1, ElementSet.Pumps).AggregationOptions,
                    Is.EqualTo(t.Item2));
        }

        private DelftIniCategory getResultsCategory(ElementSet resultsHeader,
                                                    IEnumerable<Tuple<QuantityType, AggregationOptions>> properties)
        {
            var category = new DelftIniCategory(MappingElementSetToRegionHeader[resultsHeader]);

            foreach (var prop in properties)
                category.AddProperty(prop.Item1.ToString(), (int) prop.Item2, "Some really interesting comment.");

            return category;
        }
    }
}