using System.IO;
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
        public void GivenAnWaveModel_WhenTScaleIsNotAnIntegerAndTimeStepNotADivisor_ThenAnErrorAndWarningShouldBeGiven()
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
        public void GivenAnWaveModel_WhenTimeStepIsNotAnIntegerAndNotADivisor_ThenAnErrorAndWarningShouldBeGiven()
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
        public void GivenAnWaveModel_WhenTimeStepIsBiggerThanTScale_ThenAnErrorShouldBeGiven()
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