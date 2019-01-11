using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMapTestUtils;


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

        #region Thin dams

        [Test]
        public void GivenFmModelWithThinDamsThatDoNotIntersectWithModelGrid_WhenValidatingModelArea_ThenValidationWarningIsReturned()
        {
            // Given
            var fmModel = new WaterFlowFMModel
            {
                Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2)
            };
            var thinDam = new ThinDam2D
            {
                Name = "myThinDam",
                // Thin dam geometry is far outside of grid extent
                Geometry = new LineString(new[] {new Coordinate(10, 10), new Coordinate(20, 20) })
            };
            fmModel.Area.ThinDams.Add(thinDam);

            // When
            var validationReport = WaterFlowFMArea2DValidator.Validate(fmModel);

            // Then
            var validationWarnings = validationReport.GetAllIssuesRecursive().Where(issue => issue.Severity == ValidationSeverity.Warning).ToArray();
            Assert.That(validationWarnings.Length, Is.EqualTo(1));

            var expectedMessage = 
                string.Format(Resources.WaterFlowFMArea2DValidator_Validate_thin_dam___0___not_within_grid_extent, thinDam.Name);
            Assert.That(validationWarnings[0].Message, Is.EqualTo(expectedMessage));
        }

        #endregion

        [Test]
        public void GivenAWaterFlowFMModelContainingASingleWeirWhichDoesNotContainCrestLevelTimeSeriesValuesWhileUsingCrestLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Weir2D(true)
            {
                WeirFormula = new SimpleWeirFormula(),
                UseCrestLevelTimeSeries = true,
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);
            
            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"'{weir.Name}': crest level time series does not contain any values.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingASingleWeirWhereTheCrestLevelTimeSeriesValuesAreSmallerThanTheModelTimeSeriesWhileUsingCrestLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Weir2D(true)
            {
                WeirFormula = new SimpleWeirFormula(),
                UseCrestLevelTimeSeries = true,
                CrestWidth = 1.0
            };

            var t = DateTime.Today;
            weir.CrestLevelTimeSeries[t.AddDays(1)] = 100.0;
            weir.CrestLevelTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;

            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"'{weir.Name}': crest level time series does not span the model run interval.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingASingleWeirWithANegativeLateralContractionWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Weir2D(true)
            {
                WeirFormula = new SimpleWeirFormula { LateralContraction = -1.0 },
                CrestWidth = 1.0
            };

            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"'{weir.Name}': lateral contraction coefficient must be greater than or equal to zero.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWithANegativeDoorHeightWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Weir2D(true)
            {
                WeirFormula = new GatedWeirFormula { DoorHeight = -1.0 },
                CrestWidth = 1.0
            };

            model.Area.Weirs.Add(weir);

            // When 
            //   Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': door height must be greater than or equal to 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhereTheOpeningWidthTimeSeriesContainsNegativeValuesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new GatedWeirFormula(true) { UseHorizontalDoorOpeningWidthTimeSeries = true };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
                CrestWidth = 1.0
            };

            var t = DateTime.Today;
            formula.HorizontalDoorOpeningWidthTimeSeries[t] = -100.0;
            formula.HorizontalDoorOpeningWidthTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;
            model.StopTime = t.AddDays(7);

            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"'{weir.Name}': opening width time series values must be greater than or equal to 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }


        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhereTheOpeningWidthTimeSeriesValuesAreSmallerThanTheModelTimeSeriesWhileUsingOpeningWidthTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new GatedWeirFormula(true) {UseHorizontalDoorOpeningWidthTimeSeries = true};
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
                CrestWidth = 1.0
            };

            var t = DateTime.Today;
            formula.HorizontalDoorOpeningWidthTimeSeries[t.AddDays(1)] = 100.0;
            formula.HorizontalDoorOpeningWidthTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;

            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': opening width time series does not span the model run interval.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhichDoesNotContainOpeningWidthTimeSeriesValuesWhileUsingOpeningWidthTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new GatedWeirFormula(true) { UseHorizontalDoorOpeningWidthTimeSeries = true };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': opening width time series does not contain any values.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWithANegativeConstantOpeningWidthWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new GatedWeirFormula(true) { HorizontalDoorOpeningWidth = -1.0 };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': opening width must be greater than or equal to 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhereTheLowerEdgeLevelTimeSeriesValuesAreSmallerThanTheModelTimeSeriesWhileUsingLowerEdgeLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new GatedWeirFormula(true) { UseLowerEdgeLevelTimeSeries = true };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
                CrestWidth = 1.0
            };

            var t = DateTime.Today;
            formula.LowerEdgeLevelTimeSeries[t.AddDays(1)] = 100.0;
            formula.LowerEdgeLevelTimeSeries[t.AddDays(7)] = 700.0;

            model.StartTime = t;

            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': lower edge level time series does not span the model run interval.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingAGatedWeirWhichDoesNotContainLowerEdgeLevelTimeSeriesValuesWhileUsingLowerEdgeLevelTimeSeriesWhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var formula = new GatedWeirFormula(true) { UseLowerEdgeLevelTimeSeries = true };
            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': lower edge level time series does not contain any values.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
        }

        [Test]
        public void GivenAWaterFlowFMModelContainingASingleWeirWhichHasACrestWidthOfZero_WhenValidateIsCalledThenTheCorrectIssueIsAdded()
        {
            // Given
            var weir = new Weir2D(true)
            {
                WeirFormula = new SimpleWeirFormula(),
                UseCrestLevelTimeSeries = false,
                CrestWidth = 0.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"Crest Width for '{weir.Name}' structure type: {weir.WeirFormula.GetName2D()}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
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
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue = $"'{weir.Name}': only symmetric horizontal door opening direction is supported for general structures.";
            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
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
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"Upstream 2 Width for '{weir.Name}' structure type: {weir.WeirFormula.GetName2D()}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
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
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"Upstream 1 Width for '{weir.Name}' structure type: {weir.WeirFormula.GetName2D()}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
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
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"Downstream 1 Width for '{weir.Name}' structure type: {weir.WeirFormula.GetName2D()}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
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
                CrestWidth = 1.0
            };
            model.Area.Weirs.Add(weir);

            // When 
            // Validate is called
            var validationReport = WaterFlowFMArea2DValidator.Validate(model);

            // Then
            // The correct issues are added.
            var expectedIssue =
                $"Downstream 2 Width for '{weir.Name}' structure type: {weir.WeirFormula.GetName2D()}, must be greater than 0.";

            Assert.That(validationReport.ContainsError(expectedIssue));
            var numberOfMessages = validationReport.ErrorCount + validationReport.WarningCount + validationReport.InfoCount;
            Assert.That(numberOfMessages, Is.EqualTo(1));
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
                WidthStructureLeftSide    = validUpstream2 ? 1.0 : -1.0,
                WidthLeftSideOfStructure  = validUpstream1 ? 1.0 : -1.0,
                WidthStructureRightSide   = validDownstream1 ? 1.0 : -1.0,
                WidthRightSideOfStructure = validDownstream2 ? 1.0 : -1.0,
            };

            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
                CrestWidth = validCrestWidth ? 1.0 : -1.0,
            };

            model.Area.Weirs.Add(weir);

            // When 
            var validationIssues = WaterFlowFMArea2DValidator.ValidateWeirs(model, model.Area.Weirs).ToList();

            // Then
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, "Crest",        weir, validCrestWidth );
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, "Upstream 2",   weir, validUpstream2  );
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, "Upstream 1",   weir, validUpstream1  );
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, "Downstream 1", weir, validDownstream1);
            AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(validationIssues, "Downstream 2", weir, validDownstream2);

            var nExpectedMessages = GetNumberOfExpectedMessagesInvalid(new bool[5]
            {
                validCrestWidth, validUpstream2, validUpstream1, validDownstream1, validDownstream2
            });

            Assert.That(validationIssues.Count, Is.EqualTo(nExpectedMessages));
        }

        private static void AssertThatValidationErrorIssueOnlyExistsInIssuesIfNotValid(IEnumerable<ValidationIssue> issues
                                                                         , string propertyName
                                                                         , Weir2D weir
                                                                         , bool isValid)
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
                WidthStructureLeftSide    = emptyUpstream2   ? double.NaN : 1.0,
                WidthLeftSideOfStructure  = emptyUpstream1   ? double.NaN : 1.0,
                WidthStructureRightSide   = emptyDownstream1 ? double.NaN : 1.0,
                WidthRightSideOfStructure = emptyDownstream2 ? double.NaN : 1.0,
            };

            var weir = new Weir2D(true)
            {
                WeirFormula = formula,
                CrestWidth = emptyCrestWidth ? double.NaN : 1.0,
            };

            model.Area.Weirs.Add(weir);

            // When 
            var validationIssues = WaterFlowFMArea2DValidator.ValidateWeirs(model, model.Area.Weirs).ToList();

            // Then
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, "Crest",        weir, emptyCrestWidth);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, "Upstream 2",   weir, emptyUpstream2);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, "Upstream 1",   weir, emptyUpstream1);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, "Downstream 1", weir, emptyDownstream1);
            AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(validationIssues, "Downstream 2", weir, emptyDownstream2);

            var nExpectedMessages = GetNumberOfExpectedMessagesEmpty(new bool[5]
            {
                emptyCrestWidth, emptyUpstream2, emptyUpstream1, emptyDownstream1, emptyDownstream2
            });

            Assert.That(validationIssues.Count, Is.EqualTo(nExpectedMessages));
        }

        private static void AssertThatValidationInfoIssueOnlyExistsInIssuesIfEmpty(IEnumerable<ValidationIssue> issues
                                                                                  , string propertyName
                                                                                  , Weir2D weir
                                                                                  , bool isEmpty)
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
