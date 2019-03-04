using System;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WaveCouplingValidatorTest
    {
        [Test]
        public void GivenWaveModelCoupledToFlowAndReferenceTimePrecedingStartTime_WhenValidatingCoupling_ThenValidationErrorIsReturnedWithExpectedViewData()
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = true,
                StartTime = DateTime.Now
            };
            waveModel.ModelDefinition.ModelReferenceDateTime = waveModel.StartTime.AddDays(1); // Model start time precedes model reference time

            // When
            var validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            var expectedMessage = Resources.WaveTimePointValidator_Validate_Model_start_time_precedes_reference_time;
            var validationError = validationReport.AllErrors.FirstOrDefault(issue => issue.Message == expectedMessage);
            Assert.IsNotNull(validationError);

            var waveValidationShortcut = validationError.ViewData as WaveValidationShortcut;
            Assert.IsNotNull(waveValidationShortcut);
            Assert.That(waveValidationShortcut.WaveModel, Is.EqualTo(waveModel));
            Assert.That(waveValidationShortcut.TabName, Is.EqualTo("General"));
        }

        [TestCase(false, "anyPath")]
        [TestCase(true, "")]
        [TestCase(true, null)]
        public void GivenWaveModelCoupledToFlowAndWriteComFileIsTrueAndComFilePathIsNullOrEmpty_WhenValidatingCoupling_ThenValidationErrorIsReturned
            (bool writeComFile, string comFilePath)
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = true,
                WriteCOM = writeComFile
            };
            waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.COMFile).Value = comFilePath;

            // When
            var validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            ContainsValidationErrorWithMessage(validationReport, Resources.WaveCouplingValidator_Validate_Coupled_wave_model_must_use_COM_file);
        }

        [Test]
        public void GivenWaveModelCoupledToFlowAndGetFlowComFilePathFunctionIsNull_WhenValidatingCoupling_ThenValidationErrorIsReturned()
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = true,
                WriteCOM = true
            };
            waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.COMFile).Value = "somePath";
            waveModel.GetFlowComFilePath = null;

            // When
            var validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            ContainsValidationErrorWithMessage(validationReport, Resources.WaveCouplingValidator_Validate_Coupled_wave_model_must_use_COM_file);
        }

        [Test]
        public void GivenWaveModelNotCoupledToFlowModelAndWriteComFileIsTrue_WhenValidatingCoupling_ThenValidationErrorIsReturned()
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = false
            };
            waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.WriteCOM).Value = true;

            // When
            var validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            ContainsValidationErrorWithMessage(validationReport, Resources.WaveCouplingValidator_Validate_Stand_alone_wave_model_cannot_use_COM_file);
        }

        [Test]
        public void GivenWaveModelNotCoupledToFlowModelAndWriteComFileIsTrueButComFilePathIsNotEmpty_WhenValidatingCoupling_ThenValidationErrorIsReturned()
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = false
            };
            waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.WriteCOM).Value = false;
            waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.COMFile).Value = "somePath";

            // When
            var validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            ContainsValidationErrorWithMessage(validationReport, Resources.WaveCouplingValidator_Validate_Stand_alone_wave_model_cannot_use_COM_file);
        }

        [TestCase(UsageFromFlowType.UseAndExtend)]
        [TestCase(UsageFromFlowType.UseDoNotExtend)]
        public void GivenWaveModelNotCoupledToFlowModelWithBedLevelUsageNotEqualToDoNotUse_WhenValidatingCoupling_ThenValidationErrorIsReturned(UsageFromFlowType usageType)
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = false
            };
            waveModel.OuterDomain.HydroFromFlowData.BedLevelUsage = usageType;

            // When
            var validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            var expectedMessage = string.Format(Resources.WaveCouplingValidator_Validate_Stand_alone_wave_model_cannot_use__0_, "flow bed level");
            ContainsValidationErrorWithMessage(validationReport, expectedMessage);
        }

        [TestCase(UsageFromFlowType.UseAndExtend)]
        [TestCase(UsageFromFlowType.UseDoNotExtend)]
        public void GivenWaveModelNotCoupledToFlowModelWithWaterLevelUsageNotEqualToDoNotUse_WhenValidatingCoupling_ThenValidationErrorIsReturned(UsageFromFlowType usageType)
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = false
            };
            waveModel.OuterDomain.HydroFromFlowData.WaterLevelUsage = usageType;

            // When
            var validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            var expectedMessage = string.Format(Resources.WaveCouplingValidator_Validate_Stand_alone_wave_model_cannot_use__0_, "flow water level");
            ContainsValidationErrorWithMessage(validationReport, expectedMessage);
        }

        [TestCase(UsageFromFlowType.UseAndExtend)]
        [TestCase(UsageFromFlowType.UseDoNotExtend)]
        public void GivenWaveModelNotCoupledToFlowModelWithVelocityUsageNotEqualToDoNotUse_WhenValidatingCoupling_ThenValidationErrorIsReturned(UsageFromFlowType usageType)
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = false
            };
            waveModel.OuterDomain.HydroFromFlowData.VelocityUsage = usageType;

            // When
            var validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            var expectedMessage = string.Format(Resources.WaveCouplingValidator_Validate_Stand_alone_wave_model_cannot_use__0_, "flow velocities");
            ContainsValidationErrorWithMessage(validationReport, expectedMessage);
        }

        [TestCase(UsageFromFlowType.UseAndExtend)]
        [TestCase(UsageFromFlowType.UseDoNotExtend)]
        public void GivenWaveModelNotCoupledToFlowModelWithWindUsageNotEqualToDoNotUse_WhenValidatingCoupling_ThenValidationErrorIsReturned(UsageFromFlowType usageType)
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = false
            };
            waveModel.OuterDomain.HydroFromFlowData.WindUsage = usageType;

            // When
            var validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            var expectedMessage = string.Format(Resources.WaveCouplingValidator_Validate_Stand_alone_wave_model_cannot_use__0_, "flow wind");
            ContainsValidationErrorWithMessage(validationReport, expectedMessage);
        }

        private static void ContainsValidationErrorWithMessage(ValidationReport validationReport, string expectedMessage)
        {
            var errorMessages = validationReport.AllErrors.Select(issue => issue.Message).ToArray();
            Assert.Contains(expectedMessage, errorMessages);
        }
    }
}