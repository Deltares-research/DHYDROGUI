using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation.Area
{
    [TestFixture]
    public class FixedWeirValidatorTest
    {
        private const string MessageDifferentNumberValidationIssues = "Number of validation issues was different than expected when validating this fixed weir.";
        private const string MessageDifferentIssueMessage = "A different issue message for this issue was expected when validating this fixed weir.";
        private const string MessageDifferentValidationSeverity = "The severity of this validation issue was different than expected when validating this issue.should have been of type Warning.";

        private IEnumerable<FixedWeir> fixedWeirs;
        private FixedWeir fixedWeir;

        [SetUp]
        public void SetUp()
        {
            fixedWeir = new FixedWeir
            {
                Name = "fixed_weir",
                Geometry = new Point(new Coordinate(10, 10))
            };
            fixedWeirs = new List<FixedWeir> {fixedWeir};
        }

        [Test]
        public void GivenAFixedWeirWithAGeometryThatDoesNotSnapToGrid_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var gridExtent = new Envelope();

            // When
            var issues = fixedWeirs.Validate(gridExtent, new List<ModelFeatureCoordinateData<FixedWeir>>(), "").ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageDifferentNumberValidationIssues);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Warning, issue.Severity, MessageDifferentValidationSeverity);
            var expectedMessage = string.Format(Resources.FixedWeirValidator_ValidateSnapping_fixed_weir___0___not_within_grid_extent_, fixedWeir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentIssueMessage);
        }

        [TestCase(0, 0.0, -50, 0)]
        [TestCase(0, 0.0, 50, 0)]
        [TestCase(6, 0.0, -1, 2)]
        [TestCase(6, 0.0, 0, 0)]
        [TestCase(6, 0.0, 1, 0)]
        [TestCase(8, 0.1, 0, 2)]
        [TestCase(8, 0.1, 0.1, 0)]
        [TestCase(8, 0.1, 0.2, 0)]
        [TestCase(9, 0.0, -1, 2)]
        [TestCase(9, 0.0, 0, 0)]
        [TestCase(9, 0.0, 1, 0)]
        public void GivenAFixedWeirAnWithInvalidSillDepth_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned(
            int scheme, double minimumValue, double value, int nExpectedIssues)
        {
            // Given
            var gridExtent = new Envelope(new Coordinate(10, 10));
            List<ModelFeatureCoordinateData<FixedWeir>> fixedWeirsProperties =
                CreateModelFeatureCoordinateDataWithValues(fixedWeirs.First(), value);

            // When
            List<ValidationIssue> issues = fixedWeirs.Validate(gridExtent, fixedWeirsProperties, scheme.ToString()).ToList();

            // Then
            Assert.AreEqual(nExpectedIssues, issues.Count, MessageDifferentNumberValidationIssues);
            for (var i = 0; i < nExpectedIssues; i++)
            {
                var issue = issues[i];
                Assert.AreEqual(ValidationSeverity.Warning, issue.Severity, MessageDifferentValidationSeverity);
                string expectedMessage = string.Format(
                    Resources.FixedWeirValidator_Fixed_weir_contains_ground_heights_smaller_than_minimum,
                    fixedWeir.Name, ((FixedWeirSchemes) scheme).GetDescription(), i == 0 ? "left" : "right", minimumValue);
                Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentIssueMessage);
            }
        }

        private static List<ModelFeatureCoordinateData<FixedWeir>> CreateModelFeatureCoordinateDataWithValues(FixedWeir fixedWeir, double value)
        {
            var fixedWeirsProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

            var fixedWeirProperty = new ModelFeatureCoordinateData<FixedWeir> {Feature = fixedWeir};
            fixedWeirProperty.DataColumns.Add(new DataColumn<double>());
            fixedWeirProperty.DataColumns.Add(CreateDataColumnWithTwoValues(value));
            fixedWeirProperty.DataColumns.Add(CreateDataColumnWithTwoValues(value));
            fixedWeirsProperties.Add(fixedWeirProperty);
            return fixedWeirsProperties;
        }

        private static DataColumn<double> CreateDataColumnWithTwoValues(double value)
        {
            var dataColumn = new DataColumn<double>();
            dataColumn.ValueList.Add(value);
            dataColumn.ValueList.Add(value);
            return dataColumn;
        }
    }
}
