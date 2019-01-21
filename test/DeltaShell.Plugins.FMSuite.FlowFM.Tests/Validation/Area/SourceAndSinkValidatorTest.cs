using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMapTestUtils;

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
                    Geometry = new LineString(new[] { new Coordinate(10, 10), new Coordinate(20, 20) })
                }
            };

            // When
            var validationIssues = SourceAndSinkValidator.Validate(new List<SourceAndSink> { sourceAndSink }, envelope, new DateTime(), new DateTime());

            // Then
            var validationWarnings = validationIssues.Where(issue => issue.Severity == ValidationSeverity.Warning).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(1));

            var expectedMessage =
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
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 1) })
                }
            };

            // When
            var validationIssues = SourceAndSinkValidator.Validate(new List<SourceAndSink>{sourceAndSink}, null, new DateTime(), new DateTime());

            // Then
            var validationErrors = validationIssues.Where(issue => issue.Severity == ValidationSeverity.Error).ToArray();
            Assert.That(validationErrors.Length, Is.EqualTo(1));

            var expectedMessage =
                string.Format(Resources.SourceAndSinkValidator_Validate_source_sink___0____discharge_time_series_does_not_contain_any_values_, sourceAndSink.Name);
            Assert.That(validationErrors[0].Message, Is.EqualTo(expectedMessage));
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
            var dateTimeNow = DateTime.Now;

            var startTime = dateTimeNow.AddDays(2);
            var stopTime = dateTimeNow.AddDays(5);
            
            var sourceAndSink = new SourceAndSink
            {
                Name = "mySourceAndSink",
                Feature = new Feature2D
                {
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 1) })
                }
            };
            sourceAndSink.Function.Arguments[0].SetValues(new List<DateTime> { dateTimeNow.AddDays(addedDaysToStartTime), dateTimeNow.AddDays(addedDaysToStopTime) });

            // When
            var validationIssues = SourceAndSinkValidator.Validate(new List<SourceAndSink> { sourceAndSink }, null, startTime, stopTime);

            // Then
            var validationErrors = validationIssues.Where(issue => issue.Severity == ValidationSeverity.Error).ToArray();
            Assert.That(validationErrors.Length, Is.EqualTo(1));

            var expectedMessage =
                string.Format(Resources.SourceAndSinkValidator_Validate_source_sink___0____discharge_time_series_does_not_span_the_model_run_interval_, sourceAndSink.Name);
            Assert.That(validationErrors[0].Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void GivenSourceAndSinkWithCorrectFunctionTimeArgumentValues_WhenValidating_ThenNoValidationErrorsAreReturned()
        {
            // Given
            var dateTimeNow = DateTime.Now;
            var startTime = dateTimeNow.AddDays(2);
            var stopTime = dateTimeNow.AddDays(5);

            var sourceAndSink = new SourceAndSink
            {
                Name = "mySourceAndSink",
                Feature = new Feature2D
                {
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 1) })
                }
            };
            sourceAndSink.Function.Arguments[0].SetValues(new List<DateTime> { dateTimeNow, dateTimeNow.AddDays(6) });

            // When
            var validationIssues = SourceAndSinkValidator.Validate(new List<SourceAndSink> { sourceAndSink }, null, startTime, stopTime);

            // Then
            var validationErrors = validationIssues.Where(issue => issue.Severity == ValidationSeverity.Error).ToArray();
            Assert.IsEmpty(validationErrors);
        }
    }
}