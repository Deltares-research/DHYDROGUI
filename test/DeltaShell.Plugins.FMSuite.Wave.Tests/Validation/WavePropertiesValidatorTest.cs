using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WavePropertiesValidatorTest
    {
        [Test]
        /* Units tests created after DELFT3DFM-510 */
        [TestCase(0.0, false, InputFieldDataType.TimeVarying, false)]
        [TestCase(0.0, false, InputFieldDataType.Constant, false)]
        [TestCase(0.0, true, InputFieldDataType.TimeVarying, true)]
        [TestCase(0.0, true, InputFieldDataType.Constant, true)]
        [TestCase(30.0, false, InputFieldDataType.TimeVarying, false)]
        [TestCase(30.0, false, InputFieldDataType.Constant, false)]
        [TestCase(30.0, true, InputFieldDataType.TimeVarying, false)]
        [TestCase(30.0, true, InputFieldDataType.Constant, false)]
        [Category(TestCategory.Integration)]
        public void CheckWavePropertiesWithFlowModel(double windSpeed, bool quadruplets, InputFieldDataType windType, bool warningAlert)
        {
            var model = new WaveModel();
            var reportMessage = Resources.WavePropertiesValidator_ValidateWindSpeedAndQuadruple_WindSpeed_is_zero_whereas_quadruple_is_true_;
            var reportSeverity = ValidationSeverity.Error;

            /* Assigning variables */
            model.TimePointData.WindSpeedConstant = windSpeed;
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.Quadruplets).
                Value = quadruplets;
            model.TimePointData.WindDataType = windType;

            /*Test*/
            var validationReport = WavePropertiesValidator.Validate(model);
            if (warningAlert)
            {
                Assert.IsTrue(validationReport.GetAllIssuesRecursive().Any(i => i.Severity == reportSeverity && i.Message == reportMessage));
            }
            else
            {
                Assert.IsFalse(validationReport.GetAllIssuesRecursive().Any(i => i.Severity == reportSeverity && i.Message == reportMessage));
            }
        }

        [Test]
        public void GivenAWaveModel_WhenTScaleAndTimeStepAreIntegersAndDivisors_ThenNoWarningsAndErrorsShouldBeGiven()
        {
            var model = new WaveModel();
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.SimulationMode).SetValueAsString("non-stationary");
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.TimeScale).SetValueAsString("60");
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.TimeStep).SetValueAsString("10");
            model.TimePointData.WindDataType = InputFieldDataType.Constant;
            model.TimePointData.WindSpeedConstant = 10;

            var validationReport = WavePropertiesValidator.Validate(model);

            Assert.AreEqual(0, validationReport.GetAllIssuesRecursive().Count);

        }

        [Test]
        public void GivenAWaveModel_WhenTScaleIsNotAnIntegerAndTimeStepNotADivisor_ThenAnErrorAndWarningShouldBeGiven()
        {
            var model = new WaveModel();
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory,KnownWaveProperties.SimulationMode).SetValueAsString("non-stationary");
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory,KnownWaveProperties.TimeScale).SetValueAsString("60.1");
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.TimeStep).SetValueAsString("10");

            var expectedMessage1 = Resources.WavePropertiesValidator_ValidateTScale;
            var expectedMessage2 = Resources.WavePropertiesValidator_ValidateDivisor;

            var validationReport = WavePropertiesValidator.Validate(model);

            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                    .Any(
                        i =>
                            i.Severity == ValidationSeverity.Warning && i.Message == expectedMessage1));

            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                    .Any(
                        i =>
                            i.Severity == ValidationSeverity.Error && i.Message == expectedMessage2));
        }

        [Test]
        public void GivenAWaveModel_WhenTimeStepIsNotAnIntegerAndNotADivisor_ThenAnErrorAndWarningShouldBeGiven()
        {
            var model = new WaveModel();
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.SimulationMode).SetValueAsString("non-stationary");
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.TimeScale).SetValueAsString("60");
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.TimeStep).SetValueAsString("9.9");

            var expectedMessage1 = Resources.WavePropertiesValidator_ValidateTimeStep;
            var expectedMessage2 = Resources.WavePropertiesValidator_ValidateDivisor;

            var validationReport = WavePropertiesValidator.Validate(model);

            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                    .Any(
                        i =>
                            i.Severity == ValidationSeverity.Warning && i.Message == expectedMessage1));

            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                    .Any(
                        i =>
                            i.Severity == ValidationSeverity.Error && i.Message == expectedMessage2));
        }

        [Test]
        public void GivenAWaveModel_WhenTimeStepIsBiggerThanTScale_ThenAnErrorShouldBeGiven()
        {
            var model = new WaveModel();
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.SimulationMode).SetValueAsString("non-stationary");
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.TimeScale).SetValueAsString("60");
            model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.TimeStep).SetValueAsString("65");

            var expectedMessage1 = Resources.WavePropertiesValidator_ValidateThat_TimeStep_Is_Not_Bigger_Than_TimeScale;

            var validationReport = WavePropertiesValidator.Validate(model);

            Assert.IsTrue(
                validationReport.GetAllIssuesRecursive()
                    .Any(
                        i =>
                            i.Severity == ValidationSeverity.Error && i.Message == expectedMessage1));
        }
    }
}