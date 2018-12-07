using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;
using Rhino.Mocks;

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
            // Given
            var model = new WaterFlowModel1D
            {
                UseSalt = true,
                UseTemperature = true,
                DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic,
            };

            // set values to something other than default so we can verify they have been properly changed.
            model.InitialConditionsType = InitialConditionsType.Depth; // Depth == 1, level == 0
            model.DefaultInitialWaterLevel = 200.0;
            model.DefaultInitialDepth = 200.0;

            model.InitialFlow.DefaultValue = 77.77;
            model.InitialSaltConcentration.DefaultValue = 77.77;
            model.InitialTemperature.DefaultValue = 77.77;
            model.DispersionCoverage.DefaultValue = 77.77;
            model.DispersionF3Coverage.DefaultValue = 77.77;
            model.DispersionF4Coverage.DefaultValue = 77.77;

            var category = new DelftIniCategory(ModelDefinitionsRegion.GlobalValuesHeader);

            var mocks = new MockRepository();

            var someErrorReportFunction = mocks.StrictMock<Action<string, IList<string>>>();
            someErrorReportFunction.Expect(e => e.Invoke(null, null))
                .IgnoreArguments()
                .Repeat.Never();
            mocks.ReplayAll();

            // When
            new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, someErrorReportFunction);

            mocks.VerifyAll();
            Assert.That(model.InitialConditionsType, Is.EqualTo(InitialConditionsType.WaterLevel));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(0.0));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(0.0));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(0.0));
            Assert.That(model.InitialSaltConcentration.DefaultValue, Is.EqualTo(0.0));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(15.0));
            Assert.That(model.DispersionCoverage.DefaultValue, Is.EqualTo(0.0));
            Assert.That(model.DispersionF3Coverage.DefaultValue, Is.EqualTo(0.0));
            Assert.That(model.DispersionF4Coverage.DefaultValue, Is.EqualTo(0.0));
        }

        /// <summary>
        /// GIVEN a simple flow model
        ///   AND a GlobalValues header with custom values
        /// WHEN SetGlobalValues is called
        /// THEN the simple flow model contains the cutom values
        /// </summary>
        [Test]
        public void GivenASimpleFlowModelAndAGlobalValuesHeaderWithCustomValues_WhenSetGlobalValuesIsCalled_ThenTheSimpleFlowModelContainsTheCutomValues()
        {
            // Given
            var model = new WaterFlowModel1D
            {
                UseSalt = true,
                UseTemperature = true,
                DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic,
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

            var category = getGlobalCategoryWithCommonElements(expectedConditionsType,
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


            var mocks = new MockRepository();

            var someErrorReportFunction = mocks.StrictMock<Action<string, IList<string>>>();
            someErrorReportFunction.Expect(e => e.Invoke(null, null))
                .IgnoreArguments()
                .Repeat.Never();
            mocks.ReplayAll();

            // When
            new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, someErrorReportFunction);

            mocks.VerifyAll();
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

            var category = getGlobalCategoryWithCommonElements(expectedConditionsType,
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

            var mocks = new MockRepository();

            var someErrorReportFunction = mocks.StrictMock<Action<string, IList<string>>>();
            someErrorReportFunction.Expect(e => e.Invoke(null, null))
                .IgnoreArguments()
                .Repeat.Never();
            mocks.ReplayAll();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, someErrorReportFunction);
            Assert.DoesNotThrow(testAction); 

            mocks.VerifyAll();
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
                DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic,
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

            var category = getGlobalCategoryWithCommonElements(expectedConditionsType,
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

            var mocks = new MockRepository();

            var someErrorReportFunction = mocks.StrictMock<Action<string, IList<string>>>();
            someErrorReportFunction.Expect(e => e.Invoke(null, null))
                .IgnoreArguments()
                .Repeat.Never();
            mocks.ReplayAll();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, someErrorReportFunction);
            Assert.DoesNotThrow(testAction);

            mocks.VerifyAll();
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
            var category = getGlobalCategoryWithCommonElements(expectedConditionsType,
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

            var mocks = new MockRepository();

            var someErrorReportFunction = mocks.StrictMock<Action<string, IList<string>>>();
            someErrorReportFunction.Expect(e => e.Invoke(null, null))
                .IgnoreArguments()
                .Repeat.Never();
            mocks.ReplayAll();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, someErrorReportFunction);
            Assert.DoesNotThrow(testAction);

            mocks.VerifyAll();
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
            var category = getGlobalCategoryWithCommonElements(expectedConditionsType,
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

            var mocks = new MockRepository();

            var someErrorReportFunction = mocks.StrictMock<Action<string, IList<string>>>();
            someErrorReportFunction.Expect(e => e.Invoke(null, null))
                .IgnoreArguments()
                .Repeat.Never();
            mocks.ReplayAll();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, someErrorReportFunction);
            Assert.DoesNotThrow(testAction);

            mocks.VerifyAll();
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
            var category = getGlobalCategoryWithCommonElements(expectedConditionsType,
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

            var mocks = new MockRepository();

            var someErrorReportFunction = mocks.StrictMock<Action<string, IList<string>>>();
            someErrorReportFunction.Expect(e => e.Invoke(null, null))
                .IgnoreArguments()
                .Repeat.Never();
            mocks.ReplayAll();

            // When
            TestDelegate testAction = () => new WaterFlowModelGlobalValuesSetter().SetProperties(category, model, someErrorReportFunction);
            Assert.DoesNotThrow(testAction);

            mocks.VerifyAll();
            Assert.That(model.DispersionF4Coverage, Is.Null);

            Assert.That(model.InitialConditionsType, Is.EqualTo(expectedConditionsType));
            Assert.That(model.DefaultInitialWaterLevel, Is.EqualTo(expectedWaterLevel));
            Assert.That(model.DefaultInitialDepth, Is.EqualTo(expectedDepth));

            Assert.That(model.InitialFlow.DefaultValue, Is.EqualTo(expectedFlow));
            Assert.That(model.InitialTemperature.DefaultValue, Is.EqualTo(expectedTemperature));
        }

        private static DelftIniCategory getGlobalCategoryWithCommonElements(InitialConditionsType conditionsType,
                                                                            double waterLevel,
                                                                            double waterDepth,
                                                                            double flow)
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.GlobalValuesHeader);
            // Non dependent components
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
