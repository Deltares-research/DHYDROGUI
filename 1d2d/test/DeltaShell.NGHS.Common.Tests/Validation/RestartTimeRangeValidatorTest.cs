using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.Common.Properties;
using DeltaShell.NGHS.Common.Validation;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Validation
{
    [TestFixture]
    public class RestartTimeRangeValidatorTest
    {
        [Test]
        public void ValidateWriteRestartSettings_WhenWriteRestartIsFalseAndAllSettingsAreInvalid_ShouldReturnEmptyValidationReport()
        {
            // Arrange
            DateTime modelStartTime = DateTime.Today.AddDays(6);
            var modelTimeStep = new TimeSpan(0, 12, 0, 0);

            DateTime restartStartTime = DateTime.Today.AddDays(2);
            DateTime restartStopTime = DateTime.Today.AddDays(1);
            var restartTimeStep = new TimeSpan(0, 13, 0, 0);

            // Act
            ValidationReport validationReport = RestartTimeRangeValidator.ValidateWriteRestartSettings(false, restartStartTime, restartStopTime, restartTimeStep,
                                                                                                       modelStartTime, modelTimeStep);
            // Assert
            Assert.IsNotNull(validationReport);
            Assert.IsTrue(validationReport.IsEmpty);
        }

        [Test]
        public void ValidateWriteRestartSettings_WhenWriteRestartIsTrueAndEverythingIsCorrect_ShouldReturnEmptyValidationReport()
        {
            // Arrange
            DateTime modelStartTime = DateTime.Today;
            var modelTimeStep = new TimeSpan(0, 12, 0, 0);

            DateTime restartStartTime = DateTime.Today;
            DateTime restartStopTime = DateTime.Today.AddDays(1);
            var restartTimeStep = new TimeSpan(0, 12, 0, 0);

            // Act
            ValidationReport validationReport = RestartTimeRangeValidator.ValidateWriteRestartSettings(true, restartStartTime, restartStopTime, restartTimeStep,
                                                                                                       modelStartTime, modelTimeStep);
            // Assert
            Assert.IsNotNull(validationReport);
            Assert.IsTrue(validationReport.IsEmpty);
        }

        [Test]
        public void ValidateWriteRestartSettings_WhenViewDataArgumentIsMissing_ShouldSetNullForViewDataInError()
        {
            // Arrange
            DateTime modelStartTime = DateTime.Today;
            var modelTimeStep = new TimeSpan(0, 12, 0, 0);

            DateTime restartStartTime = DateTime.Today;
            DateTime restartStopTime = DateTime.Today.AddDays(1);

            // Incorrect restartTimeStep, so that validation issue will be created.
            var restartTimeStep = new TimeSpan(0, 0, 0, 0);

            // Act
            ValidationReport validationReport = RestartTimeRangeValidator.ValidateWriteRestartSettings(true, restartStartTime, restartStopTime, restartTimeStep,
                                                                                                       modelStartTime, modelTimeStep);
            // Assert
            Assert.AreEqual(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_time_step,
                            validationReport.GetAllIssuesRecursive().First().ViewData);
        }

        [Test]
        public void ValidateWriteRestartSettings_WhenWriteRestartIsTrueAndRestartTimeStepIsZero_ShouldReturnError()
        {
            // Arrange
            DateTime modelStartTime = DateTime.Today;
            var modelTimeStep = new TimeSpan(0, 12, 0, 0);

            DateTime restartStartTime = DateTime.Today;
            DateTime restartStopTime = DateTime.Today.AddDays(1);
            var restartTimeStep = new TimeSpan(0, 0, 0, 0);

            const string viewData = "test";

            // Act
            ValidationReport validationReport = RestartTimeRangeValidator.ValidateWriteRestartSettings(true, restartStartTime, restartStopTime, restartTimeStep,
                                                                                                       modelStartTime, modelTimeStep, viewData);
            // Assert
            AssertExpectedErrorInValidationReport(validationReport,
                                                  Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_time_step_must_be_positive_value_,
                                                  Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_time_step,
                                                  viewData);
        }

        [Test]
        public void ValidateWriteRestartSettings_WhenWriteRestartIsTrueAndRestartStopTimeIsBeforeRestartStartTime_ShouldReturnError()
        {
            // Arrange
            DateTime modelStartTime = DateTime.Today;
            var modelTimeStep = new TimeSpan(0, 12, 0, 0);

            DateTime restartStartTime = DateTime.Today.AddDays(1);
            DateTime restartStopTime = DateTime.Today;
            var restartTimeStep = new TimeSpan(0, 12, 0, 0);

            const string viewData = "test";

            // Act
            ValidationReport validationReport = RestartTimeRangeValidator.ValidateWriteRestartSettings(true, restartStartTime, restartStopTime, restartTimeStep,
                                                                                                       modelStartTime, modelTimeStep, viewData);
            // Assert
            AssertExpectedErrorInValidationReport(validationReport,
                                                  Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_stop_time_cannot_be_before_restart_start_time_,
                                                  Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_interval,
                                                  viewData);
        }

        [Test]
        public void ValidateWriteRestartSettings_WhenWriteRestartIsTrueAndRestartTimeStepIsNotAMultipleOfModelTimeStep_ShouldReturnError()
        {
            // Arrange
            DateTime modelStartTime = DateTime.Today;
            var modelTimeStep = new TimeSpan(0, 12, 0, 0);

            DateTime restartStartTime = DateTime.Today;
            DateTime restartStopTime = DateTime.Today.AddDays(1);
            var restartTimeStep = new TimeSpan(0, 13, 0, 0);

            const string viewData = "test";

            // Act
            ValidationReport validationReport = RestartTimeRangeValidator.ValidateWriteRestartSettings(true, restartStartTime, restartStopTime, restartTimeStep,
                                                                                                       modelStartTime, modelTimeStep, viewData);
            // Assert
            AssertExpectedErrorInValidationReport(validationReport,
                                                  Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_time_step_must_be_an_integer_multiple_of_the_output_time_step_,
                                                  Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_time_step,
                                                  viewData);
        }

        [Test]
        public void ValidateWriteRestartSettings_WhenWriteRestartIsTrueAndRestartStartTimeIsBeforeModelStartTime_ShouldReturnError()
        {
            // Arrange
            DateTime modelStartTime = DateTime.Today;
            var modelTimeStep = new TimeSpan(0, 12, 0, 0);

            DateTime restartStartTime = DateTime.Today.Subtract(TimeSpan.FromDays(1));
            DateTime restartStopTime = DateTime.Today.AddDays(1);
            var restartTimeStep = new TimeSpan(0, 12, 0, 0);

            const string viewData = "test";

            // Act
            ValidationReport validationReport = RestartTimeRangeValidator.ValidateWriteRestartSettings(true, restartStartTime, restartStopTime, restartTimeStep,
                                                                                                       modelStartTime, modelTimeStep, viewData);
            // Assert
            AssertExpectedErrorInValidationReport(validationReport,
                                                  Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_start_time_must_be_expressed_by_model_start_time_plus_a_positive_integer_multiple_of_the_model_time_step_,
                                                  Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_start_time,
                                                  viewData);
        }

        [Test]
        public void ValidateWriteRestartSettings_WhenWriteRestartIsTrueAndRestartStartTimeIsNotModelStartTimePlusAMultipleOfModelTimeStep_ShouldReturnError()
        {
            // Arrange
            DateTime modelStartTime = DateTime.Today;
            var modelTimeStep = new TimeSpan(0, 12, 0, 0);

            DateTime restartStartTime = DateTime.Today.AddHours(13);
            DateTime restartStopTime = DateTime.Today.AddDays(1);
            var restartTimeStep = new TimeSpan(0, 12, 0, 0);

            const string viewData = "test";

            // Act
            ValidationReport validationReport = RestartTimeRangeValidator.ValidateWriteRestartSettings(true, restartStartTime, restartStopTime, restartTimeStep,
                                                                                                       modelStartTime, modelTimeStep, viewData);
            // Assert
            AssertExpectedErrorInValidationReport(validationReport,
                                                  Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_start_time_must_be_expressed_by_model_start_time_plus_a_positive_integer_multiple_of_the_model_time_step_,
                                                  Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_start_time,
                                                  viewData);
        }

        [Test]
        public void ValidateWriteRestartSettings_WhenWriteRestartIsTrueAndRestartStartAndStopTimesAreBeforeModelStartTime_ShouldReturn2Errors()
        {
            // Arrange
            DateTime modelStartTime = DateTime.Today.AddDays(1);
            var modelTimeStep = new TimeSpan(0, 12, 0, 0);

            DateTime restartStartTime = DateTime.Today;
            DateTime restartStopTime = DateTime.Today;
            var restartTimeStep = new TimeSpan(0, 12, 0, 0);

            // Act
            ValidationReport validationReport = RestartTimeRangeValidator.ValidateWriteRestartSettings(true, restartStartTime, restartStopTime, restartTimeStep,
                                                                                                       modelStartTime, modelTimeStep);
            // Assert
            AssertIfStartAndStopTimesAreBothOutsideModelTimeRange(validationReport);
        }

        [Test]
        public void ValidateWriteRestartSettings_WhenWriteRestartIsTrueAndRestartStopTimeIsNotRestartStartTimePlusAMultipleOfModelTimeStep_ShouldReturnError()
        {
            // Arrange
            DateTime modelStartTime = DateTime.Today;
            var modelTimeStep = new TimeSpan(0, 2, 0, 0);

            DateTime restartStartTime = DateTime.Today;
            DateTime restartStopTime = DateTime.Today.AddHours(15);
            var restartTimeStep = new TimeSpan(0, 6, 0, 0);

            const string viewData = "test";

            // Act
            ValidationReport validationReport = RestartTimeRangeValidator.ValidateWriteRestartSettings(true, restartStartTime, restartStopTime, restartTimeStep,
                                                                                                       modelStartTime, modelTimeStep, viewData);
            // Assert
            AssertExpectedErrorInValidationReport(validationReport,
                                                  Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_stop_time_must_be_expressed_by_model_start_time_plus_a_positive_integer_multiple_of_the_model_time_step_,
                                                  Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_stop_time,
                                                  viewData);
        }

        private static void AssertExpectedErrorInValidationReport(ValidationReport validationReport, string expectedMessage, string expectedSubject, object viewData)
        {
            ValidationIssue issue = RetrieveIssue(validationReport);

            Assert.AreEqual(expectedMessage, issue.Message);
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity);
            Assert.AreEqual(expectedSubject, issue.Subject);
            Assert.AreSame(viewData, issue.ViewData);
        }

        private static ValidationIssue RetrieveIssue(ValidationReport validationReport)
        {
            Assert.IsNotNull(validationReport);
            IList<ValidationIssue> allIssues = validationReport.GetAllIssuesRecursive();
            Assert.AreEqual(1, allIssues.Count);
            ValidationIssue issue = allIssues[0];
            return issue;
        }

        private static void AssertIfStartAndStopTimesAreBothOutsideModelTimeRange(ValidationReport validationReport)
        {
            Assert.IsNotNull(validationReport);
            IList<ValidationIssue> allIssues = validationReport.GetAllIssuesRecursive();
            Assert.AreEqual(2, allIssues.Count);

            ValidationIssue issue1 = allIssues[0];
            ValidationIssue issue2 = allIssues[1];

            Assert.AreEqual(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_start_time_must_be_expressed_by_model_start_time_plus_a_positive_integer_multiple_of_the_model_time_step_, issue1.Message);
            Assert.AreEqual(ValidationSeverity.Error, issue1.Severity);
            Assert.AreEqual(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_start_time, issue1.Subject);

            Assert.AreEqual(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_stop_time_must_be_expressed_by_model_start_time_plus_a_positive_integer_multiple_of_the_model_time_step_, issue2.Message);
            Assert.AreEqual(ValidationSeverity.Error, issue2.Severity);
            Assert.AreEqual(Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_stop_time, issue2.Subject);
        }
    }
}