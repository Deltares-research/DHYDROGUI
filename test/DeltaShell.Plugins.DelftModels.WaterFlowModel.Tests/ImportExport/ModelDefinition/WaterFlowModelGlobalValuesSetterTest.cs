using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelGlobalValuesSetterTest
    {
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
                UseTemperature = true,
            };

            // set values to something other than default so we can verify they have been properly changed.
            model.InitialConditionsType = InitialConditionsType.Depth; // Depth == 1, level == 0
            model.DefaultInitialWaterLevel = 200.0;
            model.DefaultInitialDepth = 200.0;

            model.InitialFlow.DefaultValue = 77.77;
            model.InitialSaltConcentration.DefaultValue = 77.77;
            model.InitialTemperature.DefaultValue = 77.77;
            model.DispersionCoverage.DefaultValue = 77.77;

            var category = new DelftIniCategory(ModelDefinitionsRegion.GlobalValuesHeader);
            var errorMessages = new List<string>();

            // When
            new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);
            
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

            const InitialConditionsType expectedConditionsType = InitialConditionsType.WaterLevel;
            const double expectedWaterLevel = 2.0;
            const double expectedDepth = 4.0;
            const double expectedFlow = 8.0;
            const double expectedSalt = 16.0;
            const double expectedTemperature = 32.0;
            const double expectedDispersionCoverage = 64.0;
            const double expectedF3Coverage = 128.0;
            const double expectedF4Coverage = 256.0;

            var category = GetGlobalCategoryWithCommonElements(expectedConditionsType,
                                                   expectedWaterLevel,
                                                   expectedDepth,
                                                   expectedFlow);
            // Salt
            category.AddProperty(ModelDefinitionsRegion.InitialSalinity.Key,
                                 expectedSalt,
                                 ModelDefinitionsRegion.InitialSalinity.Description,
                                 ModelDefinitionsRegion.InitialSalinity.Format);
            category.AddProperty(ModelDefinitionsRegion.Dispersion.Key,
                                 expectedDispersionCoverage,
                                 ModelDefinitionsRegion.Dispersion.Description,
                                 ModelDefinitionsRegion.Dispersion.Format);
            category.AddProperty(ModelDefinitionsRegion.DispersionF3.Key,
                                 expectedF3Coverage,
                                 ModelDefinitionsRegion.DispersionF3.Description,
                                 ModelDefinitionsRegion.DispersionF3.Format);
            category.AddProperty(ModelDefinitionsRegion.DispersionF4.Key,
                                 expectedF4Coverage,
                                 ModelDefinitionsRegion.DispersionF4.Description,
                                 ModelDefinitionsRegion.DispersionF4.Format);

            // Temperature
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature.Key,
                expectedTemperature,
                ModelDefinitionsRegion.InitialTemperature.Description,
                ModelDefinitionsRegion.InitialTemperature.Format);


            var errorMessages = new List<string>();

            // When
            new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            Assert.IsEmpty(errorMessages);
            Assert.That(model.InitialConditionsType, Is.EqualTo(expectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(expectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(expectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(expectedFlow));
            Assert.That(model.InitialSaltConcentration.DefaultValue, Is.EqualTo(expectedSalt));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(expectedTemperature));
            Assert.That(model.DispersionCoverage.DefaultValue, Is.EqualTo(expectedDispersionCoverage));
            Assert.That(model.DispersionF3Coverage.DefaultValue, Is.EqualTo(expectedF3Coverage));
            Assert.That(model.DispersionF4Coverage.DefaultValue, Is.EqualTo(expectedF4Coverage));
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
            const InitialConditionsType expectedConditionsType = InitialConditionsType.WaterLevel;
            const double expectedWaterLevel = 2.0;
            const double expectedDepth = 4.0;
            const double expectedFlow = 8.0;
            const double expectedTemperature = 32.0;

            // Given
            var model = new WaterFlowModel1D
            {
                UseTemperature = true,
                InitialConditionsType = expectedConditionsType,
                DefaultInitialWaterLevel = expectedWaterLevel,
                DefaultInitialDepth = expectedDepth,
            };

            model.InitialFlow.DefaultValue = expectedFlow;
            model.InitialTemperature.DefaultValue = expectedTemperature;

            var category = GetGlobalCategoryWithCommonElements(expectedConditionsType,
                                          expectedWaterLevel,
                                          expectedDepth,
                                          expectedFlow);

            // Temperature Component
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature.Key,
                expectedTemperature,
                ModelDefinitionsRegion.InitialTemperature.Description,
                ModelDefinitionsRegion.InitialTemperature.Format);



            // Salt component
            category.AddProperty(ModelDefinitionsRegion.InitialSalinity.Key,
                                 256.0,
                                 ModelDefinitionsRegion.InitialSalinity.Description,
                                 ModelDefinitionsRegion.InitialSalinity.Format);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);
            Assert.DoesNotThrow(testAction);

            Assert.IsEmpty(errorMessages);
            Assert.That(model.InitialSaltConcentration, Is.Null);
            Assert.That(model.InitialConditionsType, Is.EqualTo(expectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(expectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(expectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(expectedFlow));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(expectedTemperature));
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

            const InitialConditionsType expectedConditionsType = InitialConditionsType.WaterLevel;
            const double expectedWaterLevel = 2.0;
            const double expectedDepth = 4.0;
            const double expectedFlow = 8.0;
            const double expectedSalt = 16.0;
            const double expectedTemperature = 32.0;
            const double expectedDispersionCoverage = 64.0;
            const double expectedF3Coverage = 128.0;
            const double expectedF4Coverage = 256.0;

            var category = GetGlobalCategoryWithCommonElements(expectedConditionsType,
                                                   expectedWaterLevel,
                                                   expectedDepth,
                                                   expectedFlow);

            category.AddProperty(ModelDefinitionsRegion.InitialSalinity.Key,
                                 expectedSalt,
                                 ModelDefinitionsRegion.InitialSalinity.Description,
                                 ModelDefinitionsRegion.InitialSalinity.Format);
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature.Key,
                                 expectedTemperature,
                                 ModelDefinitionsRegion.InitialTemperature.Description,
                                 ModelDefinitionsRegion.InitialTemperature.Format);
            category.AddProperty(ModelDefinitionsRegion.Dispersion.Key,
                                 expectedDispersionCoverage,
                                 ModelDefinitionsRegion.Dispersion.Description,
                                 ModelDefinitionsRegion.Dispersion.Format);
            category.AddProperty(ModelDefinitionsRegion.DispersionF3.Key,
                                 expectedF3Coverage,
                                 ModelDefinitionsRegion.DispersionF3.Description,
                                 ModelDefinitionsRegion.DispersionF3.Format);
            category.AddProperty(ModelDefinitionsRegion.DispersionF4.Key,
                                 expectedF4Coverage,
                                 ModelDefinitionsRegion.DispersionF4.Description,
                                 ModelDefinitionsRegion.DispersionF4.Format);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);
            Assert.DoesNotThrow(testAction);

            Assert.IsEmpty(errorMessages);
            Assert.That(model.InitialConditionsType, Is.EqualTo(expectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(expectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(expectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(expectedFlow));
            Assert.That(model.InitialSaltConcentration.DefaultValue, Is.EqualTo(expectedSalt));
            Assert.That(model.DispersionCoverage.DefaultValue, Is.EqualTo(expectedDispersionCoverage));
            Assert.That(model.DispersionF3Coverage.DefaultValue, Is.EqualTo(expectedF3Coverage));
            Assert.That(model.DispersionF4Coverage.DefaultValue, Is.EqualTo(expectedF4Coverage));

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
            const InitialConditionsType expectedConditionsType = InitialConditionsType.WaterLevel;
            const double expectedWaterLevel = 2.0;
            const double expectedDepth = 4.0;
            const double expectedFlow = 8.0;
            const double expectedTemperature = 32.0;

            // Given
            var model = new WaterFlowModel1D
            {
                UseTemperature = true,
                InitialConditionsType = expectedConditionsType,
                DefaultInitialWaterLevel = expectedWaterLevel,
                DefaultInitialDepth = expectedDepth,
            };

            model.InitialFlow.DefaultValue = expectedFlow;
            model.InitialTemperature.DefaultValue = expectedTemperature;
            var category = GetGlobalCategoryWithCommonElements(expectedConditionsType,
                                                               expectedWaterLevel,
                                                               expectedDepth,
                                                               expectedFlow);

            // Temperature Component
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature.Key,
                expectedTemperature,
                ModelDefinitionsRegion.InitialTemperature.Description,
                ModelDefinitionsRegion.InitialTemperature.Format);



            // Salt component
            category.AddProperty(ModelDefinitionsRegion.Dispersion.Key,
                                 256.0,
                                 ModelDefinitionsRegion.Dispersion.Description,
                                 ModelDefinitionsRegion.Dispersion.Format);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);
            Assert.DoesNotThrow(testAction);

            Assert.IsEmpty(errorMessages);
            Assert.That(model.DispersionCoverage, Is.Null);

            Assert.That(model.InitialConditionsType, Is.EqualTo(expectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(expectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(expectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(expectedFlow));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(expectedTemperature));
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
            const InitialConditionsType expectedConditionsType = InitialConditionsType.WaterLevel;
            const double expectedWaterLevel = 2.0;
            const double expectedDepth = 4.0;
            const double expectedFlow = 8.0;
            const double expectedTemperature = 32.0;

            // Given
            var model = new WaterFlowModel1D
            {
                UseTemperature = true,
                InitialConditionsType = expectedConditionsType,
                DefaultInitialWaterLevel = expectedWaterLevel,
                DefaultInitialDepth = expectedDepth,
            };

            model.InitialFlow.DefaultValue = expectedFlow;
            model.InitialTemperature.DefaultValue = expectedTemperature;
            var category = GetGlobalCategoryWithCommonElements(expectedConditionsType,
                                                               expectedWaterLevel,
                                                               expectedDepth,
                                                               expectedFlow);

            // Temperature Component
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature.Key,
                expectedTemperature,
                ModelDefinitionsRegion.InitialTemperature.Description,
                ModelDefinitionsRegion.InitialTemperature.Format);



            // Salt component
            category.AddProperty(ModelDefinitionsRegion.DispersionF3.Key,
                                 256.0,
                                 ModelDefinitionsRegion.DispersionF3.Description,
                                 ModelDefinitionsRegion.DispersionF3.Format);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);
            Assert.DoesNotThrow(testAction);

            Assert.IsEmpty(errorMessages);
            Assert.That(model.DispersionF3Coverage, Is.Null);

            Assert.That(model.InitialConditionsType, Is.EqualTo(expectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(expectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(expectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(expectedFlow));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(expectedTemperature));
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
            const InitialConditionsType expectedConditionsType = InitialConditionsType.WaterLevel;
            const double expectedWaterLevel = 2.0;
            const double expectedDepth = 4.0;
            const double expectedFlow = 8.0;
            const double expectedTemperature = 32.0;

            // Given
            var model = new WaterFlowModel1D
            {
                UseTemperature = true,
                InitialConditionsType = expectedConditionsType,
                DefaultInitialWaterLevel = expectedWaterLevel,
                DefaultInitialDepth = expectedDepth,
            };

            model.InitialFlow.DefaultValue = expectedFlow;
            model.InitialTemperature.DefaultValue = expectedTemperature;
            var category = GetGlobalCategoryWithCommonElements(expectedConditionsType,
                                                               expectedWaterLevel,
                                                               expectedDepth,
                                                               expectedFlow);

            // Temperature Component
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature.Key,
                expectedTemperature,
                ModelDefinitionsRegion.InitialTemperature.Description,
                ModelDefinitionsRegion.InitialTemperature.Format);



            // Salt component
            category.AddProperty(ModelDefinitionsRegion.DispersionF4.Key,
                                 256.0,
                                 ModelDefinitionsRegion.DispersionF4.Description,
                                 ModelDefinitionsRegion.DispersionF4.Format);

            var errorMessages = new List<string>();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);
            Assert.DoesNotThrow(testAction);

            Assert.IsEmpty(errorMessages);
            Assert.That(model.DispersionF4Coverage, Is.Null);

            Assert.That(model.InitialConditionsType, Is.EqualTo(expectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(expectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(expectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(expectedFlow));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(expectedTemperature));
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

            const InitialConditionsType expectedConditionsType = InitialConditionsType.WaterLevel;
            const double expectedWaterLevel = 2.0;
            const double expectedDepth = 4.0;
            const double expectedFlow = 8.0;
            const double expectedSalt = 16.0;
            const double expectedTemperature = 32.0;
            const double expectedDispersionCoverage = 64.0;
            const double expectedF3Coverage = 128.0;
            const double expectedF4Coverage = 256.0;

            var category = GetGlobalCategoryWithCommonElements(expectedConditionsType,
                                                   expectedWaterLevel,
                                                   expectedDepth,
                                                   expectedFlow);
            // Salt
            category.AddProperty(ModelDefinitionsRegion.InitialSalinity.Key,
                                 expectedSalt,
                                 ModelDefinitionsRegion.InitialSalinity.Description,
                                 ModelDefinitionsRegion.InitialSalinity.Format);
            category.AddProperty(ModelDefinitionsRegion.Dispersion.Key,
                                 expectedDispersionCoverage,
                                 ModelDefinitionsRegion.Dispersion.Description,
                                 ModelDefinitionsRegion.Dispersion.Format);
            category.AddProperty(ModelDefinitionsRegion.DispersionF3.Key,
                                 expectedF3Coverage,
                                 ModelDefinitionsRegion.DispersionF3.Description,
                                 ModelDefinitionsRegion.DispersionF3.Format);
            category.AddProperty(ModelDefinitionsRegion.DispersionF4.Key,
                                 expectedF4Coverage,
                                 ModelDefinitionsRegion.DispersionF4.Description,
                                 ModelDefinitionsRegion.DispersionF4.Format);

            // Temperature
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature.Key,
                expectedTemperature,
                ModelDefinitionsRegion.InitialTemperature.Description,
                ModelDefinitionsRegion.InitialTemperature.Format);


            var errorMessages = new List<string>();

            // When
            new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            Assert.IsEmpty(errorMessages);

            Assert.That(model.DispersionFormulationType, Is.EqualTo(DispersionFormulationType.KuijperVanRijnPrismatic));
            Assert.That(model.DispersionF3Coverage.DefaultValue, Is.EqualTo(expectedF3Coverage));
            Assert.That(model.DispersionF4Coverage.DefaultValue, Is.EqualTo(expectedF4Coverage));
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

            const InitialConditionsType expectedConditionsType = InitialConditionsType.WaterLevel;
            const double expectedWaterLevel = 2.0;
            const double expectedDepth = 4.0;
            const double expectedFlow = 8.0;
            const double expectedSalt = 16.0;
            const double expectedTemperature = 32.0;
            const double expectedDispersionCoverage = 64.0;

            var category = GetGlobalCategoryWithCommonElements(expectedConditionsType,
                                                   expectedWaterLevel,
                                                   expectedDepth,
                                                   expectedFlow);
            // Salt
            category.AddProperty(ModelDefinitionsRegion.InitialSalinity.Key,
                                 expectedSalt,
                                 ModelDefinitionsRegion.InitialSalinity.Description,
                                 ModelDefinitionsRegion.InitialSalinity.Format);
            category.AddProperty(ModelDefinitionsRegion.Dispersion.Key,
                                 expectedDispersionCoverage,
                                 ModelDefinitionsRegion.Dispersion.Description,
                                 ModelDefinitionsRegion.Dispersion.Format);

            // Temperature
            category.AddProperty(ModelDefinitionsRegion.InitialTemperature.Key,
                expectedTemperature,
                ModelDefinitionsRegion.InitialTemperature.Description,
                ModelDefinitionsRegion.InitialTemperature.Format);


            var errorMessages = new List<string>();

            // When
            new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, errorMessages);

            Assert.IsEmpty(errorMessages);

            Assert.That(model.DispersionFormulationType, Is.EqualTo(DispersionFormulationType.Constant));
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
            category.AddProperty(ModelDefinitionsRegion.InitialWaterLevel.Key,
                waterLevel,
                ModelDefinitionsRegion.InitialWaterLevel.Description,
                ModelDefinitionsRegion.InitialWaterLevel.Format);
            category.AddProperty(ModelDefinitionsRegion.InitialWaterDepth.Key,
                waterDepth,
                ModelDefinitionsRegion.InitialWaterDepth.Description,
                ModelDefinitionsRegion.InitialWaterDepth.Format);
            category.AddProperty(ModelDefinitionsRegion.InitialDischarge.Key,
                flow,
                ModelDefinitionsRegion.InitialDischarge.Description,
                ModelDefinitionsRegion.InitialDischarge.Format);

            return category;
        }
    }
}
