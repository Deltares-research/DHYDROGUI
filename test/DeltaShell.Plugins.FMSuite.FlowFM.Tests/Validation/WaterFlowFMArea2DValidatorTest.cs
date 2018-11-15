using System;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    class WaterFlowFMArea2DValidatorTest
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
            var weir = new Weir2D(true)
            {
                WeirFormula = new SimpleWeirFormula(),
                UseCrestLevelTimeSeries = true,
            };
            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue = 
                String.Format("structure {0}: '{1}': crest level time series does not contain any values.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingASingleWeirWhereTheCrestLevelTimeSeriesValuesAreSmallerThanTheModelTimeSeriesWhileUsingCrestLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Weir2D(true)
            {
                WeirFormula = new SimpleWeirFormula(),
                UseCrestLevelTimeSeries = true,
            };

            var t = DateTime.Today;
            weir.CrestLevelTimeSeries[t.AddDays(1)] = 100.0;
            weir.CrestLevelTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;

            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': crest level time series does not span the model run interval.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingASingleWeirWithANegativeLateralContractionWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Weir2D(true)
            {
                WeirFormula = new SimpleWeirFormula() { LateralContraction = -1.0 },
            };

            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': lateral contraction coefficient must be greater than or equal to zero.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWithANegativeDoorHeightWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Weir2D(true)
            {
                WeirFormula = new GatedWeirFormula() { DoorHeight = -1.0 },
            };

            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': door height must be greater than or equal to 0.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhereTheOpeningWidthTimeSeriesContainsNegativeValuesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new GatedWeirFormula(true) { UseHorizontalDoorOpeningWidthTimeSeries = true };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
            };

            var t = DateTime.Today;
            formula.HorizontalDoorOpeningWidthTimeSeries[t] = -100.0;
            formula.HorizontalDoorOpeningWidthTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;
            model.StopTime = t.AddDays(7);

            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': opening width time series values must be greater than or equal to 0.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }


        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhereTheOpeningWidthTimeSeriesValuesAreSmallerThanTheModelTimeSeriesWhileUsingOpeningWidthTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new GatedWeirFormula(true) {UseHorizontalDoorOpeningWidthTimeSeries = true};
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
            };

            var t = DateTime.Today;
            formula.HorizontalDoorOpeningWidthTimeSeries[t.AddDays(1)] = 100.0;
            formula.HorizontalDoorOpeningWidthTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;

            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': opening width time series does not span the model run interval.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhichDoesNotContainOpeningWidthTimeSeriesValuesWhileUsingOpeningWidthTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new GatedWeirFormula(true) { UseHorizontalDoorOpeningWidthTimeSeries = true };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
            };
            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': opening width time series does not contain any values.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWithANegativeConstantOpeningWidthWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new GatedWeirFormula(true) { HorizontalDoorOpeningWidth = -1.0 };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
            };
            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': opening width must be greater than or equal to 0.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhereTheLowerEdgeLevelTimeSeriesValuesAreSmallerThanTheModelTimeSeriesWhileUsingLowerEdgeLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new GatedWeirFormula(true) { UseLowerEdgeLevelTimeSeries = true };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
            };

            var t = DateTime.Today;
            formula.LowerEdgeLevelTimeSeries[t.AddDays(1)] = 100.0;
            formula.LowerEdgeLevelTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;

            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': lower edge level time series does not span the model run interval.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhichDoesNotContainLowerEdgeLevelTimeSeriesValuesWhileUsingLowerEdgeLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new GatedWeirFormula(true) { UseLowerEdgeLevelTimeSeries = true };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
            };
            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': lower edge level time series does not contain any values.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANonSymmetricDoorOpeningDirectionWhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureWeirFormula()
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.FromLeft,
                WidthStructureLeftSide = 1.0,
                WidthStructureRightSide = 1.0,
                WidthLeftSideOfStructure = 1.0,
                WidthRightSideOfStructure = 1.0,
            };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
            };
            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': only symmetric horizontal door opening direction is supported for general structures.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANegativeUpstream2WhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureWeirFormula()
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                WidthStructureLeftSide = -1.0,
                WidthStructureRightSide = 1.0,
                WidthLeftSideOfStructure = 1.0,
                WidthRightSideOfStructure = 1.0,
            };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
            };
            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': Upstream 2 must be greater than 0.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANegativeUpstream1WhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureWeirFormula()
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                WidthStructureLeftSide = 1.0,
                WidthStructureRightSide = 1.0,
                WidthLeftSideOfStructure = -1.0,
                WidthRightSideOfStructure = 1.0,
            };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
            };
            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': Upstream 1 must be greater than 0.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANegativeDownstream1WhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureWeirFormula()
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                WidthStructureLeftSide = 1.0,
                WidthStructureRightSide = -1.0,
                WidthLeftSideOfStructure = 1.0,
                WidthRightSideOfStructure = 1.0,
            };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
            };
            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': Downstream 1 must be greater than 0.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGeneralStructureWithANegativeDownstream2WhenValidateIsCalledThenTheCorrectValiditionIssueIsReturned()
        {
            var formula = new GeneralStructureWeirFormula()
            {
                HorizontalDoorOpeningDirection = GateOpeningDirection.Symmetric,
                WidthStructureLeftSide = 1.0,
                WidthStructureRightSide = 1.0,
                WidthLeftSideOfStructure = 1.0,
                WidthRightSideOfStructure = -1.0,
            };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
            };
            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            //   The correct issues are added.
            var expectedIssue =
                String.Format("structure {0}: '{1}': Downstream 2 must be greater than 0.",
                    weir.WeirFormula.Name,
                    weir.Name);

            Assert.That(FlowFMTestHelper.ContainsError(validationReport, expectedIssue));
            var n_messages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(n_messages, Is.EqualTo(1));
        }
    }
}
