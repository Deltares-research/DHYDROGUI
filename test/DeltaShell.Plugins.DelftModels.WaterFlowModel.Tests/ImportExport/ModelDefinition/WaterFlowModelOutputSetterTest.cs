using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelOutputSetterTest
    {
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

        /// <summary>
        /// GIVEN a WaterFlow1DOutputSettingData with all EngineParameters set to None
        ///   AND a dataAccessModel describing a single EngineParameter with some Aggregate option not None
        /// WHEN ModelDefinitionFileReader SetOutputProperties is called with these parameters
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
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.AreaFP1)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.AreaFP2)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.AreaMain)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.ChezyFP1)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.ChezyFP2)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.ChezyMain)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.Discharge)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.DischargeFP1)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.DischargeFP2)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.DischargeMain)]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.F1                   )]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.F3                   )]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.F4                   )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.FlowArea)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.FlowChezy)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.FlowConv)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.FlowHydrad)]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.FlowWidth            )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.Froude)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.HydradFP1)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.HydradFP2)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.HydradMain)]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomAcceleration      )]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomAdvection         )]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomBedStress         )]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomLateralCorrection )]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomLosses            )]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomPressure          )]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.MomWindStress        )]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.TimeStepEstimation)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.Velocity)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.WaterLevelGradient)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.WidthFP1)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.WidthFP2)]
        [TestCase(ElementSet.ReachSegElmSet, QuantityType.WidthMain)]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.WindVelocity         )]
        //[TestCase(ElementSet.ReachSegElmSet, QuantityType.WindDirection        )]

        // ElementSet::Structures - Structures - ResultsStructures
        [TestCase(ElementSet.Structures, QuantityType.CrestLevel)]
        [TestCase(ElementSet.Structures, QuantityType.CrestWidth)]
        [TestCase(ElementSet.Structures, QuantityType.Discharge)]
        [TestCase(ElementSet.Structures, QuantityType.FlowArea)]
        [TestCase(ElementSet.Structures, QuantityType.GateLowerEdgeLevel)]
        [TestCase(ElementSet.Structures, QuantityType.GateOpeningHeight)]
        [TestCase(ElementSet.Structures, QuantityType.Head)]
        [TestCase(ElementSet.Structures, QuantityType.PressureDifference)]
        //[TestCase(ElementSet.Structures, QuantityType.State               )]
        [TestCase(ElementSet.Structures, QuantityType.Velocity)]
        [TestCase(ElementSet.Structures, QuantityType.WaterLevelAtCrest)]
        //[TestCase(ElementSet.Structures, QuantityType.WaterLevelDown      )]
        //[TestCase(ElementSet.Structures, QuantityType.WaterLevelUp        )]

        // ElementSet::Pumps - Pumps - ResultsPumps
        [TestCase(ElementSet.Pumps, QuantityType.ActualPumpStage)]
        [TestCase(ElementSet.Pumps, QuantityType.DeliverySideLevel)]
        [TestCase(ElementSet.Pumps, QuantityType.PumpCapacity)]
        [TestCase(ElementSet.Pumps, QuantityType.PumpDischarge)]
        [TestCase(ElementSet.Pumps, QuantityType.PumpHead)]
        [TestCase(ElementSet.Pumps, QuantityType.ReductionFactor)]
        [TestCase(ElementSet.Pumps, QuantityType.SuctionSideLevel)]

        // ElementSet::Observations - ResultsObeservationpoints
        [TestCase(ElementSet.Observations, QuantityType.Discharge)]
        [TestCase(ElementSet.Observations, QuantityType.Salinity)]
        [TestCase(ElementSet.Observations, QuantityType.Velocity)]
        [TestCase(ElementSet.Observations, QuantityType.WaterDepth)]
        [TestCase(ElementSet.Observations, QuantityType.WaterLevel)]

        // ElementSet::Retentions - ResultsRetentions
        [TestCase(ElementSet.Retentions, QuantityType.Volume)]
        [TestCase(ElementSet.Retentions, QuantityType.WaterLevel)]

        // ElementSet::Laterals - ResultLaterals
        [TestCase(ElementSet.Laterals, QuantityType.ActualDischarge)]
        [TestCase(ElementSet.Laterals, QuantityType.DefinedDischarge)]
        [TestCase(ElementSet.Laterals, QuantityType.LateralDifference)]
        [TestCase(ElementSet.Laterals, QuantityType.WaterLevel)]

        // ElementSet::ModelWide - ResultsWaterBalance
        [TestCase(ElementSet.ModelWide, QuantityType.Bal2d1dIn)]
        [TestCase(ElementSet.ModelWide, QuantityType.Bal2d1dOut)]
        [TestCase(ElementSet.ModelWide, QuantityType.Bal2d1dTot)]
        [TestCase(ElementSet.ModelWide, QuantityType.BalBoundariesIn)]
        [TestCase(ElementSet.ModelWide, QuantityType.BalBoundariesOut)]
        [TestCase(ElementSet.ModelWide, QuantityType.BalBoundariesTot)]
        [TestCase(ElementSet.ModelWide, QuantityType.BalError)]
        [TestCase(ElementSet.ModelWide, QuantityType.BalLatIn)]
        [TestCase(ElementSet.ModelWide, QuantityType.BalLatOut)]
        [TestCase(ElementSet.ModelWide, QuantityType.BalLatTot)]
        [TestCase(ElementSet.ModelWide, QuantityType.BalStorage)]
        [TestCase(ElementSet.ModelWide, QuantityType.BalVolume)]

        // Output for D-Water Quality / DELWAQ
        [TestCase(ElementSet.FiniteVolumeGridOnGridPoints, QuantityType.FiniteGridType)]
        public void GivenAWaterFlow1DOutputSettingDataWithAllEngineParametersSetToNoneAndADataAccessModelDescribingASingleEngineParameterWithSomeAggregateOptionNotNone_WhenWaterFlowModelPropertySetterSetOutputPropertiesIsCalledWithTheseParameters_ThenThisEnginePropertyIsSetToTheSpecifiedAggregateOptionAndAllOtherEngineParametersAreNone(ElementSet elementSet, QuantityType qType)
        {
            // Given
            var resultHeader = new DelftIniCategory(MappingElementSetToRegionHeader[elementSet]);
            resultHeader.AddProperty(qType.ToString(), (int)AggregationOptions.Average, "Tenderloin");

            var model = new WaterFlowModel1D();
            var outputSettings = model.OutputSettings;
            foreach (var eParam in outputSettings.EngineParameters)
                eParam.AggregationOptions = AggregationOptions.None;
            
            // When
            new WaterFlowModelOutputSetter().SetProperties(resultHeader, model, new List<string>());

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
        
        /// <summary>
        /// GIVEN a WaterFlow1DOutputSettingData with all EngineParameters set to None
        ///   AND a dataAccessModel describing a Dispersion EngineParameter with some Aggregate option not None
        /// WHEN ModelDefinitionFileReader SetOutputProperties is called with these parameters
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
            var resultHeader = new DelftIniCategory(MappingElementSetToRegionHeader[ElementSet.ReachSegElmSet]);
            resultHeader.AddProperty(QuantityType.Dispersion.ToString(), (int)AggregationOptions.Maximum, "Tenderloin");

            var model = new WaterFlowModel1D();
            var outputSettings = model.OutputSettings;
            foreach (var eParam in outputSettings.EngineParameters)
                eParam.AggregationOptions = AggregationOptions.None;

            // When
            new WaterFlowModelOutputSetter().SetProperties(resultHeader, model, new List<string>());

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
        /// WHEN ModelDefinitionFileReader SetOutputProperties is called with these parameters
        /// THEN This engine property is set to the specified aggregate option
        ///  AND all other engine parameters are None
        /// </summary>
        [Test]
        public void GivenAWaterFlow1DOutputSettingDataWithAllEngineParametersSetToNoneAndADataAccessModelDescribingALateral1D2DEngineParameterWithSomeAggregateOptionNotNone_WhenWaterFlowModelPropertySetterSetOutputPropertiesIsCalledWithTheseParameters_ThenThisEnginePropertyIsSetToTheSpecifiedAggregateOptionAndAllOtherEngineParametersAreNone()
        {
            // For some reason we keep the property Lateral1D2D as QuantityType.QTotal_1d2d 
            // In order to test this corner case we need to separately test this.

            // Given
            var resultHeader = new DelftIniCategory(MappingElementSetToRegionHeader[ElementSet.GridpointsOnBranches]);
            resultHeader.AddProperty("Lateral1D2D", (int)AggregationOptions.Maximum, "Tenderloin");

            var model = new WaterFlowModel1D();
            var outputSettings = model.OutputSettings;
            foreach (var eParam in outputSettings.EngineParameters)
                eParam.AggregationOptions = AggregationOptions.None;

            // When
            new WaterFlowModelOutputSetter().SetProperties(resultHeader, model, new List<string>());

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
        /// WHEN ModelDefinitionFileReader SetOutputProperties is called with these parameters
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

            // Given
            var resultHeader = new DelftIniCategory(MappingElementSetToRegionHeader[ElementSet.ReachSegElmSet]);
            resultHeader.AddProperty(QuantityType.EnergyLevels.ToString(), (int)AggregationOptions.Maximum, "Tenderloin");

            var model = new WaterFlowModel1D();
            var outputSettings = model.OutputSettings;
            foreach (var eParam in outputSettings.EngineParameters)
                eParam.AggregationOptions = AggregationOptions.None;

            // When
            new WaterFlowModelOutputSetter().SetProperties(resultHeader, model, new List<string>());

            // Then
            foreach (var eParam in outputSettings.EngineParameters)
            {
                Assert.That(eParam.AggregationOptions, Is.EqualTo(AggregationOptions.None));
            }
        }

        [TestCase(ElementSet.GridpointsOnBranches)]
        [TestCase(ElementSet.ReachSegElmSet)]
        [TestCase(ElementSet.Structures)]
        [TestCase(ElementSet.Pumps)]
        [TestCase(ElementSet.Observations)]
        [TestCase(ElementSet.Retentions)]
        [TestCase(ElementSet.Laterals)]
        [TestCase(ElementSet.ModelWide)]
        [TestCase(ElementSet.FiniteVolumeGridOnGridPoints)]
        public void GivenModelOutputDataModelWithUnknownProperty_WhenSettingModelProperties_ThenUnknownPropertyIsSkippedAndErrorMessageIsReturned
            (ElementSet elementSet)
        {
            // Given
            const string unknownPropertyName = "UnknownProperty";
            var category = new DelftIniCategory(MappingElementSetToRegionHeader[elementSet]);
            category.AddProperty(unknownPropertyName, 1);

            // When
            var errorMessages = new List<string>();
            var model = new WaterFlowModel1D();
            new WaterFlowModelOutputSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.That(errorMessages.Count, Is.EqualTo(1));
            var expectedMessage = string.Format(Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                0, unknownPropertyName);
            Assert.Contains(expectedMessage, errorMessages);
        }

        [Test]
        [TestCase(ModelDefinitionsRegion.ResultsNodesHeader, "totalwidth")]
        [TestCase(ModelDefinitionsRegion.ResultsBranchesHeader, "areamain")]
        [TestCase(ModelDefinitionsRegion.ResultsStructuresHeader, "crestlevel")]
        [TestCase(ModelDefinitionsRegion.ResultsPumpsHeader, "pumpcapacity")]
        [TestCase(ModelDefinitionsRegion.ResultsObservationsPointsHeader, "dispersion")]
        [TestCase(ModelDefinitionsRegion.ResultsLateralsHeader, "lateraldifference")]
        [TestCase(ModelDefinitionsRegion.ResultsRetentionsHeader, "volume")]
        [TestCase(ModelDefinitionsRegion.ResultsWaterBalanceHeader, "bal2d1din")]
        public void GivenACategoryWithCorrectPropertiesInLowerCase_WhenSettingProperties_NoExceptionsOrErrorMessagesAreThrown(string categoryName, string propertyName)
        {
            var category = new DelftIniCategory(categoryName);
            category.AddProperty(propertyName, 1);

            // When - Then
            var errorMessages = new List<string>();
            var model = new WaterFlowModel1D();
            Assert.DoesNotThrow(() => new WaterFlowModelOutputSetter().SetProperties(category, model, errorMessages));
            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }
    }
}