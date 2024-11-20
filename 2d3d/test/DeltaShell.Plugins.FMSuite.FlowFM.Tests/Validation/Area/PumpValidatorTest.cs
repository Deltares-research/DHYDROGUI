using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation.Area
{
    [TestFixture]
    public class PumpValidatorTest
    {
        [Test]
        public void GivenValidPump_WhenValidating_ThenNoValidationWarningsAreReturned()
        {
            // Given
            var envelope = new Envelope(0, 4, 0, 4);
            var pump = new Pump
            {
                Name = "myPump",
                Geometry = new LineString(new[]
                {
                    new Coordinate(1, 1),
                    new Coordinate(2, 2)
                })
            };
            var pumps = new List<IPump> {pump};

            // When
            IEnumerable<ValidationIssue> validationIssues = pumps.Validate(envelope, DateTime.Now, DateTime.Now);

            // Then
            ValidationIssue[] validationWarnings =
                validationIssues.Where(issue => issue.Severity == ValidationSeverity.Warning).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(0));
        }

        [Test]
        public void GivenPumpThatDoesNotIntersectWithGridExtent_WhenValidating_ThenValidationWarningIsReturned()
        {
            // Given
            var envelope = new Envelope(0, 4, 0, 4);
            var pump = new Pump
            {
                Name = "myPump",
                // Pump geometry is far outside of grid extent
                Geometry = new LineString(new[]
                {
                    new Coordinate(10, 10),
                    new Coordinate(20, 20)
                })
            };
            var pumps = new List<IPump> {pump};

            // When
            IEnumerable<ValidationIssue> validationIssues = pumps.Validate(envelope, DateTime.Now, DateTime.Now);

            // Then
            ValidationIssue[] validationWarnings =
                validationIssues.Where(issue => issue.Severity == ValidationSeverity.Warning).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(1));

            var expectedMessage = $"pump '{pump.Name}' not within grid extent";
            Assert.That(validationWarnings[0].Message, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void GivenPumpWithNegativeCapacity_WhenValidating_ThenValidationErrorIsReturned()
        {
            // Given
            var envelope = new Envelope(0, 4, 0, 4);
            var pump = new Pump
            {
                Name = "myPump",
                Geometry = new LineString(new[]
                {
                    new Coordinate(1, 1),
                    new Coordinate(2, 2)
                }),
                Capacity = -2.0
            };
            var pumps = new List<IPump> {pump};

            // When
            IEnumerable<ValidationIssue> validationIssues = pumps.Validate(envelope, DateTime.Now, DateTime.Now);

            // Then
            ValidationIssue[] validationWarnings =
                validationIssues.Where(issue => issue.Severity == ValidationSeverity.Error).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(1));

            var expectedMessage =
                $"pump '{pump.Name}': Capacity must be greater than or equal to 0.";
            Assert.That(validationWarnings[0].Message, Is.EqualTo(expectedMessage));
        }
    }
}