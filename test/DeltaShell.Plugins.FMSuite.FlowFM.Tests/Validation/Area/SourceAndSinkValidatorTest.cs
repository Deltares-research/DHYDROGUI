using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
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
            var fmModel = new WaterFlowFMModel
            {
                Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2)
            };
            var sourceAndSink = new SourceAndSink
            {
                Name = "mySourceAndSink",
                // Thin dam geometry is far outside of grid extent
                Feature = new Feature2D
                {
                    Geometry = new LineString(new[] { new Coordinate(10, 10), new Coordinate(20, 20) })
                }
            };
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            // When
            var validationIssues = SourceAndSinkValidator.Validate(fmModel, fmModel.SourcesAndSinks);

            // Then
            var validationWarnings = validationIssues.Where(issue => issue.Severity == ValidationSeverity.Warning).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(1));

            var expectedMessage =
                string.Format(Resources.SourceAndSinkValidator_Validate_source_sink___0___not_within_grid_extent, sourceAndSink.Name);
            Assert.That(validationWarnings[0].Message, Is.EqualTo(expectedMessage));
        }
    }
}