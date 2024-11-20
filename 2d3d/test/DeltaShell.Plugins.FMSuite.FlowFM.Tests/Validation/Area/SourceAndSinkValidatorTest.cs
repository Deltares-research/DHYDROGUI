using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation.Area
{
    [TestFixture]
    public class SourceAndSinkValidatorTest
    {
        [Test]
        public void GivenSourceAndSinkThatDoesNotSnapToGrid_WhenValidating_ThenValidationWarningIsReturned()
        {
            // Given
            var envelope = new Envelope(0, 4, 0, 4);
            var sourceAndSink = new SourceAndSink
            {
                Name = "mySourceAndSink",
                // Thin dam geometry is far outside of grid extent
                Feature = new Feature2D
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(10, 10),
                        new Coordinate(20, 20)
                    })
                }
            };

            // When
            var sourceAndSinks = new List<SourceAndSink> {sourceAndSink};
            IEnumerable<ValidationIssue> validationIssues = sourceAndSinks.Validate(envelope, new DateTime(), new DateTime());

            // Then
            ValidationIssue[] validationWarnings = validationIssues.Where(issue => issue.Severity == ValidationSeverity.Warning).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(1));

            string expectedMessage =
                string.Format(Resources.SourceAndSinkValidator_Validate_source_sink___0___not_within_grid_extent, sourceAndSink.Name);
            Assert.That(validationWarnings[0].Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void GivenSourceAndSinkWithoutFunctionValuesDefined_WhenValidating_ThenValidationErrorIsReturned()
        {
            // Given
            var sourceAndSink = new SourceAndSink
            {
                Name = "mySourceAndSink",
                Feature = new Feature2D
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 1)
                    })
                }
            };

            // When
            var sourceAndSinks = new List<SourceAndSink> {sourceAndSink};
            IEnumerable<ValidationIssue> validationIssues = sourceAndSinks.Validate(null, new DateTime(), new DateTime());

            // Then
            ValidationIssue[] validationErrors = validationIssues.Where(issue => issue.Severity == ValidationSeverity.Error).ToArray();
            Assert.That(validationErrors.Length, Is.EqualTo(1));

            string expectedMessage =
                string.Format(Resources.SourceAndSinkValidator_Validate_source_sink___0____discharge_time_series_does_not_contain_any_values_, sourceAndSink.Name);
            Assert.That(validationErrors[0].Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void GivenSourceAndSinkWithCorrectFunctionTimeArgumentValues_WhenValidating_ThenNoValidationErrorsAreReturned()
        {
            // Given
            DateTime dateTimeNow = DateTime.Now;
            DateTime startTime = dateTimeNow.AddDays(2);
            DateTime stopTime = dateTimeNow.AddDays(5);

            var sourceAndSink = new SourceAndSink
            {
                Name = "mySourceAndSink",
                Feature = new Feature2D
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 1)
                    })
                }
            };
            sourceAndSink.Function.Arguments[0].SetValues(new List<DateTime>
            {
                dateTimeNow,
                dateTimeNow.AddDays(6)
            });

            // When
            var sourceAndSinks = new List<SourceAndSink> {sourceAndSink};
            IEnumerable<ValidationIssue> validationIssues = sourceAndSinks.Validate(null, startTime, stopTime);

            // Then
            ValidationIssue[] validationErrors = validationIssues.Where(issue => issue.Severity == ValidationSeverity.Error).ToArray();
            Assert.IsEmpty(validationErrors);
        }

        [TestCase(0, 1, TestName = "SourceSinkIntervalBeforeModelInterval")]
        [TestCase(1, 3, TestName = "SourceSinkStopTimeBeforeModelStopTime")]
        [TestCase(3, 4, TestName = "SourceSinkIntervalWithinModelInterval")]
        [TestCase(4, 6, TestName = "SourceSinkStartTimeAfterModelStartTime")]
        [TestCase(6, 7, TestName = "SourceSinkIntervalAfterModelInterval")]
        public void GivenSourceAndSinkWithFunctionTimeArgumentValuesDefinedBeforeModelStartTime_WhenValidating_ThenValidationErrorIsReturned
            (int addedDaysToStartTime, int addedDaysToStopTime)
        {
            // Given
            DateTime dateTimeNow = DateTime.Now;

            DateTime startTime = dateTimeNow.AddDays(2);
            DateTime stopTime = dateTimeNow.AddDays(5);

            var sourceAndSink = new SourceAndSink
            {
                Name = "mySourceAndSink",
                Feature = new Feature2D
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 1)
                    })
                }
            };
            sourceAndSink.Function.Arguments[0].SetValues(new List<DateTime>
            {
                dateTimeNow.AddDays(addedDaysToStartTime),
                dateTimeNow.AddDays(addedDaysToStopTime)
            });

            // When
            var sourceAndSinks = new List<SourceAndSink> {sourceAndSink};
            IEnumerable<ValidationIssue> validationIssues = sourceAndSinks.Validate(null, startTime, stopTime);

            // Then
            ValidationIssue[] validationErrors = validationIssues.Where(issue => issue.Severity == ValidationSeverity.Error).ToArray();
            Assert.That(validationErrors.Length, Is.EqualTo(1));

            string expectedMessage =
                string.Format(Resources.SourceAndSinkValidator_Validate_source_sink___0____discharge_time_series_does_not_span_the_model_run_interval_, sourceAndSink.Name);
            Assert.That(validationErrors[0].Message, Is.EqualTo(expectedMessage));
        }
    }
}