using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation.Area
{
    [TestFixture]
    public class ThinDamValidatorTest
    {
        [Test]
        public void GivenFmModelWithThinDamsThatDoNotIntersectWithModelGrid_WhenValidatingThinDams_ThenValidationWarningIsReturned()
        {
            // Given
            var fmModel = new WaterFlowFMModel
            {
                Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2)
            };
            var thinDam = new ThinDam2D
            {
                Name = "myThinDam",
                // Thin dam geometry is far outside of grid extent
                Geometry = new LineString(new[] { new Coordinate(10, 10), new Coordinate(20, 20) })
            };
            fmModel.Area.ThinDams.Add(thinDam);

            // When
            var validationIssues = ThinDamValidator.Validate(fmModel);

            // Then
            var validationWarnings = validationIssues.Where(issue => issue.Severity == ValidationSeverity.Warning).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(1));

            var expectedMessage =
                string.Format(Resources.WaterFlowFMArea2DValidator_Validate_thin_dam___0___not_within_grid_extent, thinDam.Name);
            Assert.That(validationWarnings[0].Message, Is.EqualTo(expectedMessage));
        }
    }
}