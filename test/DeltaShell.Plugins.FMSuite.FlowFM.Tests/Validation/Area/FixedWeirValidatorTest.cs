using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation.Area
{
    [TestFixture]
    public class FixedWeirValidatorTest
    {
        private const string MessageOneValidationIssueExpected = "Exactly one log message was expected when validating this weir.";
        private const string MessageDifferentLogMessageExpected = "A different log message for this issue was expected.";
        private const string MessageValidationSeverityWarningExpected = "The severity of this validation issue should have been of type Warning.";

        private IEnumerable<FixedWeir> fixedWeirs;

        [SetUp]
        public void SetUp()
        {
            var fixedWeir = new FixedWeir
            {
                Name = "fixed_weir",
                Geometry = new Point(new Coordinate(10, 10))
            };
            fixedWeirs = new List<FixedWeir>() {fixedWeir};
        }

        [Test]
        public void GivenAFixedWeirWithAGeometryThatDoesNotSnapToGrid_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var gridExtent = new Envelope();

            // When
            var issues = FixedWeirValidator.Validate(
                fixedWeirs,
                gridExtent,
                new List<ModelFeatureCoordinateData<FixedWeir>>()
            ).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Warning, issue.Severity, MessageValidationSeverityWarningExpected);
            Assert.AreEqual("fixed weir 'fixed_weir' not within grid extent.", issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAFixedWeirAnWithInvalidSillDepth_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var gridExtent = new Envelope(new Coordinate(10, 10));
            var fixedWeirsProperties = CreateModelFeatureCoordinateDataWithInvalidValues(fixedWeirs.First());

            // When
            var issues = FixedWeirValidator.Validate(
                fixedWeirs,
                gridExtent,
                fixedWeirsProperties
            ).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Warning, issue.Severity, MessageValidationSeverityWarningExpected);
            Assert.AreEqual("fixed weir 'fixed_weir' has unphysical sill depths, parts will be ignored by dflow-fm.", issue.Message, MessageDifferentLogMessageExpected);
        }

        private static List<ModelFeatureCoordinateData<FixedWeir>> CreateModelFeatureCoordinateDataWithInvalidValues(FixedWeir fixedWeir)
        {
            var fixedWeirsProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

            var fixedWeirProperty = new ModelFeatureCoordinateData<FixedWeir> {Feature = fixedWeir};
            fixedWeirProperty.DataColumns.Add(new DataColumn<double>());
            fixedWeirProperty.DataColumns.Add(CreateDataColumnOneZeroValue());
            fixedWeirProperty.DataColumns.Add(CreateDataColumnOneZeroValue());
            fixedWeirsProperties.Add(fixedWeirProperty);
            return fixedWeirsProperties;
        }

        private static DataColumn<double> CreateDataColumnOneZeroValue()
        {
            var dataColumn = new DataColumn<double>();
            dataColumn.ValueList.Add(0.0d);
            return dataColumn;
        }
    }
}
