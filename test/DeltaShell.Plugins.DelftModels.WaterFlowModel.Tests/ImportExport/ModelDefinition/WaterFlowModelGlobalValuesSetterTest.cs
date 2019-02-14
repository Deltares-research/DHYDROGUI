using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using NUnit.Framework;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelGlobalValuesSetterTest
    {
        private const InitialConditionsType ExpectedConditionsType = InitialConditionsType.WaterLevel;
        private const double ExpectedWaterLevel = 2.0;
        private const double ExpectedDepth = 4.0;
        private const double ExpectedFlow = 8.0;
        private const double ExpectedSalt = 16.0;
        private const double ExpectedTemperature = 32.0;
        private const double ExpectedDispersionCoverage = 64.0;
        private const double ExpectedF3Coverage = 128.0;
        private const double ExpectedF4Coverage = 256.0;

        [Test]
        public void WhenSettingGlobalValuesOnModelWithCategoryThatHasNoGlobalValuesName_ThenNoExceptionIsThrown()
        {
            Assert.DoesNotThrow(() => 
                new WaterFlowModelGlobalValuesSetter().SetProperties(new DelftIniCategory("UnknownHeader"), null, new List<string>()));
        }

        /// <summary>
        /// GIVEN a simple flow model
        ///   AND a GlobalValues header without any values
        /// WHEN SetGlobalValues is called
        /// THEN the simple flow model contains the default values
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelAndAGlobalValuesHeaderWithoutAnyValues_WhenSetGlobalValuesIsCalled_ThenTheSimpleFlowModelContainsTheDefaultValues()
        {
            // Given
            var model = new WaterFlowModel1D
            {
                UseSalt = true,
                UseTemperature = true
            };

            model.SetCustomInitialValues();

            var category = new DelftIniCategory(ModelDefinitionsRegion.GlobalValuesHeader);
            var errorMessages = new List<string>();

            // When
            new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);
            
            // Then
            Assert.IsEmpty(errorMessages);
            Assert.That(model.InitialConditionsType, Is.EqualTo(InitialConditionsType.WaterLevel));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(0.0));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(0.0));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(0.0));
            Assert.That(model.InitialSaltConcentration.DefaultValue, Is.EqualTo(0.0));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(15.0));
            Assert.That(model.DispersionCoverage.DefaultValue, Is.EqualTo(0.0));
        }

        /// <summary>
        /// GIVEN a simple flow model
        ///   AND a GlobalValues header with custom values
        /// WHEN SetGlobalValues is called
        /// THEN the simple flow model contains the custom values
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelAndAGlobalValuesHeaderWithCustomValues_WhenSetGlobalValuesIsCalled_ThenTheSimpleFlowModelContainsTheCustomValues()
        {
            // Given
            var model = new WaterFlowModel1D
            {
                UseSalt = true,
                UseTemperature = true,
            };

            var category = GetGlobalCategoryWithCommonElements(ExpectedConditionsType,
                                                               ExpectedWaterLevel,
                                                               ExpectedDepth,
                                                               ExpectedFlow);

            category.AddProperty(ModelDefinitionsRegion.InitialSalinity,
                                 ExpectedSalt);
            category.AddProperty(ModelDefinitionsRegion.Dispersion,
                                 ExpectedDispersionCoverage);
            category.AddProperty(ModelDefinitionsRegion.DispersionF3,
                                 ExpectedF3Coverage);
            category.AddProperty(ModelDefinitionsRegion.DispersionF4,
                                 ExpectedF4Coverage);
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature,
                                 ExpectedTemperature);

            var errorMessages = new List<string>();

            // When
            new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.IsEmpty(errorMessages);
            Assert.That(model.InitialConditionsType, Is.EqualTo(ExpectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(ExpectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(ExpectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(ExpectedFlow));
            Assert.That(model.InitialSaltConcentration.DefaultValue, Is.EqualTo(ExpectedSalt));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(ExpectedTemperature));
            Assert.That(model.DispersionCoverage.DefaultValue, Is.EqualTo(ExpectedDispersionCoverage));
            Assert.That(model.DispersionF3Coverage.DefaultValue, Is.EqualTo(ExpectedF3Coverage));
            Assert.That(model.DispersionF4Coverage.DefaultValue, Is.EqualTo(ExpectedF4Coverage));
        }

        /// <summary>
        /// GIVEN a simple flow model without Salinity
        ///   AND a GlobalValuesHeader with a salinity description
        /// WHEN SetGlobalValues is called
        /// THEN no changes should occur
        ///  AND no error should be thrown
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelWithoutSalinityAndAGlobalValuesHeaderWithASalinityDescription_WhenSetGlobalValuesIsCalled_ThenNoChangesShouldOccurAndNoErrorShouldBeThrown()
        {
            // Given
            var model = new WaterFlowModel1D
            {
                UseTemperature = true,
                InitialConditionsType = ExpectedConditionsType,
                DefaultInitialWaterLevel = ExpectedWaterLevel,
                DefaultInitialDepth = ExpectedDepth,
            };

            model.InitialFlow.DefaultValue = ExpectedFlow;
            model.InitialTemperature.DefaultValue = ExpectedTemperature;

            var category = GetGlobalCategoryWithCommonElements(ExpectedConditionsType,
                                          ExpectedWaterLevel,
                                          ExpectedDepth,
                                          ExpectedFlow);

            category.AddProperty(ModelDefinitionsRegion.InitialTemperature,
                                 ExpectedTemperature);
            category.AddProperty(ModelDefinitionsRegion.InitialSalinity,
                                 256.0);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.DoesNotThrow(testAction);

            Assert.IsEmpty(errorMessages);
            Assert.That(model.InitialSaltConcentration, Is.Null);
            Assert.That(model.InitialConditionsType, Is.EqualTo(ExpectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(ExpectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(ExpectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(ExpectedFlow));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(ExpectedTemperature));
        }

        /// <summary>
        /// GIVEN a simple flow model without Temperature
        ///   AND a GlobalValuesHeader with a temperature description
        /// WHEN SetGlobalValues is called
        /// THEN no changes should occur
        ///  AND no error should be thrown
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelWithoutTemperatureAndAGlobalValuesHeaderWithATemperatureDescription_WhenSetGlobalValuesIsCalled_ThenNoChangesShouldOccurAndNoErrorShouldBeThrown()
        {
            // Given
            var model = new WaterFlowModel1D
            {
                UseSalt = true,
            };

            var category = GetGlobalCategoryWithCommonElements(ExpectedConditionsType,
                                                   ExpectedWaterLevel,
                                                   ExpectedDepth,
                                                   ExpectedFlow);

            category.AddProperty(ModelDefinitionsRegion.InitialSalinity,
                                 ExpectedSalt);
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature,
                                 ExpectedTemperature);
            category.AddProperty(ModelDefinitionsRegion.Dispersion,
                                 ExpectedDispersionCoverage);
            category.AddProperty(ModelDefinitionsRegion.DispersionF3,
                                 ExpectedF3Coverage);
            category.AddProperty(ModelDefinitionsRegion.DispersionF4,
                                 ExpectedF4Coverage);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.DoesNotThrow(testAction);

            Assert.IsEmpty(errorMessages);
            Assert.That(model.InitialConditionsType, Is.EqualTo(ExpectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(ExpectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(ExpectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(ExpectedFlow));
            Assert.That(model.InitialSaltConcentration.DefaultValue, Is.EqualTo(ExpectedSalt));
            Assert.That(model.DispersionCoverage.DefaultValue, Is.EqualTo(ExpectedDispersionCoverage));
            Assert.That(model.DispersionF3Coverage.DefaultValue, Is.EqualTo(ExpectedF3Coverage));
            Assert.That(model.DispersionF4Coverage.DefaultValue, Is.EqualTo(ExpectedF4Coverage));

            Assert.That(model.InitialTemperature, Is.Null);
        }

        /// <summary>
        /// GIVEN a simple flow model without Dispersion
        ///   AND a GlobalValuesHeader with a dispersion description
        /// WHEN SetGlobalValues is called
        /// THEN no changes should occur
        ///  AND no error should be thrown
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelWithoutDispersionAndAGlobalValuesHeaderWithADispersionDescription_WhenSetGlobalValuesIsCalled_ThenNoChangesShouldOccurAndNoErrorShouldBeThrown()
        {
            // Given
            var model = new WaterFlowModel1D
            {
                UseTemperature = true,
                InitialConditionsType = ExpectedConditionsType,
                DefaultInitialWaterLevel = ExpectedWaterLevel,
                DefaultInitialDepth = ExpectedDepth,
            };

            model.InitialFlow.DefaultValue = ExpectedFlow;
            model.InitialTemperature.DefaultValue = ExpectedTemperature;
            var category = GetGlobalCategoryWithCommonElements(ExpectedConditionsType,
                                                               ExpectedWaterLevel,
                                                               ExpectedDepth,
                                                               ExpectedFlow);

            category.AddProperty(ModelDefinitionsRegion.InitialTemperature,
                                 ExpectedTemperature);
            category.AddProperty(ModelDefinitionsRegion.Dispersion,
                                 256.0);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.DoesNotThrow(testAction);

            Assert.IsEmpty(errorMessages);
            Assert.That(model.DispersionCoverage, Is.Null);

            Assert.That(model.InitialConditionsType, Is.EqualTo(ExpectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(ExpectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(ExpectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(ExpectedFlow));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(ExpectedTemperature));
        }

        /// <summary>
        /// GIVEN a simple flow model without Dispersion F3
        ///   AND a GlobalValuesHeader with a dispersion F3 description
        /// WHEN SetGlobalValues is called
        /// THEN no changes should occur
        ///  AND no error should be thrown
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelWithoutDispersionF3AndAGlobalValuesHeaderWithADispersionF3Description_WhenSetGlobalValuesIsCalled_ThenNoChangesShouldOccurAndNoErrorShouldBeThrown()
        {
            // Given
            var model = new WaterFlowModel1D
            {
                UseTemperature = true,
                InitialConditionsType = ExpectedConditionsType,
                DefaultInitialWaterLevel = ExpectedWaterLevel,
                DefaultInitialDepth = ExpectedDepth,
            };

            model.InitialFlow.DefaultValue = ExpectedFlow;
            model.InitialTemperature.DefaultValue = ExpectedTemperature;
            var category = GetGlobalCategoryWithCommonElements(ExpectedConditionsType,
                                                               ExpectedWaterLevel,
                                                               ExpectedDepth,
                                                               ExpectedFlow);

            category.AddProperty(ModelDefinitionsRegion.InitialTemperature,
                                 ExpectedTemperature);
            category.AddProperty(ModelDefinitionsRegion.DispersionF3,
                                 256.0);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.DoesNotThrow(testAction);

            Assert.IsEmpty(errorMessages);
            Assert.That(model.DispersionF3Coverage, Is.Null);

            Assert.That(model.InitialConditionsType, Is.EqualTo(ExpectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(ExpectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(ExpectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(ExpectedFlow));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(ExpectedTemperature));
        }

        /// <summary>
        /// GIVEN a simple flow model without Dispersion F4
        ///   AND a GlobalValuesHeader with a dispersion F4 description
        /// WHEN SetGlobalValues is called
        /// THEN no changes should occur
        ///  AND no error should be thrown
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelWithoutDispersionF4AndAGlobalValuesHeaderWithADispersionF4Description_WhenSetGlobalValuesIsCalled_ThenNoChangesShouldOccurAndNoErrorShouldBeThrown()
        {
            // Given
            var model = new WaterFlowModel1D
            {
                UseTemperature = true,
                InitialConditionsType = ExpectedConditionsType,
                DefaultInitialWaterLevel = ExpectedWaterLevel,
                DefaultInitialDepth = ExpectedDepth,
            };

            model.InitialFlow.DefaultValue = ExpectedFlow;
            model.InitialTemperature.DefaultValue = ExpectedTemperature;
            var category = GetGlobalCategoryWithCommonElements(ExpectedConditionsType,
                                                               ExpectedWaterLevel,
                                                               ExpectedDepth,
                                                               ExpectedFlow);

            category.AddProperty(ModelDefinitionsRegion.InitialTemperature,
                                 ExpectedTemperature);
            category.AddProperty(ModelDefinitionsRegion.DispersionF4,
                                 256.0);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);
            
            // Then
            Assert.DoesNotThrow(testAction);

            Assert.IsEmpty(errorMessages);
            Assert.That(model.DispersionF4Coverage, Is.Null);

            Assert.That(model.InitialConditionsType, Is.EqualTo(ExpectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(ExpectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(ExpectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(ExpectedFlow));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(ExpectedTemperature));
        }

        /// <summary>
        /// GIVEN a simple flow model
        ///   AND a GlobalValues header with F3 and F4 values
        /// WHEN SetGlobalValues is called
        /// THEN the simple model has KuijperVanRijn dispersion formula
        ///  AND the F3 and F4 values are correctly set
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelAndAGlobalValuesHeaderWithF3AndF4Values_WhenSetGlobalValuesIsCalled_ThenTheSimpleModelHasKuijperVanRijnDispersionFormulaAndTheF3AndF4ValuesAreCorrectlySet()
        {
            // Given
            var model = new WaterFlowModel1D
            {
                UseSalt = true,
                UseTemperature = true,
            };

            var category = GetGlobalCategoryWithCommonElements(ExpectedConditionsType,
                                                   ExpectedWaterLevel,
                                                   ExpectedDepth,
                                                   ExpectedFlow);

            category.AddProperty(ModelDefinitionsRegion.InitialSalinity,
                                 ExpectedSalt);
            category.AddProperty(ModelDefinitionsRegion.Dispersion,
                                 ExpectedDispersionCoverage);
            category.AddProperty(ModelDefinitionsRegion.DispersionF3,
                                 ExpectedF3Coverage);
            category.AddProperty(ModelDefinitionsRegion.DispersionF4,
                                 ExpectedF4Coverage);
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature,
                                 ExpectedTemperature);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.DoesNotThrow(testAction);
            Assert.IsEmpty(errorMessages);

            Assert.That(model.DispersionFormulationType, Is.EqualTo(DispersionFormulationType.KuijperVanRijnPrismatic));
            Assert.That(model.DispersionF3Coverage.DefaultValue, Is.EqualTo(ExpectedF3Coverage));
            Assert.That(model.DispersionF4Coverage.DefaultValue, Is.EqualTo(ExpectedF4Coverage));
        }

        /// <summary>
        /// GIVEN a simple flow model
        ///   AND a GlobalValues header without F3 and F4 values
        /// WHEN SetGlobalValues is called
        /// THEN the simple model has a constant dispersion formula
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelAndAGlobalValuesHeaderWithoutF3AndF4Values_WhenSetGlobalValuesIsCalled_ThenTheSimpleModelHasAConstantDispersionFormula()
        {
            // Given
            var model = new WaterFlowModel1D
            {
                UseSalt = true,
                UseTemperature = true,
            };

            var category = GetGlobalCategoryWithCommonElements(ExpectedConditionsType,
                                                   ExpectedWaterLevel,
                                                   ExpectedDepth,
                                                   ExpectedFlow);

            category.AddProperty(ModelDefinitionsRegion.InitialSalinity,
                                 ExpectedSalt);
            category.AddProperty(ModelDefinitionsRegion.Dispersion,
                                 ExpectedDispersionCoverage);
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature,
                                 ExpectedTemperature);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.DoesNotThrow(testAction);
            Assert.IsEmpty(errorMessages);

            Assert.That(model.DispersionFormulationType, Is.EqualTo(DispersionFormulationType.Constant));
        }

        /// <summary>
        /// GIVEN a simple flow model
        ///   AND a GlobalValues header without F3 value
        /// WHEN SetGlobalValues is called
        /// THEN the simple model has a constant dispersion formula
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelAndAGlobalValuesHeaderWithoutF3Value_WhenSetGlobalValuesIsCalled_ThenTheSimpleModelHasAConstantDispersionFormula()
        {
            // Given
            var model = new WaterFlowModel1D
            {
                UseSalt = true,
                UseTemperature = true,
            };

            var category = GetGlobalCategoryWithCommonElements(ExpectedConditionsType,
                                                   ExpectedWaterLevel,
                                                   ExpectedDepth,
                                                   ExpectedFlow);
            
            category.AddProperty(ModelDefinitionsRegion.InitialSalinity,
                                 ExpectedSalt);
            category.AddProperty(ModelDefinitionsRegion.Dispersion,
                                 ExpectedDispersionCoverage);
            category.AddProperty(ModelDefinitionsRegion.DispersionF4,
                                 ExpectedF4Coverage);
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature,
                                 ExpectedTemperature);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.DoesNotThrow(testAction);
            Assert.IsEmpty(errorMessages);
            Assert.That(model.DispersionFormulationType, Is.EqualTo(DispersionFormulationType.Constant));
        }

        /// <summary>
        /// GIVEN a simple flow model
        ///   AND a GlobalValues header without F4 value
        /// WHEN SetGlobalValues is called
        /// THEN the simple model has a constant dispersion formula
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelAndAGlobalValuesHeaderWithoutF4Value_WhenSetGlobalValuesIsCalled_ThenTheSimpleModelHasAConstantDispersionFormula()
        {
            // Given
            var model = new WaterFlowModel1D
            {
                UseSalt = true,
                UseTemperature = true,
            };

            var category = GetGlobalCategoryWithCommonElements(ExpectedConditionsType,
                                                   ExpectedWaterLevel,
                                                   ExpectedDepth,
                                                   ExpectedFlow);

            category.AddProperty(ModelDefinitionsRegion.InitialSalinity,
                                 ExpectedSalt);
            category.AddProperty(ModelDefinitionsRegion.Dispersion,
                                 ExpectedDispersionCoverage);
            category.AddProperty(ModelDefinitionsRegion.DispersionF3,
                                 ExpectedF3Coverage);
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature,
                                 ExpectedTemperature);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);
            Assert.DoesNotThrow(testAction);

            // Then
            Assert.IsEmpty(errorMessages);
            Assert.That(model.DispersionFormulationType, Is.EqualTo(DispersionFormulationType.Constant));
        }

        /// <summary>
        /// GIVEN a simple flow model without UseSalt
        ///   AND a GlobalValues header with F3 and F4 values
        /// WHEN SetGlobalValues is called
        /// THEN the simple model has a constant dispersion formula
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelWithoutUseSaltAndAGlobalValuesHeaderWithF3AndF4Values_WhenSetGlobalValuesIsCalled_ThenTheSimpleModelHasAConstantDispersionFormula()
        {
            // Given
            var model = new WaterFlowModel1D
            {

                UseTemperature = true,
            };

            var category = GetGlobalCategoryWithCommonElements(ExpectedConditionsType,
                ExpectedWaterLevel,
                ExpectedDepth,
                ExpectedFlow);

            category.AddProperty(ModelDefinitionsRegion.InitialSalinity,
                ExpectedSalt);
            category.AddProperty(ModelDefinitionsRegion.Dispersion,
                ExpectedDispersionCoverage);
            category.AddProperty(ModelDefinitionsRegion.DispersionF3,
                ExpectedF3Coverage);
            category.AddProperty(ModelDefinitionsRegion.DispersionF4,
                ExpectedF4Coverage);
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature,
                ExpectedTemperature);

            var errorMessages = new List<string>();
            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.DoesNotThrow(testAction);
            Assert.IsEmpty(errorMessages);
            Assert.That(model.DispersionFormulationType, Is.EqualTo(DispersionFormulationType.Constant));
        }

        [Test]
        public void GivenGlobalValuesDataModelWithUnknownProperty_WhenSettingModelProperties_ThenUnknownPropertyIsSkippedAndErrorMessageIsReturned()
        {
            // Given
            const string unknownPropertyName = "UnknownProperty";
            var category = GetGlobalCategoryWithCommonElements(ExpectedConditionsType, ExpectedWaterLevel, ExpectedDepth, ExpectedFlow);
            category.AddProperty(unknownPropertyName, 1);

            // When
            var errorMessages = new List<string>();
            var model = new WaterFlowModel1D();
            new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            // Then
            Assert.That(errorMessages.Count, Is.EqualTo(1));
            var expectedMessage = string.Format(Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                0, unknownPropertyName);
            Assert.Contains(expectedMessage, errorMessages);
        }

        private static DelftIniCategory GetGlobalCategoryWithCommonElements(InitialConditionsType conditionsType,
                                                                            double waterLevel,
                                                                            double waterDepth,
                                                                            double flow)
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.GlobalValuesHeader);
            
            // Non-dependent components
            category.AddProperty(ModelDefinitionsRegion.UseInitialWaterDepth.Key,
                conditionsType == InitialConditionsType.Depth ? 1 : 0,
                ModelDefinitionsRegion.UseInitialWaterDepth.Description);
            category.AddProperty(ModelDefinitionsRegion.InitialWaterLevel,
                waterLevel);
            category.AddProperty(ModelDefinitionsRegion.InitialWaterDepth,
                                 waterDepth);
            category.AddProperty(ModelDefinitionsRegion.InitialDischarge,
                                 flow);

            return category;
        }
    }

    public static class DelftIniCategoryExtension
    {
        internal static void SetCustomInitialValues(this WaterFlowModel1D model)
        {
            // set values to something other than default so we can verify they have been properly changed.
            model.InitialConditionsType = InitialConditionsType.Depth; // Depth == 1, level == 0
            model.DefaultInitialWaterLevel = 200.0;
            model.DefaultInitialDepth = 200.0;

            model.InitialFlow.DefaultValue = 77.77;
            model.InitialSaltConcentration.DefaultValue = 77.77;
            model.InitialTemperature.DefaultValue = 77.77;
            model.DispersionCoverage.DefaultValue = 77.77;
        }
    }
}
