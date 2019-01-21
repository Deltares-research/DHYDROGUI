using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var formula = new GeneralStructureWeirFormula()
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                WidthStructureLeftSide = validUpstream2 ? 1.0 : -1.0,
                WidthLeftSideOfStructure = validUpstream1 ? 1.0 : -1.0,
                WidthStructureRightSide = validDownstream1 ? 1.0 : -1.0,
                WidthRightSideOfStructure = validDownstream2 ? 1.0 : -1.0,
            };

            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
                CrestWidth = validCrestWidth ? 1.0 : -1.0,
            };
            weirs.Add(weir);

            // When 
            var validationIssues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, "Crest", weir, validCrestWidth);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, "Upstream 2", weir, validUpstream2);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, "Upstream 1", weir, validUpstream1);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, "Downstream 1", weir, validDownstream1);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, "Downstream 2", weir, validDownstream2);

            var nExpectedMessages = GetNumberOfExpectedMessagesInvalid(new bool[5]
            {
                validCrestWidth, validUpstream2, validUpstream1, validDownstream1, validDownstream2
            });

            Assert.That(validationIssues.Count, Is.EqualTo(nExpectedMessages));
        }

        private static void AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(IEnumerable<ValidationIssue> issues, string propertyName, Weir2D weir, bool isValid)
        {
            var expectedIssue = new ValidationIssue(weir
                , ValidationSeverity.Error
                , $"{propertyName} Width for '{weir.Name}' structure type: {weir.WeirFormula.GetName2D()}, must be greater than 0."
                , weir);

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
            var formula = new GeneralStructureWeirFormula()
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                WidthStructureLeftSide = emptyUpstream2 ? double.NaN : 1.0,
                WidthLeftSideOfStructure = emptyUpstream1 ? double.NaN : 1.0,
                WidthStructureRightSide = emptyDownstream1 ? double.NaN : 1.0,
                WidthRightSideOfStructure = emptyDownstream2 ? double.NaN : 1.0,
            };

            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
                CrestWidth = emptyCrestWidth ? double.NaN : 1.0,
            };
            weirs.Add(weir);

            // When
            var validationIssues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, "Crest", weir, emptyCrestWidth);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, "Upstream 2", weir, emptyUpstream2);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, "Upstream 1", weir, emptyUpstream1);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, "Downstream 1", weir, emptyDownstream1);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, "Downstream 2", weir, emptyDownstream2);

            var nExpectedMessages = GetNumberOfExpectedMessagesEmpty(new bool[5]
            {
                emptyCrestWidth, emptyUpstream2, emptyUpstream1, emptyDownstream1, emptyDownstream2
            });

            Assert.That(validationIssues.Count, Is.EqualTo(nExpectedMessages));
        }

        private static void AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(IEnumerable<ValidationIssue> issues, string propertyName, Weir2D weir, bool isEmpty)
        {
            var expectedIssue = new ValidationIssue(weir
                , ValidationSeverity.Info
                , $"{propertyName} Width for '{weir.Name}' structure type: {weir.WeirFormula.GetName2D()}, will be calculated by the computational core."
                , weir);

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
            var gridExtent = new UnstructuredGrid {Vertices = new[] {new Coordinate(0, 0)}}.GetExtents();

            // When
            var issues = WeirValidator.Validate(weirs, gridExtent, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Warning, issue.Severity, "The severity of this validation issue should have been of type Warning.");
            Assert.AreEqual("Structure is not within grid extend.", issue.Message, MessageDifferentLogMessageExpected);
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
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual("'Structure': lateral contraction coefficient must be greater than or equal to zero.", issue.Message, MessageDifferentLogMessageExpected);
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
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual("'Structure': crest level time series does not contain any values.", issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirWithACrestWidthWithAValueOfZero_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Weir2D {CrestWidth = 0.0d};
            weirs.Add(weir);

            // When
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual("Crest Width for 'Structure' structure type: Simple weir, must be greater than 0.", issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAWeirWithACrestWidthWithANaNValue_WhenValidateIsCalled_ThenExpectedValidationIssueIsReturned()
        {
            // Given
            var weir = new Weir2D {CrestWidth = double.NaN};
            weirs.Add(weir);

            // When
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Info, issue.Severity, "The severity of this validation issue should have been of type Info.");
            Assert.AreEqual("Crest Width for 'Structure' structure type: Simple weir, will be calculated by the computational core.", issue.Message, MessageDifferentLogMessageExpected);
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
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual("'Structure': crest level time series does not span the model run interval.", issue.Message, MessageDifferentLogMessageExpected);
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
                },
            };
            weirs.Add(weir);

            // When
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual("'Structure': only symmetric horizontal door opening direction is supported for general structures.", issue.Message, MessageDifferentLogMessageExpected);
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
                    WidthStructureRightSide = -1.0d,
                },
            };
            weirs.Add(weir);

            // When
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(5, issues.Count, "Exactly 5 log messages were expected when validating this weir.");
            Assert.IsTrue(issues.All(i => i.Severity == ValidationSeverity.Error), "The severity of all these validation issues should have been of type Error.");
            var messages = issues.Select(i => i.Message);
            const string expectedMessageWithoutPropertyName = "for 'Structure' structure type: General structure, must be greater than 0.";
            Assert.IsTrue(messages.All(m => m.EndsWith(expectedMessageWithoutPropertyName, StringComparison.Ordinal)), "All log messages of these issues should have ended with the same expected message.");
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
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual("'Structure': door height must be greater than or equal to 0.", issue.Message, MessageDifferentLogMessageExpected);
        }

        [Test]
        public void GivenAGatedWeirWithAnInvalidHorizontalDoorOpeningWidth_WhenValidateIsCalled_ThenExpectedValidationIssuesAreReturned()
        {
            // Given
            var weir = new Weir2D
            {
                CrestWidth = 1.0d,
                WeirFormula = new GatedWeirFormula {HorizontalDoorOpeningWidth = -1.0d},
            };
            weirs.Add(weir);

            // When
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual("'Structure': opening width must be greater than or equal to 0.", issue.Message, MessageDifferentLogMessageExpected);
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
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual("'Structure': opening width time series values must be greater than or equal to 0.", issue.Message, MessageDifferentLogMessageExpected);
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
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual("'Structure': opening width time series does not contain any values.", issue.Message, MessageDifferentLogMessageExpected);
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
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual("'Structure': opening width time series does not span the model run interval.", issue.Message, MessageDifferentLogMessageExpected);
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
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual("'Structure': lower edge level time series does not span the model run interval.", issue.Message, MessageDifferentLogMessageExpected);
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
            var issues = WeirValidator.Validate(weirs, null, modelStartTime, modelStopTime).ToList();

            // Then
            Assert.AreEqual(1, issues.Count, MessageOneValidationIssueExpected);
            var issue = issues.Single();
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity, MessageValidationSeverityErrorExpected);
            Assert.AreEqual("'Structure': lower edge level time series does not contain any values.", issue.Message, MessageDifferentLogMessageExpected);
        }
    }
}