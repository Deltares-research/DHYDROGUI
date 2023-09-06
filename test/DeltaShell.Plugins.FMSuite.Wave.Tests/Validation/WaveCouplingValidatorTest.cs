using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WaveCouplingValidatorTest
    {
        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public void Given_WaveModelCoupledToFlowAnd_Invalid_TimeStep_When_ValidatingCoupling_Then_ValidationErrorIsReturnedWithExpectedViewData(int timeStep)
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = true,
                StartTime = DateTime.Now,
                TimeStep = new TimeSpan(0, 0, timeStep)
            };
            var expectedTabName = "General";
            const string expectedMessage = "The coupling time step must be positive.";

            // When
            ValidationReport validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            ValidationIssue validationError = validationReport.AllErrors.FirstOrDefault(issue => issue.Message == expectedMessage);
            Assert.That(validationError, Is.Not.Null, "No validation error was generated.");

            var waveValidationShortcut = validationError.ViewData as WaveValidationShortcut;
            Assert.That(waveValidationShortcut, Is.Not.Null, "No WaveValidation Shortcut found.");
            Assert.That(waveValidationShortcut.WaveModel, Is.EqualTo(waveModel), "Shortcut wave model not as expected.");
            Assert.That(waveValidationShortcut.TabName, Is.EqualTo("General"), $"Expected shortcut tab name {expectedTabName}, but got {waveValidationShortcut.TabName}");
        }

        [Test]
        public void Given_WaveModelCoupledToFlowAnd_Valid_TimeStep_When_ValidatingCoupling_Then_ValidationErrorIsReturnedWithExpectedViewData()
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = true,
                StartTime = DateTime.Now,
                TimeStep = new TimeSpan(0, 0, 1)
            };
            const string expectedMessage = "The coupling time step must be positive.";

            // When
            ValidationReport validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            ValidationIssue validationError = validationReport.AllErrors.FirstOrDefault(issue => issue.Message == expectedMessage);
            Assert.That(validationError, Is.Null, "Validation error was generated but it was not expected.");
        }

        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public void GivenWaveModelCoupledToFlowAndInvalidCouplingPeriod_WhenValidatingCoupling_ThenValidationErrorIsReturnedWithExpectedViewData(int couplingPeriod)
        {
            using (var waveModel = new WaveModel())
            {
                // Given
                waveModel.IsCoupledToFlow = true;
                waveModel.StartTime = DateTime.Now;
                waveModel.StopTime = waveModel.StartTime.AddDays(couplingPeriod);

                const string expectedMessage = "The coupling period must be positive.";
                const string expectedTabName = "General";

                // When
                ValidationReport validationReport = WaveCouplingValidator.Validate(waveModel);

                // Then
                ValidationIssue validationError = validationReport.AllErrors.FirstOrDefault(issue => issue.Message == expectedMessage);
                Assert.IsNotNull(validationError);

                var waveValidationShortcut = validationError.ViewData as WaveValidationShortcut;
                Assert.IsNotNull(waveValidationShortcut);
                Assert.That(waveValidationShortcut.WaveModel, Is.EqualTo(waveModel));
                Assert.That(waveValidationShortcut.TabName, Is.EqualTo(expectedTabName));
            }
        }

        [Test]
        public void GivenWaveModelCoupledToFlowAndValidCouplingPeriod_WhenValidatingCoupling_ThenValidationErrorIsReturnedWithExpectedViewData()
        {
            using (var waveModel = new WaveModel())
            {
                // Given
                waveModel.IsCoupledToFlow = true;
                waveModel.StartTime = DateTime.Now;
                waveModel.StopTime = DateTime.Now.AddDays(1);

                const string expectedMessage = "The coupling period must be positive.";

                // When
                ValidationReport validationReport = WaveCouplingValidator.Validate(waveModel);

                // Then
                ValidationIssue validationError = validationReport.AllErrors.FirstOrDefault(issue => issue.Message == expectedMessage);
                Assert.That(validationError, Is.Null, "Validation error was generated but it was not expected.");
            }
        }

        [Test]
        public void GivenWaveModelCoupledToFlowAndReferenceTimePrecedingStartTime_WhenValidatingCoupling_ThenValidationErrorIsReturnedWithExpectedViewData()
        {
            // Given
            var waveModel = new WaveModel
            {
                IsCoupledToFlow = true,
                StartTime = new DateTime(2023,6,30,14,16,42)
            };
            waveModel.ModelDefinition.ModelReferenceDateTime = waveModel.StartTime.AddDays(1).Date; // Model start time precedes model reference time

            // When
            ValidationReport validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            const string expectedMessage = "Model start time precedes reference time";
            ValidationIssue validationError = validationReport.AllErrors.FirstOrDefault(issue => issue.Message == expectedMessage);
            Assert.IsNotNull(validationError);

            var waveValidationShortcut = validationError.ViewData as WaveValidationShortcut;
            Assert.IsNotNull(waveValidationShortcut);
            Assert.That(waveValidationShortcut.WaveModel, Is.EqualTo(waveModel));
            Assert.That(waveValidationShortcut.TabName, Is.EqualTo("General"));
        }

        [Test]
        public void GivenWaveModelNotCoupledToFlowModelAndWriteComFileIsTrue_WhenValidatingCoupling_ThenValidationErrorIsReturned()
        {
            // Given
            var waveModel = new WaveModel();

            waveModel.ModelDefinition.GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.WriteCOM).Value = true;

            // When
            ValidationReport validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            ContainsCouplingValidationErrorWithMessage(validationReport, "Stand-alone wave model cannot write COM-file");
        }

        [Test]
        public void Validate_WaveModelWithNonExistingCommunicationFile_ThenValidationErrorIsReturned()
        {
            // Arrange
            const string nonExistingFilePath = "C:/NonExistingDirectory/NonExistingFile_com.nc";

            using (var waveModel = new WaveModel())
            {
                waveModel.ModelDefinition.CommunicationsFilePath = nonExistingFilePath;

                // Act
                ValidationReport validationReport = WaveCouplingValidator.Validate(waveModel);

                // Assert
                var expectedErrorMessage = $"Communications file '{nonExistingFilePath}' does not exist.";
                ContainsCouplingValidationErrorWithMessage(validationReport, expectedErrorMessage);
            }
        }

        [Test]
        public void Validate_WaveModelWithExistingRelativeCommunicationFilePath_ThenNoValidationErrorIsReturned()
        {
            // Arrange
            using (var temporaryDirectory = new TemporaryDirectory())
            using (var waveModel = new WaveModel())
            {
                string mdwFilePath = Path.Combine(temporaryDirectory.Path, "WaveModelDirectory", "myModel.mdw");
                waveModel.ModelSaveTo(mdwFilePath, true);

                string communicationFilePath = Path.Combine(temporaryDirectory.Path, "myComFile_com.nc");
                FileStream fileStream = File.Create(communicationFilePath);
                fileStream.Close();

                waveModel.ModelDefinition.CommunicationsFilePath = "../myComFile_com.nc";

                // Act
                ValidationReport validationReport = WaveCouplingValidator.Validate(waveModel);

                // Assert
                IEnumerable<ValidationIssue> validationErrors = validationReport.AllErrors.Where(issue => issue.Message.StartsWith("Communications file '"));
                Assert.IsEmpty(validationErrors);
            }
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
            waveModel.ModelDefinition.GetModelProperty(KnownWaveSections.OutputSection, KnownWaveProperties.COMFile).Value = comFilePath;

            // When
            ValidationReport validationReport = WaveCouplingValidator.Validate(waveModel);

            // Then
            ContainsCouplingValidationErrorWithMessage(validationReport, "Coupled wave model must use COM-file");
        }

        [TestCase("")]
        [TestCase(null)]
        public void Validate_WaveModelWithNullOrEmptyCommunicationFilePath_ThenNoValidationErrorIsReturned(string communicationFilePath)
        {
            // Arrange
            using (var waveModel = new WaveModel())
            {
                waveModel.ModelDefinition.CommunicationsFilePath = communicationFilePath;

                // Act
                ValidationReport validationReport = WaveCouplingValidator.Validate(waveModel);

                // Assert
                IEnumerable<ValidationIssue> validationErrors = validationReport.AllErrors.Where(issue => issue.Message.StartsWith("Communications file '"));
                Assert.IsEmpty(validationErrors);
            }
        }

        private static void ContainsCouplingValidationErrorWithMessage(ValidationReport validationReport, string expectedMessage)
        {
            ValidationIssue validationError = validationReport.AllErrors.Single(issue => issue.Message == expectedMessage);
            Assert.That(validationError.Subject, Is.EqualTo("Coupling"));
        }
    }
}