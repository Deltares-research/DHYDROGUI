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
            modelDefinition.GetModelProperty(KnownProperties.Conveyance2d).SetValueAsString("4"); // This method now throws. For TOOLS-20091 this should not happen any more.
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
            modelDefinition.GetModelProperty("teta0").SetValueAsString("1.1");
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
            modelDefinition.GetModelProperty(GuiProperties.UseMorSed).SetValueAsString("true");

            // CellEdges
            var facesValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.CellEdges).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueAsString(facesValue);
            ValidationReport issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);

            // NodesMaxLev
            var nodesMaxLevValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueAsString(nodesMaxLevValue);
            issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);

            // FacesMeanLevFromNodes
            var nodesMaxLevAtFacesValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueAsString(nodesMaxLevAtFacesValue);
            issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);

            // NodesMeanLev
            var nodesMeanLevValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueAsString(nodesMeanLevValue);
            issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);

            // NodesMinLev
            var nodesMinLevValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueAsString(nodesMinLevValue);
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
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueAsString(cellsValue);
            modelDefinition.GetModelProperty(GuiProperties.UseMorSed).SetValueAsString("true");
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
            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).SetValueAsString("1");
            model.ModelDefinition.GetModelProperty(KnownProperties.Conveyance2d).SetValueAsString(((int) type).ToString());
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