using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMModelDefinitionValidatorTest
    {
        [Test]
        public void Validate_ModelNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => WaterFlowFMModelDefinitionValidator.Validate(null);
            
            // Assert
            Assert.That(Call, Throws.ArgumentNullException);    
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void DoNotValidateCalculationTimeStep()
        {
            WaterFlowFMModel model = CreateSimpleModel();
            model.StartTime = new DateTime(2000, 1, 1);
            model.StopTime = new DateTime(2000, 1, 2);
            model.TimeStep = new TimeSpan(0, 0, 1, 0);

            ValidationReport validationReport = model.Validate();

            Assert.AreEqual(0, validationReport.ErrorCount);
        }

        [Test]
        public void ValidateValidTimeSteps()
        {
            WaterFlowFMModel model = CreateSimpleModel();
            model.StartTime = new DateTime(2000, 1, 1);
            model.StopTime = new DateTime(2000, 1, 2);
            model.TimeStep = new TimeSpan(0, 0, 1, 0);
            model.OutputTimeStep = new TimeSpan(0, 2, 0, 0);

            ValidationReport validationReport = model.Validate();

            Assert.AreEqual(0, validationReport.ErrorCount);
        }

        [Test]
        public void ValidateDefaults()
        {
            WaterFlowFMModel model = CreateValidModel();

            ValidationReport issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(0, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);
        }

        [Test]
        [Ignore("Ignored.")]                   // no priority
        [Category(TestCategory.WorkInProgress)] // See TOOLS-20091
        public void Conveyance2DOutOfRangeYieldsValidationError()
        {
            WaterFlowFMModel model = CreateValidModel();

            WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
            modelDefinition.GetModelProperty(KnownProperties.Conveyance2d).SetValueFromString("4"); // This method now throws. For TOOLS-20091 this should not happen any more.
            ValidationReport issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);
        }

        [Test]
        public void Teta0OutOfRangeYieldsValidationError()
        {
            WaterFlowFMModel model = CreateValidModel();

            WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
            modelDefinition.GetModelProperty("teta0").SetValueFromString("1.1");
            ValidationReport issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);
        }

        [Test]
        public void BedLevelTypeNotEqualToCellsWithMorphologyValidationError()
        {
            WaterFlowFMModel model = CreateValidModel();
            WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
            modelDefinition.GetModelProperty(GuiProperties.UseMorSed).SetValueFromString("true");

            // CellEdges
            var facesValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.CellEdges).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(facesValue);
            ValidationReport issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);

            // NodesMaxLev
            var nodesMaxLevValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(nodesMaxLevValue);
            issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);

            // FacesMeanLevFromNodes
            var nodesMaxLevAtFacesValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(nodesMaxLevAtFacesValue);
            issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);

            // NodesMeanLev
            var nodesMeanLevValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(nodesMeanLevValue);
            issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);

            // NodesMinLev
            var nodesMinLevValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(nodesMinLevValue);
            issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);
        }

        [Test]
        public void BedLevelTypeEqualToCellsWithMorphologyValidationError()
        {
            WaterFlowFMModel model = CreateValidModel();
            WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
            var cellsValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.Faces).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(cellsValue);
            modelDefinition.GetModelProperty(GuiProperties.UseMorSed).SetValueFromString("true");
            ValidationReport issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(0, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);
        }

        [Test]
        [TestCase(Conveyance2DType.RisHU, false)]            //R=HU
        [TestCase(Conveyance2DType.RisH, false)]             //R=H
        [TestCase(Conveyance2DType.RisAperP, false)]         //R=A/P
        [TestCase(Conveyance2DType.Kisanalytic1Dconv, true)] //K=analytic-1D conv
        [TestCase(Conveyance2DType.Kisanalytic2Dconv, true)] //K=analytic-2D conv
        public void CheckConveyance2DType(Conveyance2DType type, bool validationErrorThrown)
        {
            //please note the enum is validated with test LoadConveyance2dEnumAndVerifyThatItHasNotChanged
            WaterFlowFMModel model = CreateValidModel();
            var sedFrac = new SedimentFraction() {Name = "Frac1"};
            model.SedimentFractions.Add(sedFrac);
            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).SetValueFromString("1");
            model.ModelDefinition.GetModelProperty(KnownProperties.Conveyance2d).SetValueFromString(((int) type).ToString());
            ValidationReport report = model.Validate();
            string issues = string.Join(";", report.AllErrors.Where(e => e.Severity == ValidationSeverity.Error).Select(e => e.Message));
            Assert.That(issues.Contains(Resources.WaterFlowFMModelDefinitionValidator_Validate_), Is.EqualTo(validationErrorThrown));
        }

        [Test]
        public void GivenFmModelWithUseSedFileButNoBedLevelThenAddsIssueAsExpected()
        {
            // 1. Set up initial test data
            var fmModel = new WaterFlowFMModel();
            string expectedErrMessage = Resources
                .WaterFlowFMModelDefinitionValidator_Validate_Bed_level_locations_should_be_set_to__faces__when_morphology_is_active_;
            var expectedTabName = "Physical Parameters";
            object expectedSubject = fmModel;
            // 2. Verify initial expectations
            Assert.That(fmModel.ModelDefinition, Is.Not.Null);
            fmModel.ModelDefinition.UseMorphologySediment = true;

            // 3. Run test
            ValidationReport testReport = WaterFlowFMModelDefinitionValidator.Validate(fmModel);

            // 4. Verify final expectations
            Assert.That(testReport, Is.Not.Null);

            List<ValidationIssue> issues = testReport.AllErrors.ToList();
            Assert.That(issues.Any(), Is.True);

            ValidationIssue issueFound = issues.FirstOrDefault(iss => iss.Message.Equals(expectedErrMessage));
            Assert.That(issueFound, Is.Not.Null);
            Assert.That(issueFound.Subject, Is.EqualTo(expectedSubject));

            var issueViewData = issueFound.ViewData as FmValidationShortcut;
            Assert.That(issueViewData, Is.Not.Null);
            Assert.That(issueViewData.FlowFmModel, Is.EqualTo(fmModel));
            Assert.That(issueViewData.TabName, Is.EqualTo(expectedTabName));
        }

        [Test]
        public void FmModelWithValid3DLayerProperties_DoesNotAddValidationIssueToReport()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
                
                // Enable all 3D modelling properties
                SetKmxToOne(modelDefinition);
                SetLayerTypeToAllZ(modelDefinition);

                SetModelPropertyAndAssertIsEnabled(KnownProperties.DzTop, "2", modelDefinition);
                SetModelPropertyAndAssertIsEnabled(KnownProperties.FloorLevTopLay, "-2", modelDefinition);
                SetModelPropertyAndAssertIsEnabled(KnownProperties.DzTopUniAboveZ, "-2", modelDefinition);
                SetModelPropertyAndAssertIsEnabled(KnownProperties.SigmaGrowthFactor, "2", modelDefinition);
                SetModelPropertyAndAssertIsEnabled(KnownProperties.NumTopSig, "1", modelDefinition);
                SetModelPropertyAndAssertIsEnabled(KnownProperties.NumTopSigUniform, "1", modelDefinition);

                // Call
                ValidationReport report = WaterFlowFMModelDefinitionValidator.Validate(model);
                
                // Assert
                Assert.That(report.ErrorCount, Is.Zero);
            }            
        }

        private static void SetModelPropertyAndAssertIsEnabled(string propertyName, string valueAsString, WaterFlowFMModelDefinition modelDefinition)
        {
            WaterFlowFMProperty property = modelDefinition.GetModelProperty(propertyName);
            property.SetValueFromString(valueAsString);

            Assert.That(property.IsEnabled(modelDefinition.Properties));
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidKmxPropertyCases))]
        public void GivenAnFmModelWithInvalidKmxProperty_AddsValidationIssueToReport(string invalidValue)
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                // Set property to invalid value
                model.ModelDefinition.GetModelProperty(KnownProperties.Kmx).SetValueFromString(invalidValue);
                
                // Call
                ValidationReport report = WaterFlowFMModelDefinitionValidator.Validate(model);
                
                // Assert
                const string expectedErrorMessage = "Parameter Kmx outside validity range  [0,99].";
                ValidationIssue issue = report.AllErrors.FirstOrDefault(i => i.Message.Equals(expectedErrorMessage));
                Assert.That(issue, Is.Not.Null);
                Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Error));
            }
        }

        private static IEnumerable<TestCaseData> GetInvalidKmxPropertyCases()
        {
            yield return new TestCaseData("-1").SetName("Invalid Kmx value: lower than 0.");
            yield return new TestCaseData("100").SetName("Invalid Kmx value: higher than 99.");
        }
        
        [Test]
        [TestCaseSource(nameof(GetInvalid3DLayerPropertyCases))]
        public void FmModelWithInvalid3DLayerProperty_AddsValidationIssueToReportAndSetsCustomViewData(string propertyName,
                                                                                                       string invalidValue,
                                                                                                       string expectedErrorMessage)
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
                
                // Enable the property
                SetKmxToOne(modelDefinition);
                SetLayerTypeToAllZ(modelDefinition);

                SetModelPropertyAndAssertIsEnabled(propertyName, invalidValue, modelDefinition);
                
                // Call
                ValidationReport report = WaterFlowFMModelDefinitionValidator.Validate(model);
                
                // Assert
                ValidationIssue issue = report.AllErrors.FirstOrDefault(i => i.Message.Equals(expectedErrorMessage));
                Assert.That(issue, Is.Not.Null);
                Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Error));
                
                AssertThatIssueHasCustomViewData(issue, model);
            }
        }

        private static IEnumerable<TestCaseData> GetInvalid3DLayerPropertyCases()
        {
            yield return new TestCaseData(KnownProperties.DzTop, "-0.001", "Parameter DzTop should be > 0.00.")
                .SetName("Invalid DzTop value.");
            yield return new TestCaseData(KnownProperties.FloorLevTopLay, "0.001", "Parameter FloorLevTopLay should be < 0.00.")
                .SetName("Invalid FloorLevToPlay value.");
            yield return new TestCaseData(KnownProperties.DzTopUniAboveZ, "0.001", "Parameter DzTopUniAboveZ should be < 0.00.")
                .SetName("Invalid DzTopUniAboveZ value.");
        }
        
        [Test]
        public void GivenAnFmModelWithInvalidSigmaGrowthFactor_AddsValidationIssueToReport()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
                
                // Enable the property
                SetKmxToOne(modelDefinition);
                SetLayerTypeToAllZ(modelDefinition);
                
                SetModelPropertyAndAssertIsEnabled(KnownProperties.SigmaGrowthFactor, "-1", modelDefinition);

                // Call
                ValidationReport report = WaterFlowFMModelDefinitionValidator.Validate(model);

                // Assert
                const string expectedErrorMessage = "Parameter SigmaGrowthFactor outside validity range  [1,+inf].";
                ValidationIssue issue = report.AllErrors.FirstOrDefault(i => i.Message.Equals(expectedErrorMessage));
                Assert.That(issue, Is.Not.Null);
                Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Error));
            }
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidNumTopSigPropertyCases))]
        public void GivenAnFmModelWithInvalidNumTopSigProperty_AddsValidationIssueToReportAndSetsCustomViewData(string invalidValue)
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
                
                // Enable the property
                SetKmxToOne(modelDefinition);
                SetLayerTypeToAllZ(modelDefinition);

                // Set property to invalid value
                SetModelPropertyAndAssertIsEnabled(KnownProperties.NumTopSig, invalidValue, modelDefinition);
                
                // Call
                ValidationReport report = WaterFlowFMModelDefinitionValidator.Validate(model);
                
                // Assert
                const string expectedErrorMessage = "Parameter numtopsig should be between 0 and 1 (the current value of kmx).";
                ValidationIssue issue = report.AllErrors.FirstOrDefault(i => i.Message.Equals(expectedErrorMessage));
                Assert.That(issue, Is.Not.Null);
                Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Error));
                
                AssertThatIssueHasCustomViewData(issue, model);
            }
        }

        private static IEnumerable<TestCaseData> GetInvalidNumTopSigPropertyCases()
        {
            yield return new TestCaseData("-1").SetName("Invalid NumTopSig: value smaller than 0.");
            yield return new TestCaseData("100").SetName("Invalid NumTopSig: value larger than Kmx.");
        }

        [Test]
        public void GivenAnFmModelWithInvalidNumTopSigUniformProperty_AddsValidationIssueToReport()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
                
                // Ensure the property
                SetKmxToOne(modelDefinition);
                SetLayerTypeToAllZ(modelDefinition);

                // Set property to invalid value
                SetModelPropertyAndAssertIsEnabled(KnownProperties.NumTopSigUniform, "2", modelDefinition);
                
                // Call
                ValidationReport report = WaterFlowFMModelDefinitionValidator.Validate(model);
                
                // Assert
                const string expectedErrorMessage = "Parameter NumTopSigUniform outside validity range  [0,1].";
                ValidationIssue issue = report.AllErrors.FirstOrDefault(i => i.Message.Equals(expectedErrorMessage));
                Assert.That(issue, Is.Not.Null);
                Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Error));
            }
        }

        private static void AssertThatIssueHasCustomViewData(ValidationIssue issue, WaterFlowFMModel model)
        {
            var viewData = (FmValidationShortcut)issue.ViewData;
            Assert.That(viewData.FlowFmModel, Is.EqualTo(model));
            const string expectedTabName = "3D Layers";
            Assert.That(viewData.TabName, Is.EqualTo(expectedTabName));
        }

        private static void SetKmxToOne(WaterFlowFMModelDefinition modelModelDefinition)
        {
            modelModelDefinition.GetModelProperty(KnownProperties.Kmx).SetValueFromString("1");
        }
        
        private static void SetLayerTypeToAllZ(WaterFlowFMModelDefinition modelModelDefinition)
        {
            modelModelDefinition.GetModelProperty(KnownProperties.LayerType).SetValueFromString("2"); // 2 = all-z
        }

        private static WaterFlowFMModel CreateSimpleModel()
        {
            var model = new WaterFlowFMModel();
            var vertices = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 10),
                new Coordinate(10, 10),
                new Coordinate(10, 0)
            };

            var edges = new int[,]
            {
                {
                    1,
                    2
                },
                {
                    2,
                    3
                },
                {
                    3,
                    4
                },
                {
                    4,
                    1
                },
                {
                    1,
                    3
                }
            };

            model.Grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges);
            model.ReferenceTime = new DateTime(2000, 1, 1);
            return model;
        }

        public static WaterFlowFMModel CreateValidModel()
        {
            WaterFlowFMModel model = CreateSimpleModel();
            model.TimeStep = new TimeSpan(0, 0, 1, 0);
            model.StartTime = new DateTime(2000, 1, 1);
            model.StopTime = new DateTime(2000, 1, 2);
            model.OutputTimeStep = new TimeSpan(0, 0, 2, 0);
            return model;
        }
    }
}