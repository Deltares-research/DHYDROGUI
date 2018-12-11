using System;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelPropertySetterFactoryTest
    {
        [TestCase(ModelDefinitionsRegion.TimeHeader, typeof(WaterFlowModelTimePropertiesSetter))]
        [TestCase(ModelDefinitionsRegion.ResultsNodesHeader, typeof(WaterFlowModelOutputSetter))]
        [TestCase(ModelDefinitionsRegion.ResultsBranchesHeader, typeof(WaterFlowModelOutputSetter))]
        [TestCase(ModelDefinitionsRegion.ResultsStructuresHeader, typeof(WaterFlowModelOutputSetter))]
        [TestCase(ModelDefinitionsRegion.ResultsPumpsHeader, typeof(WaterFlowModelOutputSetter))]
        [TestCase(ModelDefinitionsRegion.ResultsObservationsPointsHeader, typeof(WaterFlowModelOutputSetter))]
        [TestCase(ModelDefinitionsRegion.ResultsRetentionsHeader, typeof(WaterFlowModelOutputSetter))]
        [TestCase(ModelDefinitionsRegion.ResultsLateralsHeader, typeof(WaterFlowModelOutputSetter))]
        [TestCase(ModelDefinitionsRegion.ResultsWaterBalanceHeader, typeof(WaterFlowModelOutputSetter))]
        [TestCase(ModelDefinitionsRegion.FiniteVolumeGridOnGridPoints, typeof(WaterFlowModelOutputSetter))]
        [TestCase(ModelDefinitionsRegion.TransportComputationValuesHeader, typeof(WaterFlowModelTransportComputationPropertiesSetter))]
        [TestCase(ModelDefinitionsRegion.AdvancedOptionsHeader, typeof(WaterFlowModelAdvancedOptionsSetter))]
        [TestCase(ModelDefinitionsRegion.TemperatureValuesHeader, typeof(WaterFlowModelTemperatureSetter))]
        [TestCase(ModelDefinitionsRegion.MorphologyValuesHeader, typeof(WaterFlowModelMorphologySetter))]
        [TestCase(ModelDefinitionsRegion.NumericalParametersValuesHeader, typeof(WaterFlowModelNumericalParametersSetter))]
        [TestCase(ModelDefinitionsRegion.SimulationOptionsValuesHeader, typeof(WaterFlowModelSimulationOptionsSetter))]
        [TestCase(ModelDefinitionsRegion.RestartHeader, typeof(WaterFlowModelRestartSetter))]
        [TestCase(ModelDefinitionsRegion.SalinityValuesHeader, typeof(WaterFlowModelSalinitySetter))]
        public void GivenDataModelWithSpecificHeader_WhenGettingPropertySetterFromFactory_ThenCorrectPropertiesSetterIsReturned(string headerText, Type expectedType)
        {
            // Given
            var category = new DelftIniCategory(headerText);

            // When
            var propertySetter = WaterFlowModelPropertySetterFactory.GetPropertySetter(category);

            // Then
            Assert.That(propertySetter.GetType(), Is.EqualTo(expectedType));
        }

        [Test]
        public void GivenDataModelWithUnkownHeader_WhenGettingPropertySetterFromFactory_ThenNotImplementedExceptionIsThrown()
        {
            // Given
            var unknownCategory = new DelftIniCategory("Unknown Header");

            // When
            TestDelegate getPropertySetter = () => WaterFlowModelPropertySetterFactory.GetPropertySetter(unknownCategory);

            // Then
            Assert.Throws<NotImplementedException>(getPropertySetter);
        }
    }
}