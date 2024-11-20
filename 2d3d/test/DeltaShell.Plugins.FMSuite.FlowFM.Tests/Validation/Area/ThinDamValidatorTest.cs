using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation.Area
{
    [TestFixture]
    public class ThinDamValidatorTest
    {
        [Test]
        public void GivenFmModelWithThinDamsThatDoNotIntersectWithModelGrid_WhenValidatingThinDams_ThenValidationWarningIsReturned()
        {
            // Given
            var envelope = new Envelope(0, 4, 0, 4);
            var thinDam = new ThinDam2D
            {
                Name = "myThinDam",
                // Thin dam geometry is far outside of grid extent
                Geometry = new LineString(new[]
                {
                    new Coordinate(10, 10),
                    new Coordinate(20, 20)
                })
            };

            // When
            var thinDams = new List<ThinDam2D> {thinDam};
            IEnumerable<ValidationIssue> validationIssues = thinDams.Validate(envelope);

            // Then
            ValidationIssue[] validationWarnings = validationIssues.Where(issue => issue.Severity == ValidationSeverity.Warning).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(1), "No validation warning was returned for grid snapping, while we expect to see one validation warning here.");

            string expectedMessage =
                string.Format(Resources.WaterFlowFMArea2DValidator_Validate_thin_dam___0___not_within_grid_extent, thinDam.Name);
            Assert.That(validationWarnings[0].Message, Is.EqualTo(expectedMessage), "The validation message was different than expected.");
        }
    }
}