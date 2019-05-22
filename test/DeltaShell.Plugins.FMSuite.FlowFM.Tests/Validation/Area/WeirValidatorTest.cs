using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation.Area
{
    [TestFixture]
    public class WeirValidatorTest
    {
        private const string MessageValidationSeverityErrorExpected = "The severity of this validation issue should have been of type Error.";
        private const string MessageOneValidationIssueExpected = "Exactly one log message was expected when validating this weir.";
        private const string MessageDifferentLogMessageExpected = "A different log message for this issue was expected.";

        private static DateTime modelStartTime;
        private static DateTime modelStopTime;
        private IList<Weir2D> weirs;

        [SetUp]
        public void SetUp()
        {
            modelStartTime = DateTime.Today;
            modelStopTime = DateTime.Today.AddDays(1);
            weirs = new List<Weir2D>();
        }

        /// <summary>
        /// GIVEN a general structure with an invalid crest width
        ///   AND a WaterFlowFMModel containing this general structure
        /// WHEN ValidateWeirs is called with these values
        /// THEN the correct validation issues are returned
        /// </summary>
        [TestCase(true, true, true, true, true)]
        [TestCase(false, true, true, true, true)]
        [TestCase(true, false, true, true, true)]
        [TestCase(true, true, false, true, true)]
        [TestCase(true, true, true, false, true)]
        [TestCase(true, true, true, true, false)]
        [TestCase(true, false, true, true, false)]
        [TestCase(false, false, true, true, false)]
        [TestCase(false, false, false, false, false)]
        public void GivenAGeneralStructureWithAnInvalidCrestWidthAndAWaterFlowFMModelContainingThisGeneralStructure_WhenValidateWeirsIsCalledWithTheseValues_ThenTheCorrectValidationIssuesAreReturned(bool validCrestWidth,
            bool validUpstream2,
            bool validUpstream1,
            bool validDownstream1,
            bool validDownstream2)
        {
            // Given
            var formula = new GeneralStructureWeirFormula
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                WidthStructureLeftSide = validUpstream2 ? 1.0 : -1.0,
                WidthLeftSideOfStructure = validUpstream1 ? 1.0 : -1.0,
                WidthStructureRightSide = validDownstream1 ? 1.0 : -1.0,
                WidthRightSideOfStructure = validDownstream2 ? 1.0 : -1.0
            };

            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
                CrestWidth = validCrestWidth ? 1.0 : -1.0
            };
            weirs.Add(weir);

            // When
            var validationIssues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, WeirValidator.CrestWidthPropertyName, weir, validCrestWidth);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, WeirValidator.Upstream2WidthPropertyName, weir, validUpstream2);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, WeirValidator.Upstream1WidthPropertyName, weir, validUpstream1);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, WeirValidator.Downstream1WidthPropertyName, weir, validDownstream1);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, WeirValidator.Downstream2WidthPropertyName, weir, validDownstream2);

            var nExpectedMessages = GetNumberOfExpectedMessagesInvalid(new[]
            {
                validCrestWidth, validUpstream2, validUpstream1, validDownstream1, validDownstream2
            });

            Assert.That(validationIssues.Count, Is.EqualTo(nExpectedMessages));
        }

        private static void AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(IEnumerable<ValidationIssue> issues, string propertyName, IWeir weir, bool isValid)
        {
            var expectedIssue = new ValidationIssue(weir,
                ValidationSeverity.Error,
                string.Format(Resources.WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___must_be_greater_than_0_, propertyName, weir.Name, weir.WeirFormula.GetName2D()),
                weir);

            Assert.That(issues.Contains(expectedIssue), Is.EqualTo(!isValid));
        }

        private static int GetNumberOfExpectedMessagesInvalid(IEnumerable<bool> values)
        {
            return values.Count(e => !e);
        }

        /// <summary>
        /// GIVEN a general structure with an empty crest width
        ///   AND a WaterFlowFMModel containing this general structure
        /// WHEN ValidateWeirs is called with these values
        /// THEN the correct validation issues are returned
        /// </summary>
        [TestCase(true, false, false, false, false)]
        [TestCase(false, true, false, false, false)]
        [TestCase(false, false, true, false, false)]
        [TestCase(false, false, false, true, false)]
        [TestCase(false, false, false, false, true)]
        [TestCase(false, false, false, true, true)]
        [TestCase(false, false, false, false, false)]
        [TestCase(true, true, true, true, true)]
        public void GivenAGeneralStructureWithAnEmptyCrestWidthAndAWaterFlowFMModelContainingThisGeneralStructure_WhenValidateWeirsIsCalledWithTheseValues_ThenTheCorrectValidationIssuesAreReturned(bool emptyCrestWidth,
            bool emptyUpstream2,
            bool emptyUpstream1,
            bool emptyDownstream1,
            bool emptyDownstream2)
        {
            // Given
            var formula = new GeneralStructureWeirFormula
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                WidthStructureLeftSide = emptyUpstream2 ? double.NaN : 1.0,
                WidthLeftSideOfStructure = emptyUpstream1 ? double.NaN : 1.0,
                WidthStructureRightSide = emptyDownstream1 ? double.NaN : 1.0,
                WidthRightSideOfStructure = emptyDownstream2 ? double.NaN : 1.0
            };

            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
                CrestWidth = emptyCrestWidth ? double.NaN : 1.0
            };
            weirs.Add(weir);

            // When
            var validationIssues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, WeirValidator.CrestWidthPropertyName, weir, emptyCrestWidth);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, WeirValidator.Upstream2WidthPropertyName, weir, emptyUpstream2);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, WeirValidator.Upstream1WidthPropertyName, weir, emptyUpstream1);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, WeirValidator.Downstream1WidthPropertyName, weir, emptyDownstream1);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, WeirValidator.Downstream2WidthPropertyName, weir, emptyDownstream2);

            var nExpectedMessages = GetNumberOfExpectedMessagesEmpty(new[]
            {
                emptyCrestWidth, emptyUpstream2, emptyUpstream1, emptyDownstream1, emptyDownstream2
            });

            Assert.That(validationIssues.Count, Is.EqualTo(nExpectedMessages));
        }

        private static void AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(IEnumerable<ValidationIssue> issues, string propertyName, IWeir weir, bool isEmpty)
        {
            var expectedMessage = string.Format(Resources.WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___will_be_calculated_by_the_computational_core_,
                propertyName,
                weir.Name,
                weir.WeirFormula.GetName2D());
            var expectedIssue = new ValidationIssue(weir, ValidationSeverity.Info, expectedMessage, weir);

            Assert.That(issues.Contains(expectedIssue), Is.EqualTo(isEmpty));
        }

        private static int GetNumberOfExpectedMessagesEmpty(IEnumerable<bool> values)
        {
            return values.Count(e => e);
        }

        [Test]
        public void GivenAWeirWithAGeometryThatDoesNotSnapToGrid_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Weir2D
            {
                CrestWidth = 1.0d,
                Geometry = new Point(new Coordinate(10, 10))
            };
            weirs.Add(weir);
            var gridExtent = new Envelope();

            // When
            var issues = weirs.Validate(gridExtent, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Warning, issue.Severity, "The severity of this validation issue should have been of type Warning.");
            var expectedMessage = string.Format(Resources.WeirValidator_ValidateSnapping__0__is_not_within_grid_extend_, weir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirWithAnInvalidLateralContraction_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Weir2D
            {
                CrestWidth = 1.0d,
                WeirFormula = new SimpleWeirFormula {LateralContraction = -1.0d}
            };
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            var expectedMessage = string.Format(Resources.WeirValidator_ValidateLateralContraction___0____lateral_contraction_coefficient_must_be_greater_than_or_equal_to_zero_,
                weir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirWithThatUsesCrestLevelTimeSeriesAndCrestLevelHasNoTimeSeries_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Weir2D
            {
                CrestWidth = 1.0d,
                UseCrestLevelTimeSeries = true
            };
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            var expectedMessage = string.Format(Resources.WeirValidator_ValidateCrestLevel___0____crest_level_time_series_does_not_contain_any_values_, weir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirWithACrestWidthWithAValueOfZero_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Weir2D {CrestWidth = 0.0d};
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            var expectedMessage = string.Format(Resources.WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___must_be_greater_than_0_,
                WeirValidator.CrestWidthPropertyName,
                weir.Name,
                weir.WeirFormula.GetName2D());
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirWithACrestWidthWithANaNValue_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Weir2D {CrestWidth = double.NaN};
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Info, issue.Severity, "The severity of this validation issue should have been of type Info.");
            var expectedMessage = string.Format(Resources.WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___will_be_calculated_by_the_computational_core_,
                WeirValidator.CrestWidthPropertyName,
                weir.Name,
                weir.WeirFormula.GetName2D());
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirWithCrestLevelTimeSeriesThatDoesNotSpanTheModelRunInterval_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Weir2D
            {
                CrestWidth = 1.0d,
                UseCrestLevelTimeSeries = true
            };
            weir.CrestLevelTimeSeries.Time.Values.Add(modelStartTime.AddHours(1));
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            var expectedMessage = string.Format(Resources.WeirValidator_ValidateCrestLevel___0____crest_level_time_series_does_not_span_the_model_run_interval_, weir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGeneralStructureWithAnInvalidHorizontalDoorOpeningDirection_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Weir2D
            {
                WeirFormula = new GeneralStructureWeirFormula
                {
                    WidthStructureCentre = 1.0,
                    WidthLeftSideOfStructure = 1.0,
                    WidthStructureLeftSide = 1.0,
                    WidthRightSideOfStructure = 1.0,
                    WidthStructureRightSide = 1.0,
                    HorizontalDoorOpeningDirection = GateOpeningDirection.FromLeft
                }
            };
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            var expectedMessage = string.Format(Resources.WeirValidator_ValidateHorizontalDoorOpeningDirection___0____only_symmetric_horizontal_door_opening_direction_is_supported_for_general_structures_,
                weir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGeneralStructureWithInvalidCrestWidths_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var weir = new Weir2D
            {
                WeirFormula = new GeneralStructureWeirFormula
                {
                    WidthStructureCentre = -1.0d,
                    WidthLeftSideOfStructure = -1.0d,
                    WidthStructureLeftSide = -1.0d,
                    WidthRightSideOfStructure = -1.0d,
                    WidthStructureRightSide = -1.0d
                }
            };
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(5, issues.Count, "Exactly 5 log messages were expected when validating this weir.");
            AssertCrestWidthErrorExists(weir, issues, WeirValidator.CrestWidthPropertyName);
            AssertCrestWidthErrorExists(weir, issues, WeirValidator.Downstream1WidthPropertyName);
            AssertCrestWidthErrorExists(weir, issues, WeirValidator.Downstream2WidthPropertyName);
            AssertCrestWidthErrorExists(weir, issues, WeirValidator.Upstream1WidthPropertyName);
            AssertCrestWidthErrorExists(weir, issues, WeirValidator.Upstream2WidthPropertyName);
        }

        private static void AssertCrestWidthErrorExists(IWeir weir, IEnumerable<ValidationIssue> issues, string propertyName)
        {
            var expectedMessage = string.Format(
                Resources.WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___must_be_greater_than_0_,
                propertyName,
                weir.Name,
                weir.WeirFormula.GetName2D());

            var expectedIssue = issues.FirstOrDefault(i => i.Message == expectedMessage);
            Assert.NotNull(expectedIssue, $"The following message was expected in the returned validation messages: '{expectedMessage}'");
            Assert.AreEqual(ValidationSeverity.Error, expectedIssue.Severity, MessageValidationSeverityErrorExpected);
        }

        [Test]
        public void GivenAGatedWeirWithAnInvalidDoorHeight_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var weir = new Weir2D
            {
                CrestWidth = 1.0d,
                WeirFormula = new GatedWeirFormula {DoorHeight = -1.0d}
            };
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateDoorHeight___0____door_height_must_be_greater_than_or_equal_to_0_, weir.Name),
                issue.Message,
                MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithAnInvalidHorizontalDoorOpeningWidth_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var weir = new Weir2D
            {
                CrestWidth = 1.0d,
                WeirFormula = new GatedWeirFormula {HorizontalDoorOpeningWidth = -1.0d}
            };
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateHorizontalDoorOpeningWidth___0____opening_width_must_be_greater_than_or_equal_to_0_, weir.Name),
                issue.Message,
                MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithHorizontalDoorOpeningWidthTimeSeriesWithAtLeastOneValueSmallerThanZero_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var gatedWeirFormula = new GatedWeirFormula(true) {UseHorizontalDoorOpeningWidthTimeSeries = true};
            gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries.Time.AddValues(new[] {modelStartTime, modelStopTime});
            gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries.SetValues(new[] {-1.0d, 1.0d});

            var weir = new Weir2D(true)
            {
                CrestWidth = 1.0d,
                WeirFormula = gatedWeirFormula
            };
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateHorizontalDoorOpeningWidth___0____opening_width_time_series_values_must_be_greater_than_or_equal_to_0_, weir.Name),
                issue.Message,
                MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithHorizontalDoorOpeningWidthTimeSeriesWithoutValues_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var weir = new Weir2D(true)
            {
                CrestWidth = 1.0d,
                WeirFormula = new GatedWeirFormula(true) {UseHorizontalDoorOpeningWidthTimeSeries = true}
            };
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateHorizontalDoorOpeningWidth___0____opening_width_time_series_does_not_contain_any_values_, weir.Name),
                issue.Message,
                MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithHorizontalDoorOpeningWidthTimeSeriesThatDoesNotSpanTheModelRunInterval_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var gatedWeirFormula = new GatedWeirFormula(true) {UseHorizontalDoorOpeningWidthTimeSeries = true};
            gatedWeirFormula.HorizontalDoorOpeningWidthTimeSeries.Time.Values.Add(modelStartTime.AddHours(1));

            var weir = new Weir2D(true)
            {
                CrestWidth = 1.0d,
                WeirFormula = gatedWeirFormula
            };
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateHorizontalDoorOpeningWidth___0____opening_width_time_series_does_not_span_the_model_run_interval_, weir.Name),
                issue.Message,
                MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithLowerEdgeLevelTimeSeriesThatDoesNotSpanTheModelRunInterval_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var gatedWeirFormula = new GatedWeirFormula(true) {UseLowerEdgeLevelTimeSeries = true};
            gatedWeirFormula.LowerEdgeLevelTimeSeries.Time.Values.Add(modelStartTime.AddHours(1));

            var weir = new Weir2D(true)
            {
                CrestWidth = 1.0d,
                WeirFormula = gatedWeirFormula
            };
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateLowerEdgeLevel___0____lower_edge_level_time_series_does_not_span_the_model_run_interval_, weir.Name),
                issue.Message,
                MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithLowerEdgeLevelTimeSeriesWithoutValues_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var weir = new Weir2D(true)
            {
                CrestWidth = 1.0d,
                WeirFormula = new GatedWeirFormula(true) {UseLowerEdgeLevelTimeSeries = true}
            };
            weirs.Add(weir);

            // When
            var issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateLowerEdgeLevel___0____lower_edge_level_time_series_does_not_contain_any_values_, weir.Name),
                issue.Message,
                MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirValidator_WhenPropertyNamesAreCalled_ThenExpectedStringsAreReturned()
        {
            Assert.AreEqual(WeirValidator.CrestWidthPropertyName, "Crest Width");
            Assert.AreEqual(WeirValidator.Upstream1WidthPropertyName, "Upstream 1 Width");
            Assert.AreEqual(WeirValidator.Upstream2WidthPropertyName, "Upstream 2 Width");
            Assert.AreEqual(WeirValidator.Downstream1WidthPropertyName, "Downstream 1 Width");
            Assert.AreEqual(WeirValidator.Downstream2WidthPropertyName, "Downstream 2 Width");
        }
    }
}