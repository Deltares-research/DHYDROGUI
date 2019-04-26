using System;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation.Area
{
    [TestFixture]
    public class FixedWeirValidatorTest
    {
        private const string MessageOneValidationIssueExpected = "Exactly one log message was expected when validating this weir.";
        private const string MessageDifferentLogMessageExpected = "A different log message for this issue was expected.";
        private const string MessageValidationSeverityWarningExpected = "The severity of this validation issue should have been of type Warning.";

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
            fixedWeirs = new List<FixedWeir> { fixedWeir };
        }

        [Test]
        public void GivenAFixedWeirWithAGeometryThatDoesNotSnapToGrid_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var gridExtent = new Envelope();

            // When
            var issues = fixedWeirs.Validate(gridExtent, new List<ModelFeatureCoordinateData<FixedWeir>>(), String.Empty).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Warning, issue.Severity, MessageValidationSeverityWarningExpected);
            var expectedMessage = string.Format(Resources.FixedWeirValidator_ValidateSnapping_fixed_weir___0___not_within_grid_extent_, fixedWeir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAFixedWeirAnWithInvalidSillDepthAndFixedWeirSchemeIsNoneWhenValidateIsCalled_ThenNoExpectedValidationIssueIsReturned()
        {
            // Given
            var gridExtent = new Envelope(new Coordinate(10, 10));
            var fixedWeirsProperties = CreateModelFeatureCoordinateDataWithInvalidValues(fixedWeirs.First());
            var fixedWeirScheme = "0";

            // When
            var issues = fixedWeirs.Validate(gridExtent, fixedWeirsProperties, fixedWeirScheme).ToList();

            // Then
            Assert.AreEqual(0, issues.Count, MessageOneValidationIssueExpected);
        }

        [Test]
        public void GivenAFixedWeirAnWithInvalidSillDepthAndFixedWeirSchemeIsNumerical_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var gridExtent = new Envelope(new Coordinate(10, 10));
            var fixedWeirsProperties = CreateModelFeatureCoordinateDataWithInvalidValues(fixedWeirs.First());
            fixedWeirsProperties.FirstOrDefault(d => d.Feature == fixedWeir).DataColumns[1].ValueList[0] = -1.0;
            var numericalFixedWeirScheme = "6";

            // When
            var issues = fixedWeirs.Validate(gridExtent, fixedWeirsProperties, numericalFixedWeirScheme).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Info, issue.Severity, MessageValidationSeverityWarningExpected);
            var expectedMessage = string.Format(Resources.FixedWeirValidator_ValidateSillDepths_fixed_weir___0___has_unphysical_sill_depths__parts_will_be_ignored_by_dflow_fm_,
                fixedWeir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAFixedWeirAnWithInvalidSillDepthAndFixedWeirSchemeIsVilleMonte_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var gridExtent = new Envelope(new Coordinate(10, 10));
            var fixedWeirsProperties = CreateModelFeatureCoordinateDataWithInvalidValues(fixedWeirs.First());
            fixedWeirsProperties.FirstOrDefault(d => d.Feature == fixedWeir).DataColumns[1].ValueList[0] = -1.0;
            var villemonteFixedWeirScheme = "9";

            // When
            var issues = fixedWeirs.Validate(gridExtent, fixedWeirsProperties, villemonteFixedWeirScheme).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Info, issue.Severity, MessageValidationSeverityWarningExpected);
            var expectedMessage = string.Format(Resources.FixedWeirValidator_ValidateSillDepths_fixed_weir___0___has_unphysical_sill_depths__parts_will_be_ignored_by_dflow_fm_,
                fixedWeir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAFixedWeirAnWithInvalidSillDepthAndFixedWeirSchemeIsTabellenBoek_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturnedANdDefaultValueIsSet()
        {
            // Given
            var gridExtent = new Envelope(new Coordinate(10, 10));
            var fixedWeirsProperties = CreateModelFeatureCoordinateDataWithInvalidValues(fixedWeirs.First());
            fixedWeirsProperties.FirstOrDefault(d => d.Feature == fixedWeir).DataColumns[1].ValueList[0] = 0.0;
            var tabellenBoekFixedWeirScheme = "8";

            // When
            var issues = fixedWeirs.Validate(gridExtent, fixedWeirsProperties, tabellenBoekFixedWeirScheme).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Info, issue.Severity, MessageValidationSeverityWarningExpected);
            var expectedMessage = string.Format(Resources.FixedWeirValidator_ValidateSillDepths__0___Fixed_weir_with_type_1_have_a_ground_heights_smaller_than_0_10_m__A_minimum_of_0_10_m_will_be_applied_by_the_computational_core_,
                fixedWeir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        private static List<ModelFeatureCoordinateData<FixedWeir>> CreateModelFeatureCoordinateDataWithInvalidValues(FixedWeir fixedWeir)
        {
            var fixedWeirsProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

            var fixedWeirProperty = new ModelFeatureCoordinateData<FixedWeir> { Feature = fixedWeir };
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
