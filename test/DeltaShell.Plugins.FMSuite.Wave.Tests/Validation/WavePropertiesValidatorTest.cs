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
        /* Units tests created after DELFT3DFM-510 */
        [TestCase(0.0, new double[]{0.0}, false, InputFieldDataType.Constant, false, TestName = "ConstantWindSpeedZero_TimeseriesZero_QuadrupletsFalse_ForConstantWindCase_NoWarning")]
        [TestCase(0.0, new double[]{0.0}, true, InputFieldDataType.Constant, true, TestName = "ConstantWindSpeedZero_TimeseriesZero_QuadrupletsTrue_ForConstantWindCase_WarningMustBeThere")]
        [TestCase(0.0, new double[]{30.0}, true, InputFieldDataType.Constant, true, TestName = "ConstantWindSpeedZero_TimeseriesGreaterThanZero_QuadrupletsTrue_ForConstantWindCase_WarningMustBeThere")]
        [TestCase(30.0, new double[]{0.0}, false, InputFieldDataType.Constant, false, TestName = "ConstantWindSpeedGreaterThanZero_TimeseriesZero_QuadrupletsFalse_ForConstantWindCase_NoWarning")]
        [TestCase(30.0, new double[]{0.0}, true, InputFieldDataType.Constant, false, TestName = "ConstantWindSpeedGreaterThanZero_TimeseriesZero_QuadrupletsTrue_ForConstantWindCase_NoWarning")]
        [TestCase(30.0, new double[] {30.0}, true, InputFieldDataType.Constant, false, TestName = "ConstantWindSpeedGreaterThanZero_TimeseriesGreaterThanZero_QuadrupletsTrue_ForConstantWindCase_NoWarning")]
        [TestCase(0.0, new double[]{30.0, 30.0}, false, InputFieldDataType.TimeVarying, false, TestName = "ConstantWindSpeedZero_TimeseriesGreaterThanZero_QuadrupletsFalse_ForTimeSeriesWindCase_NoWarning")]
        [TestCase(0.0, new double[]{30.0, 0.0}, false, InputFieldDataType.TimeVarying, false, TestName = "ConstantWindSpeedZero_TimeseriesNotAllGreaterThanZero_QuadrupletsFalse_ForTimeSeriesWindCase_NoWarning")]
        [TestCase(0.0, new double[]{0.0, 0.0}, false, InputFieldDataType.TimeVarying, false, TestName = "ConstantWindSpeedZero_TimeseriesZero_QuadrupletsFalse_ForTimeSeriesWindCase_NoWarning")]
        [TestCase(0.0, new double[]{30.0, 30.0}, true, InputFieldDataType.TimeVarying, false, TestName = "ConstantWindSpeedZero_TimeseriesGreaterThanZero_QuadrupletsTrue_ForTimeSeriesWindCase_NoWarning")]
        [TestCase(0.0, new double[]{30.0, 0.0}, true, InputFieldDataType.TimeVarying, true, TestName = "ConstantWindSpeedZero_TimeseriesNotAllGreaterThanZero_QuadrupletsTrue_ForTimeSeriesWindCase_WarningMustBeThere")]
        [TestCase(30.0, new double[]{30.0, 0.0}, true, InputFieldDataType.TimeVarying, true, TestName = "ConstantWindSpeedGreaterThanZero_TimeseriesNotAllGreaterThanZero_QuadrupletsTrue_ForTimeSeriesWindCase_WarningMustBeThere")]
        [TestCase(0.0, new double[]{0.0, 0.0}, true, InputFieldDataType.TimeVarying, true, TestName = "ConstantWindSpeedZero_TimeseriesZero_QuadrupletsTrue_ForTimeSeriesWindCase_WarningMustBeThere")]
        [TestCase(30.0, new double[]{0.0, 0.0}, true, InputFieldDataType.TimeVarying, true, TestName = "ConstantWindSpeedGreaterThanZero_TimeseriesZero_QuadrupletsTrue_ForTimeSeriesWindCase_WarningMustBeThere")]
        [Category(TestCategory.Integration)]
        public void CheckWavePropertiesWithFlowModel(double windSpeedConstant, double [] windSpeedTimeseries, bool quadruplets, InputFieldDataType windType, bool warningAlert)
        {
            var model = new WaveModel();
            var reportMessage = Resources.WavePropertiesValidator_ValidateWindSpeedAndQuadruple_WindSpeed_is_zero_whereas_quadruple_is_true_;
            var reportSeverity = ValidationSeverity.Error;

            /* Assigning variables */
           
            model.TimePointData.WindSpeedConstant = windSpeedConstant;
            
            var timeSeriesWindSpeed = model.TimePointData.InputFields.Components.FirstOrDefault(c => c.Name == "Wind Speed");
            Assert.NotNull(timeSeriesWindSpeed);
            
            foreach (var wind in windSpeedTimeseries)
            {
               timeSeriesWindSpeed.Values.Add(wind);
            }
            
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