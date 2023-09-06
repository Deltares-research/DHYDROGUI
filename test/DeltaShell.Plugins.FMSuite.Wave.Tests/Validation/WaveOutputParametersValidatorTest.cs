using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WaveOutputParametersValidatorTest
    {
        [Test]
        public void GivenWaveModelWithoutObservationPointsAndWriteTableTrue_WhenValidatingOutputParameters_ThenErrorValidationIssueIsReturned()
        {
            // Given
            var waveModel = new WaveModel();
            waveModel.ModelDefinition.GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.WriteTable).Value = true;

            // When
            ValidationReport validationReport = WaveOutputParametersValidator.Validate(waveModel);

            // Then
            Assert.That(validationReport.Category, Is.EqualTo("Output parameters"));

            IList<ValidationIssue> validationIssues = validationReport.GetAllIssuesRecursive();
            Assert.That(validationIssues.Count, Is.EqualTo(1));

            ValidationIssue validationIssue = validationIssues.FirstOrDefault();
            Assert.IsNotNull(validationIssue);
            Assert.That(validationIssue.Severity, Is.EqualTo(ValidationSeverity.Warning));
            string expectedMessage = Resources.WaveOutputParametersValidator_Validate_Option__Write_Tables__is_selected_but_there_are_no_Observation_Points_in_your_model_;
            Assert.That(validationIssue.Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void GivenAWaveModelWithUseSinglePrecisionAndWriteNetCDF_WhenValidatingOutputParameters_ThenTheCorrectValidationIssueIsReturned()
        {
            // Given
            var waveModel = new WaveModel();
            waveModel.ModelDefinition.GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.MapWriteNetCDF).Value = true;
            waveModel.ModelDefinition.GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.NetCdfSinglePrecision).Value = true;
            
            // When
            ValidationReport validationReport = WaveOutputParametersValidator.Validate(waveModel);

            // Then
            Assert.That(validationReport.Category, Is.EqualTo("Output parameters"));

            IList<ValidationIssue> validationIssues = validationReport.GetAllIssuesRecursive();
            Assert.That(validationIssues.Count, Is.EqualTo(1));

            ValidationIssue validationIssue = validationIssues.FirstOrDefault();
            Assert.IsNotNull(validationIssue);
            Assert.That(validationIssue.Severity, Is.EqualTo(ValidationSeverity.Warning));
            string expectedMessage = Resources.WaveOutputParametersValidator_Validate_Enabling__Use_NetCDF_single_precision__might_lead_to_unexpected_behavior_when_inspecting_the_NetCDF_model_output_data_;
            Assert.That(validationIssue.Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void GivenAWaveModelWithoutUseSinglePrecisionAndWriteNetCDF_WhenValidatingOutputParameters_ThenTheCorrectValidationIssueIsReturned(bool writeMapNetCDF, bool useSinglePrecision)
        {
            // Given
            var waveModel = new WaveModel();
            waveModel.ModelDefinition.GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.MapWriteNetCDF).Value = writeMapNetCDF;
            waveModel.ModelDefinition.GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.NetCdfSinglePrecision).Value = useSinglePrecision;
            
            // When
            ValidationReport validationReport = WaveOutputParametersValidator.Validate(waveModel);

            // Then
            Assert.That(validationReport.Category, Is.EqualTo("Output parameters"));

            IList<ValidationIssue> validationIssues = validationReport.GetAllIssuesRecursive();
            Assert.That(validationIssues, Is.Empty);
        }
    }
}