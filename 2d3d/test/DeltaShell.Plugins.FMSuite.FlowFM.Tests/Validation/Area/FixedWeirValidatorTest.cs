using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
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

        [TestCase(FixedWeirSchemes.None)]
        [TestCase(FixedWeirSchemes.Scheme6)]
        [TestCase(FixedWeirSchemes.Scheme8)]
        [TestCase(FixedWeirSchemes.Scheme9)]
        public void GivenAFixedWeirWithAGeometryThatDoesNotSnapToGrid_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned(FixedWeirSchemes scheme)
        {
            // Given
            var gridExtent = new Envelope();

            // When
            List<ValidationIssue> issues = fixedWeirs.Validate(gridExtent, new List<ModelFeatureCoordinateData<FixedWeir>>(), scheme).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageDifferentNumberValidationIssues);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Warning, issue.Severity, MessageDifferentValidationSeverity);
            string expectedMessage = string.Format(Resources.FixedWeirValidator_ValidateSnapping_fixed_weir___0___not_within_grid_extent_, fixedWeir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentIssueMessage);
        }

        [TestCase(FixedWeirSchemes.None, -50, 0)]
        [TestCase(FixedWeirSchemes.None, 50, 0)]
        [TestCase(FixedWeirSchemes.Scheme6, -1, 2)]
        [TestCase(FixedWeirSchemes.Scheme6, 0, 0)]
        [TestCase(FixedWeirSchemes.Scheme6, 1, 0)]
        [TestCase(FixedWeirSchemes.Scheme8, 0, 2)]
        [TestCase(FixedWeirSchemes.Scheme8, 0.1, 0)]
        [TestCase(FixedWeirSchemes.Scheme8, 0.2, 0)]
        [TestCase(FixedWeirSchemes.Scheme9, -1, 2)]
        [TestCase(FixedWeirSchemes.Scheme9, 0, 0)]
        [TestCase(FixedWeirSchemes.Scheme9, 1, 0)]
        public void GivenAFixedWeirAnWithInvalidGroundHeights_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned(
            FixedWeirSchemes scheme, double value, int nExpectedIssues)
        {
            // Given
            var gridExtent = new Envelope(new Coordinate(10, 10));
            List<ModelFeatureCoordinateData<FixedWeir>> fixedWeirsProperties =
                CreateModelFeatureCoordinateDataWithValues(fixedWeirs.First(), value);
            double minimumValue = scheme.GetMinimalAllowedGroundHeight();

            // When
            List<ValidationIssue> issues = fixedWeirs.Validate(gridExtent, fixedWeirsProperties, scheme).ToList();

            // Then
            Assert.AreEqual(nExpectedIssues, issues.Count, MessageDifferentNumberValidationIssues);
            for (var i = 0; i < nExpectedIssues; i++)
            {
                ValidationIssue issue = issues[i];
                Assert.AreEqual(ValidationSeverity.Info, issue.Severity, MessageDifferentValidationSeverity);
                string expectedMessage = string.Format(
                    Resources.FixedWeirValidator_Fixed_weir_contains_ground_heights_smaller_than_minimum,
                    fixedWeir.Name, scheme.GetDescription(), i == 0 ? "left" : "right", minimumValue.ToString("0.00", CultureInfo.InvariantCulture));
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