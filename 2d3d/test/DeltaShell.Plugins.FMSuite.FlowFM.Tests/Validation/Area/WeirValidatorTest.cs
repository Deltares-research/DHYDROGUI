using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

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
        private IList<IStructure> weirs;

        [SetUp]
        public void SetUp()
        {
            modelStartTime = DateTime.Today;
            modelStopTime = DateTime.Today.AddDays(1);
            weirs = new List<IStructure>();
        }

        [Test]
        public void GivenAWeirWithAGeometryThatDoesNotSnapToGrid_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Structure
            {
                CrestWidth = 1.0d,
                Geometry = new Point(new Coordinate(10, 10))
            };
            weirs.Add(weir);
            var gridExtent = new Envelope();

            // When
            List<ValidationIssue> issues = weirs.Validate(gridExtent, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Warning, issue.Severity, "The severity of this validation issue should have been of type Warning.");
            string expectedMessage = string.Format(Resources.WeirValidator_ValidateSnapping__0__is_not_within_grid_extend_, weir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirWithAnInvalidLateralContraction_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Structure
            {
                CrestWidth = 1.0d,
                Formula = new SimpleWeirFormula {LateralContraction = -1.0d}
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            string expectedMessage = string.Format(Resources.WeirValidator_ValidateLateralContraction___0____lateral_contraction_coefficient_must_be_greater_than_or_equal_to_zero_,
                                                   weir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirWithThatUsesCrestLevelTimeSeriesAndCrestLevelHasNoTimeSeries_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Structure
            {
                CrestWidth = 1.0d,
                UseCrestLevelTimeSeries = true
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            string expectedMessage = string.Format(Resources.WeirValidator_ValidateCrestLevel___0____crest_level_time_series_does_not_contain_any_values_, weir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirWithACrestWidthWithAValueOfZero_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Structure {CrestWidth = 0.0d};
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            string expectedMessage = string.Format(Resources.WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___must_be_greater_than_0_,
                                                   StructureValidator.CrestWidthPropertyName,
                                                   weir.Name,
                                                   weir.Formula.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirWithACrestWidthWithANaNValue_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Structure {CrestWidth = double.NaN};
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Info, issue.Severity, "The severity of this validation issue should have been of type Info.");
            string expectedMessage = string.Format(Resources.WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___will_be_calculated_by_the_computational_core_,
                                                   StructureValidator.CrestWidthPropertyName,
                                                   weir.Name,
                                                   weir.Formula.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirWithCrestLevelTimeSeriesThatDoesNotSpanTheModelRunInterval_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Structure
            {
                CrestWidth = 1.0d,
                UseCrestLevelTimeSeries = true
            };
            weir.CrestLevelTimeSeries.Time.Values.Add(modelStartTime.AddHours(1));
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            string expectedMessage = string.Format(Resources.WeirValidator_ValidateCrestLevel___0____crest_level_time_series_does_not_span_the_model_run_interval_, weir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGeneralStructureWithAnInvalidHorizontalGateOpeningDirection_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Structure
            {
                Formula = new GeneralStructureFormula
                {
                    CrestWidth = 1.0,
                    Upstream1Width = 1.0,
                    Upstream2Width = 1.0,
                    Downstream2Width = 1.0,
                    Downstream1Width = 1.0,
                    GateOpeningHorizontalDirection = GateOpeningDirection.FromLeft
                }
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            string expectedMessage = string.Format(Resources.WeirValidator_ValidateHorizontalGateOpeningDirection___0____only_symmetric_gate_opening_horizontal_direction_is_supported_for_general_structures_,
                                                   weir.Name);
            Assert.AreEqual(expectedMessage, issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGeneralStructureWithInvalidCrestWidths_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var weir = new Structure
            {
                Formula = new GeneralStructureFormula
                {
                    CrestWidth = -1.0d,
                    Upstream1Width = -1.0d,
                    Upstream2Width = -1.0d,
                    Downstream2Width = -1.0d,
                    Downstream1Width = -1.0d
                }
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(5, issues.Count, "Exactly 5 log messages were expected when validating this weir.");
            AssertCrestWidthErrorExists(weir, issues, StructureValidator.CrestWidthPropertyName);
            AssertCrestWidthErrorExists(weir, issues, StructureValidator.Downstream1WidthPropertyName);
            AssertCrestWidthErrorExists(weir, issues, StructureValidator.Downstream2WidthPropertyName);
            AssertCrestWidthErrorExists(weir, issues, StructureValidator.Upstream1WidthPropertyName);
            AssertCrestWidthErrorExists(weir, issues, StructureValidator.Upstream2WidthPropertyName);
        }

        [Test]
        public void GivenAGatedWeirWithAnInvalidGateHeight_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var weir = new Structure
            {
                CrestWidth = 1.0d,
                Formula = new SimpleGateFormula {GateHeight = -1.0d}
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateGateHeight___0____gate_height_must_be_greater_than_or_equal_to_0_, weir.Name),
                            issue.Message,
                            MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithAnInvalidHorizontalGateOpeningWidth_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var weir = new Structure
            {
                CrestWidth = 1.0d,
                Formula = new SimpleGateFormula {HorizontalGateOpeningWidth = -1.0d}
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateHorizontalGateOpeningWidth___0____gate_opening_width_must_be_greater_than_or_equal_to_0_, weir.Name),
                            issue.Message,
                            MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithHorizontalGateOpeningWidthTimeSeriesWithAtLeastOneValueSmallerThanZero_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var gatedWeirFormula = new SimpleGateFormula(true) {UseHorizontalGateOpeningWidthTimeSeries = true};
            gatedWeirFormula.HorizontalGateOpeningWidthTimeSeries.Time.AddValues(new[]
            {
                modelStartTime,
                modelStopTime
            });
            gatedWeirFormula.HorizontalGateOpeningWidthTimeSeries.SetValues(new[]
            {
                -1.0d,
                1.0d
            });

            var weir = new Structure()
            {
                CrestWidth = 1.0d,
                Formula = gatedWeirFormula
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateHorizontalGateOpeningWidth___0____gate_opening_width_time_series_values_must_be_greater_than_or_equal_to_0_, weir.Name),
                            issue.Message,
                            MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithHorizontalGateOpeningWidthTimeSeriesWithoutValues_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var weir = new Structure()
            {
                CrestWidth = 1.0d,
                Formula = new SimpleGateFormula(true) {UseHorizontalGateOpeningWidthTimeSeries = true}
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateHorizontalGateOpeningWidth___0____gate_opening_width_time_series_does_not_contain_any_values_, weir.Name),
                            issue.Message,
                            MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithHorizontalGateOpeningWidthTimeSeriesThatDoesNotSpanTheModelRunInterval_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var gatedWeirFormula = new SimpleGateFormula(true) {UseHorizontalGateOpeningWidthTimeSeries = true};
            gatedWeirFormula.HorizontalGateOpeningWidthTimeSeries.Time.Values.Add(modelStartTime.AddHours(1));

            var weir = new Structure()
            {
                CrestWidth = 1.0d,
                Formula = gatedWeirFormula
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateHorizontalGateOpeningWidth___0____gate_opening_width_time_series_does_not_span_the_model_run_interval_, weir.Name),
                            issue.Message,
                            MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithGateLowerEdgeLevelTimeSeriesThatDoesNotSpanTheModelRunInterval_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var gatedWeirFormula = new SimpleGateFormula(true) {UseGateLowerEdgeLevelTimeSeries = true};
            gatedWeirFormula.GateLowerEdgeLevelTimeSeries.Time.Values.Add(modelStartTime.AddHours(1));

            var weir = new Structure()
            {
                CrestWidth = 1.0d,
                Formula = gatedWeirFormula
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateGateLowerEdgeLevel___0____gate_lower_edge_level_time_series_does_not_span_the_model_run_interval_, weir.Name),
                            issue.Message,
                            MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithGateLowerEdgeLevelTimeSeriesWithoutValues_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var weir = new Structure()
            {
                CrestWidth = 1.0d,
                Formula = new SimpleGateFormula(true) {UseGateLowerEdgeLevelTimeSeries = true}
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> issues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            ValidationIssue issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual(string.Format(Resources.WeirValidator_ValidateGateLowerEdgeLevel___0____gate_lower_edge_level_time_series_does_not_contain_any_values_, weir.Name),
                            issue.Message,
                            MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirValidator_WhenPropertyNamesAreCalled_ThenExpectedStringsAreReturned()
        {
            Assert.AreEqual(StructureValidator.CrestWidthPropertyName, "Crest Width");
            Assert.AreEqual(StructureValidator.Upstream1WidthPropertyName, "Upstream 1 Width");
            Assert.AreEqual(StructureValidator.Upstream2WidthPropertyName, "Upstream 2 Width");
            Assert.AreEqual(StructureValidator.Downstream1WidthPropertyName, "Downstream 1 Width");
            Assert.AreEqual(StructureValidator.Downstream2WidthPropertyName, "Downstream 2 Width");
        }

        /// <summary>
        /// GIVEN a general structure with an invalid crest width
        /// AND a WaterFlowFMModel containing this general structure
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
            var formula = new GeneralStructureFormula
            {
                GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric,
                Upstream2Width = validUpstream2 ? 1.0 : -1.0,
                Upstream1Width = validUpstream1 ? 1.0 : -1.0,
                Downstream1Width = validDownstream1 ? 1.0 : -1.0,
                Downstream2Width = validDownstream2 ? 1.0 : -1.0
            };

            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = validCrestWidth ? 1.0 : -1.0
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> validationIssues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, StructureValidator.CrestWidthPropertyName, weir, validCrestWidth);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, StructureValidator.Upstream2WidthPropertyName, weir, validUpstream2);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, StructureValidator.Upstream1WidthPropertyName, weir, validUpstream1);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, StructureValidator.Downstream1WidthPropertyName, weir, validDownstream1);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, StructureValidator.Downstream2WidthPropertyName, weir, validDownstream2);

            int nExpectedMessages = GetNumberOfExpectedMessagesInvalid(new[]
            {
                validCrestWidth,
                validUpstream2,
                validUpstream1,
                validDownstream1,
                validDownstream2
            });

            Assert.That(validationIssues.Count, Is.EqualTo(nExpectedMessages));
        }

        private static void AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(IEnumerable<ValidationIssue> issues, string propertyName, IStructure weir, bool isValid)
        {
            var expectedIssue = new ValidationIssue(weir,
                                                    ValidationSeverity.Error,
                                                    string.Format(Resources.WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___must_be_greater_than_0_, propertyName, weir.Name, weir.Formula.Name),
                                                    weir);

            Assert.That(issues.Contains(expectedIssue), Is.EqualTo(!isValid));
        }

        private static int GetNumberOfExpectedMessagesInvalid(IEnumerable<bool> values)
        {
            return values.Count(e => !e);
        }

        /// <summary>
        /// GIVEN a general structure with an empty crest width
        /// AND a WaterFlowFMModel containing this general structure
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
            var formula = new GeneralStructureFormula
            {
                GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric,
                Upstream2Width = emptyUpstream2 ? double.NaN : 1.0,
                Upstream1Width = emptyUpstream1 ? double.NaN : 1.0,
                Downstream1Width = emptyDownstream1 ? double.NaN : 1.0,
                Downstream2Width = emptyDownstream2 ? double.NaN : 1.0
            };

            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = emptyCrestWidth ? double.NaN : 1.0
            };
            weirs.Add(weir);

            // When
            List<ValidationIssue> validationIssues = weirs.Validate(null, modelStartTime, modelStopTime).ToList();

            // Then
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, StructureValidator.CrestWidthPropertyName, weir, emptyCrestWidth);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, StructureValidator.Upstream2WidthPropertyName, weir, emptyUpstream2);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, StructureValidator.Upstream1WidthPropertyName, weir, emptyUpstream1);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, StructureValidator.Downstream1WidthPropertyName, weir, emptyDownstream1);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, StructureValidator.Downstream2WidthPropertyName, weir, emptyDownstream2);

            int nExpectedMessages = GetNumberOfExpectedMessagesEmpty(new[]
            {
                emptyCrestWidth,
                emptyUpstream2,
                emptyUpstream1,
                emptyDownstream1,
                emptyDownstream2
            });

            Assert.That(validationIssues.Count, Is.EqualTo(nExpectedMessages));
        }

        private static void AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(IEnumerable<ValidationIssue> issues, string propertyName, IStructure weir, bool isEmpty)
        {
            string expectedMessage = string.Format(Resources.WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___will_be_calculated_by_the_computational_core_,
                                                   propertyName,
                                                   weir.Name,
                                                   weir.Formula.Name);
            var expectedIssue = new ValidationIssue(weir, ValidationSeverity.Info, expectedMessage, weir);

            Assert.That(issues.Contains(expectedIssue), Is.EqualTo(isEmpty));
        }

        private static int GetNumberOfExpectedMessagesEmpty(IEnumerable<bool> values)
        {
            return values.Count(e => e);
        }

        private static void AssertCrestWidthErrorExists(IStructure weir, IEnumerable<ValidationIssue> issues, string propertyName)
        {
            string expectedMessage = string.Format(
                Resources.WeirValidator_ValidateCrestWidth__0__for___1___structure_type___2___must_be_greater_than_0_,
                propertyName,
                weir.Name,
                weir.Formula.Name);

            ValidationIssue expectedIssue = issues.FirstOrDefault(i => i.Message == expectedMessage);
            Assert.NotNull(expectedIssue, $"The following message was expected in the returned validation messages: '{expectedMessage}'");
            Assert.AreEqual(ValidationSeverity.Error, expectedIssue.Severity, MessageValidationSeverityErrorExpected);
        }
    }
}