using System.Globalization;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.DelftIniCategoryGenerators;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.DelftIniCategoryGenerators
{
    [TestFixture]
    public class GlobalValuesCategoryGeneratorTest
    {
        [TestCase(InitialConditionsType.WaterLevel, "0")]
        [TestCase(InitialConditionsType.Depth, "1")]
        public void GivenWaterFlowModel1DWithSpecificInitialConditionsType_WhenGeneratingGlobalValuesCategory_ThenCorrectDataModelIsReturned
            (InitialConditionsType initialConditionsType, string expectedUseInitialWaterDepthValue)
        {
            // Given
            var flowModel = new WaterFlowModel1D
            {
                InitialConditionsType = initialConditionsType
            };

            // When
            var category = flowModel.GenerateGlobalValuesCategory();

            // Then
            Assert.That(category.GetPropertyValue(ModelDefinitionsRegion.UseInitialWaterDepth.Key), Is.EqualTo(expectedUseInitialWaterDepthValue));
        }

        [Test]
        public void GivenWaterFlowModel1D_WhenGeneratingGlobalValuesCategory_ThenCorrectDataModelIsReturned()
        {
            // Given
            const double defaultInitialWaterLevel = 1.0;
            const double defaultInitialDepth = 2.0;
            const double initialFlowDefaultValue = 4.0;

            var flowModel = new WaterFlowModel1D
            {
                DefaultInitialWaterLevel = defaultInitialWaterLevel,
                DefaultInitialDepth = defaultInitialDepth
            };
            flowModel.InitialFlow.DefaultValue = initialFlowDefaultValue;

            // When
            var category = flowModel.GenerateGlobalValuesCategory();

            // Then
            Assert.That(GetPropertyValueAsDouble(category, ModelDefinitionsRegion.InitialWaterLevel.Key), Is.EqualTo(defaultInitialWaterLevel));
            Assert.That(GetPropertyValueAsDouble(category, ModelDefinitionsRegion.InitialWaterDepth.Key), Is.EqualTo(defaultInitialDepth));
            Assert.That(GetPropertyValueAsDouble(category, ModelDefinitionsRegion.InitialDischarge.Key), Is.EqualTo(initialFlowDefaultValue));
        }

        [Test]
        public void GivenWaterFlowModel1DThatUsesSalt_WhenGeneratingGlobalValuesCategory_ThenCorrectDataModelIsReturned()
        {
            // Given
            const double saltConcentrationDefaultValue = 4.0;
            const double dispersionCoverageDefaultValue = 5.0;

            var flowModel = new WaterFlowModel1D
            {
                UseSalt = true
            };
            flowModel.InitialSaltConcentration.DefaultValue = saltConcentrationDefaultValue;
            flowModel.DispersionCoverage.DefaultValue = dispersionCoverageDefaultValue;

            // When
            var category = flowModel.GenerateGlobalValuesCategory();

            // Then
            Assert.That(GetPropertyValueAsDouble(category, ModelDefinitionsRegion.InitialSalinity.Key), Is.EqualTo(saltConcentrationDefaultValue));
            Assert.That(GetPropertyValueAsDouble(category, ModelDefinitionsRegion.Dispersion.Key), Is.EqualTo(dispersionCoverageDefaultValue));
        }

        [Test]
        public void GivenWaterFlowModel1DThatDoesNotUseSalt_WhenGeneratingGlobalValuesCategory_ThenNoSaltPropertiesAreGenerated()
        {
            // Given
            var flowModel = new WaterFlowModel1D
            {
                UseSalt = false
            };

            // When
            var category = flowModel.GenerateGlobalValuesCategory();

            // Then
            Assert.IsFalse(category.Properties.Any(p => p.Name == ModelDefinitionsRegion.InitialSalinity.Key));
            Assert.IsFalse(category.Properties.Any(p => p.Name == ModelDefinitionsRegion.Dispersion.Key));
        }

        [Test]
        public void GivenWaterFlowModel1DThatUsesTemperature_WhenGeneratingGlobalValuesCategory_ThenCorrectDataModelIsReturned()
        {
            // Given
            const double initialTemperatureDefaultValue = 4.0;

            var flowModel = new WaterFlowModel1D
            {
                UseTemperature = true
            };
            flowModel.InitialTemperature.DefaultValue = initialTemperatureDefaultValue;

            // When
            var category = flowModel.GenerateGlobalValuesCategory();

            // Then
            Assert.That(GetPropertyValueAsDouble(category, ModelDefinitionsRegion.InitialTemperature.Key), Is.EqualTo(initialTemperatureDefaultValue));
        }

        [Test]
        public void GivenWaterFlowModel1DThatDoesNotUseTemperature_WhenGeneratingGlobalValuesCategory_ThenNoTemperaturePropertiesAreGenerated()
        {
            // Given
            var flowModel = new WaterFlowModel1D
            {
                UseTemperature = false
            };

            // When
            var category = flowModel.GenerateGlobalValuesCategory();

            // Then
            Assert.IsFalse(category.Properties.Any(p => p.Name == ModelDefinitionsRegion.InitialTemperature.Key));
        }

        [Test]
        public void GivenWaterFlowModel1DWithKuijperVanRijnPrismaticDispersionFormulationType_WhenGeneratingGlobalValuesCategory_ThenCorrectDataModelIsReturned()
        {
            // Given
            const double dispersionF3DefaultValue = 3.0;
            const double dispersionF4DefaultValue = 4.0;

            var flowModel = new WaterFlowModel1D
            {
                DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic
            };
            flowModel.DispersionF3Coverage.DefaultValue = dispersionF3DefaultValue;
            flowModel.DispersionF4Coverage.DefaultValue = dispersionF4DefaultValue;

            // When
            var category = flowModel.GenerateGlobalValuesCategory();

            // Then
            Assert.That(GetPropertyValueAsDouble(category, ModelDefinitionsRegion.DispersionF3.Key), Is.EqualTo(dispersionF3DefaultValue));
            Assert.That(GetPropertyValueAsDouble(category, ModelDefinitionsRegion.DispersionF4.Key), Is.EqualTo(dispersionF4DefaultValue));
        }

        [Test]
        public void GivenWaterFlowModel1DWithConstantDispersionFormulationType_WhenGeneratingGlobalValuesCategory_ThenNoDispersionPropertiesAreGenerated()
        {
            // Given
            var flowModel = new WaterFlowModel1D
            {
                DispersionFormulationType = DispersionFormulationType.Constant
            };

            // When
            var category = flowModel.GenerateGlobalValuesCategory();

            // Then
            Assert.IsFalse(category.Properties.Any(p => p.Name == ModelDefinitionsRegion.DispersionF3.Key));
            Assert.IsFalse(category.Properties.Any(p => p.Name == ModelDefinitionsRegion.DispersionF4.Key));
        }

        private static double GetPropertyValueAsDouble(IDelftIniCategory category, string propertyName)
        {
            return double.Parse(category.GetPropertyValue(propertyName), CultureInfo.InvariantCulture);
        }
    }
}