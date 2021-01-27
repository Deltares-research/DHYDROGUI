using System;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
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
            model.Area.Weirs.Add(weir);

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

            model.Area.Weirs.Add(weir);

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

            model.Area.Weirs.Add(weir);

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
        public void GivenAWaterFlowFMModelContainingAGatedWeirWithANegativeDoorHeightWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Structure()
            {
                Formula = new SimpleGateFormula {DoorHeight = -1.0},
                CrestWidth = 1.0
            };

            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': door height must be greater than or equal to 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhereTheOpeningWidthTimeSeriesContainsNegativeValuesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new SimpleGateFormula(true) {UseHorizontalDoorOpeningWidthTimeSeries = true};
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };

            DateTime t = DateTime.Today;
            formula.HorizontalDoorOpeningWidthTimeSeries[t] = -100.0;
            formula.HorizontalDoorOpeningWidthTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;
            model.StopTime = t.AddDays(7);

            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"'{weir.Name}': opening width time series values must be greater than or equal to 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhereTheOpeningWidthTimeSeriesValuesAreSmallerThanTheModelTimeSeriesWhileUsingOpeningWidthTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new SimpleGateFormula(true) {UseHorizontalDoorOpeningWidthTimeSeries = true};
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };

            DateTime t = DateTime.Today;
            formula.HorizontalDoorOpeningWidthTimeSeries[t.AddDays(1)] = 100.0;
            formula.HorizontalDoorOpeningWidthTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;

            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': opening width time series does not span the model run interval.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhichDoesNotContainOpeningWidthTimeSeriesValuesWhileUsingOpeningWidthTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new SimpleGateFormula(true) {UseHorizontalDoorOpeningWidthTimeSeries = true};
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': opening width time series does not contain any values.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWithANegativeConstantOpeningWidthWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new SimpleGateFormula(true) {HorizontalDoorOpeningWidth = -1.0};
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': opening width must be greater than or equal to 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhereTheLowerEdgeLevelTimeSeriesValuesAreSmallerThanTheModelTimeSeriesWhileUsingLowerEdgeLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new SimpleGateFormula(true) {UseLowerEdgeLevelTimeSeries = true};
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };

            DateTime t = DateTime.Today;
            formula.LowerEdgeLevelTimeSeries[t.AddDays(1)] = 100.0;
            formula.LowerEdgeLevelTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;

            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': lower edge level time series does not span the model run interval.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhichDoesNotContainLowerEdgeLevelTimeSeriesValuesWhileUsingLowerEdgeLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new SimpleGateFormula(true) {UseLowerEdgeLevelTimeSeries = true};
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': lower edge level time series does not contain any values.";

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
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"Crest Width for '{weir.Name}' structure type: {weir.Formula.GetName2D()}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANonSymmetricDoorOpeningDirectionWhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureFormula()
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.FromLeft,
                WidthStructureLeftSide = 1.0,
                WidthStructureRightSide = 1.0,
                WidthLeftSideOfStructure = 1.0,
                WidthRightSideOfStructure = 1.0
            };
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': only symmetric horizontal door opening direction is supported for general structures.";
            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANegativeUpstream2WhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureFormula()
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                WidthStructureLeftSide = -1.0,
                WidthStructureRightSide = 1.0,
                WidthLeftSideOfStructure = 1.0,
                WidthRightSideOfStructure = 1.0
            };
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"Upstream 2 Width for '{weir.Name}' structure type: {weir.Formula.GetName2D()}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANegativeUpstream1WhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureFormula()
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                WidthStructureLeftSide = 1.0,
                WidthStructureRightSide = 1.0,
                WidthLeftSideOfStructure = -1.0,
                WidthRightSideOfStructure = 1.0
            };
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"Upstream 1 Width for '{weir.Name}' structure type: {weir.Formula.GetName2D()}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANegativeDownstream1WhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureFormula()
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                WidthStructureLeftSide = 1.0,
                WidthStructureRightSide = -1.0,
                WidthLeftSideOfStructure = 1.0,
                WidthRightSideOfStructure = 1.0
            };
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"Downstream 1 Width for '{weir.Name}' structure type: {weir.Formula.GetName2D()}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANegativeDownstream2WhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureFormula()
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                WidthStructureLeftSide = 1.0,
                WidthStructureRightSide = 1.0,
                WidthLeftSideOfStructure = 1.0,
                WidthRightSideOfStructure = -1.0
            };
            var weir = new Structure()
            {
                Formula = formula,
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            ValidationReport validationReport = FMStructuresValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"Downstream 2 Width for '{weir.Name}' structure type: {weir.Formula.GetName2D()}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            int numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }
    }
}