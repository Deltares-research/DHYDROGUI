using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation.Area;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation.Area
{
    [TestFixture]
    public class WeirValidatorTest
    {
        private WaterFlowFMModel model;

        [SetUp]
        public void SetUp()
        {
            model = new WaterFlowFMModel();
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

            model.Area.Weirs.Add(weir);

            // When 
            var validationIssues = WeirValidator.Validate(model, model.Area.Weirs).ToList();

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

        private static void AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(IEnumerable<ValidationIssue> issues , string propertyName , Weir2D weir , bool isValid)
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

            model.Area.Weirs.Add(weir);

            // When 
            var validationIssues = WeirValidator.Validate(model, model.Area.Weirs).ToList();

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

    }
}