using System;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class FMStructuresValidatorTest
    {
        private WaterFlowFMModel model;

        [SetUp]
        public void SetUp()
        {
            model = new WaterFlowFMModel();
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingASingleWeirWhichDoesNotContainCrestLevelTimeSeriesValuesWhileUsingCrestLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Structure()
            {
                Formula = new SimpleWeirFormula(),
                UseCrestLevelTimeSeries = true,
                CrestWidth = 1.0
            };
            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"'{weir.Name}': crest level time series does not contain any values.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingASingleWeirWhereTheCrestLevelTimeSeriesValuesAreSmallerThanTheModelTimeSeriesWhileUsingCrestLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Structure()
            {
                Formula = new SimpleWeirFormula(),
                UseCrestLevelTimeSeries = true,
                CrestWidth = 1.0
            };

            DateTime t = DateTime.Today;
            weir.CrestLevelTimeSeries[t.AddDays(1)] = 100.0;
            weir.CrestLevelTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;

            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"'{weir.Name}': crest level time series does not span the model run interval.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingASingleWeirWithANegativeLateralContractionWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Structure()
            {
                Formula = new SimpleWeirFormula {LateralContraction = -1.0},
                CrestWidth = 1.0
            };

            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"'{weir.Name}': lateral contraction coefficient must be greater than or equal to zero.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWithANegativeGateHeightWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Structure()
            {
                Formula = new SimpleGateFormula {GateHeight = -1.0},
                CrestWidth = 1.0
            };

            model.Area.Structures.Add(weir);

            // When 
            //   Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': gate height must be greater than or equal to 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhereTheOpeningWidthTimeSeriesContainsNegativeValuesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new SimpleGateFormula(true) {UseHorizontalGateOpeningWidthTimeSeries = true};
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };

            DateTime t = DateTime.Today;
            formula.HorizontalGateOpeningWidthTimeSeries[t] = -100.0;
            formula.HorizontalGateOpeningWidthTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;
            model.StopTime = t.AddDays(7);

            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"'{weir.Name}': gate opening width time series values must be greater than or equal to 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhereTheOpeningWidthTimeSeriesValuesAreSmallerThanTheModelTimeSeriesWhileUsingOpeningWidthTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new SimpleGateFormula(true) {UseHorizontalGateOpeningWidthTimeSeries = true};
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };

            DateTime t = DateTime.Today;
            formula.HorizontalGateOpeningWidthTimeSeries[t.AddDays(1)] = 100.0;
            formula.HorizontalGateOpeningWidthTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;

            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': gate opening width time series does not span the model run interval.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhichDoesNotContainOpeningWidthTimeSeriesValuesWhileUsingOpeningWidthTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new SimpleGateFormula(true) {UseHorizontalGateOpeningWidthTimeSeries = true};
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': gate opening width time series does not contain any values.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWithANegativeConstantOpeningWidthWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new SimpleGateFormula(true) {HorizontalGateOpeningWidth = -1.0};
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': gate opening width must be greater than or equal to 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhereTheGateLowerEdgeLevelTimeSeriesValuesAreSmallerThanTheModelTimeSeriesWhileUsingGateLowerEdgeLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new SimpleGateFormula(true) {UseGateLowerEdgeLevelTimeSeries = true};
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };

            DateTime t = DateTime.Today;
            formula.GateLowerEdgeLevelTimeSeries[t.AddDays(1)] = 100.0;
            formula.GateLowerEdgeLevelTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;

            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': gate lower edge level time series does not span the model run interval.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhichDoesNotContainLowerEdgeLevelTimeSeriesValuesWhileUsingLowerEdgeLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new SimpleGateFormula(true) {UseGateLowerEdgeLevelTimeSeries = true};
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': gate lower edge level time series does not contain any values.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingASingleWeirWhichHasACrestWidthOfZero_WhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Structure()
            {
                Formula = new SimpleWeirFormula(),
                UseCrestLevelTimeSeries = false,
                CrestWidth = 0.0
            };
            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"Crest Width for '{weir.Name}' structure type: {weir.Formula.Name}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANonSymmetricGateOpeningDirectionWhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureFormula()
            {
                GateOpeningHorizontalDirection = GateOpeningDirection.FromLeft,
                Upstream2Width = 1.0,
                Downstream1Width = 1.0,
                Upstream1Width = 1.0,
                Downstream2Width = 1.0
            };
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': only symmetric gate opening horizontal direction is supported for general structures.";
            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANegativeUpstream2WhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureFormula()
            {
                GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric,
                Upstream2Width = -1.0,
                Downstream1Width = 1.0,
                Upstream1Width = 1.0,
                Downstream2Width = 1.0
            };
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"Upstream 2 Width for '{weir.Name}' structure type: {weir.Formula.Name}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANegativeUpstream1WhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureFormula()
            {
                GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric,
                Upstream2Width = 1.0,
                Downstream1Width = 1.0,
                Upstream1Width = -1.0,
                Downstream2Width = 1.0
            };
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"Upstream 1 Width for '{weir.Name}' structure type: {weir.Formula.Name}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANegativeDownstream1WhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureFormula()
            {
                GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric,
                Upstream2Width = 1.0,
                Downstream1Width = -1.0,
                Upstream1Width = 1.0,
                Downstream2Width = 1.0
            };
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"Downstream 1 Width for '{weir.Name}' structure type: {weir.Formula.Name}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANegativeDownstream2WhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureFormula()
            {
                GateOpeningHorizontalDirection = GateOpeningDirection.Symmetric,
                Upstream2Width = 1.0,
                Downstream1Width = 1.0,
                Upstream1Width = 1.0,
                Downstream2Width = -1.0
            };
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Structures.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"Downstream 2 Width for '{weir.Name}' structure type: {weir.Formula.Name}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }
    }
}