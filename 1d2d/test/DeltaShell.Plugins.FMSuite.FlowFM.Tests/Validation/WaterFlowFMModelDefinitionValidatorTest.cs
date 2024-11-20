using System;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
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
        public void ValidateZeroTimeSteps()
        {
            var model = CreateSimpleModel();
            model.TimeStep = new TimeSpan(0, 0, 0, 0);

            var validationReport = model.Validate();

            Assert.AreEqual(2, validationReport.ErrorCount);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void DoNotValidateCalculationTimeStep()
        {
            var model = CreateSimpleModel();
            model.StartTime = new DateTime(2000, 1, 1);
            model.StopTime = new DateTime(2000, 1, 2);
            model.TimeStep = new TimeSpan(0, 0, 1, 0);

            var validationReport = model.Validate();

            Assert.AreEqual(0, validationReport.ErrorCount);
        }

        [Test]
        public void ValidateValidTimeSteps()
        {
            var model = CreateSimpleModel();
            model.StartTime = new DateTime(2000, 1, 1);
            model.StopTime = new DateTime(2000, 1, 2);
            model.TimeStep = new TimeSpan(0, 0, 1, 0);
            model.OutputTimeStep = new TimeSpan(0, 2, 0, 0);

            var validationReport = model.Validate();

            Assert.AreEqual(0, validationReport.ErrorCount);
        }

        [Test]
        public void ValidateDefaults()
        {
            var model = CreateValidModel();

            var issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(0, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);
        }

        [Test]
        public void Teta0OutOfRangeYieldsValidationError()
        {
            var model = CreateValidModel();

            var modelDefinition = model.ModelDefinition;
            modelDefinition.GetModelProperty("teta0").SetValueFromString("1.1");
            var issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);
        }

        [Test]
        public void BedLevelTypeNotEqualToCellsWithMorphologyValidationError()
        {
            var model = CreateValidModel();
            var modelDefinition = model.ModelDefinition;
            modelDefinition.GetModelProperty(GuiProperties.UseMorSed).SetValueFromString("true");

            // CellEdges
            var facesValue = ((int)BedLevelLocation.CellEdges).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(facesValue);
            var issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);

            // NodesMaxLev
            var nodesMaxLevValue = ((int)BedLevelLocation.NodesMaxLev).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(nodesMaxLevValue);
            issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);

            // FacesMeanLevFromNodes
            var nodesMaxLevAtFacesValue = ((int)BedLevelLocation.FacesMeanLevFromNodes).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(nodesMaxLevAtFacesValue);
            issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);

            // NodesMeanLev
            var nodesMeanLevValue = ((int)BedLevelLocation.NodesMeanLev).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(nodesMeanLevValue);
            issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);

            // NodesMinLev
            var nodesMinLevValue = ((int)BedLevelLocation.NodesMinLev).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(nodesMinLevValue);
            issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);
        }

        [Test]
        public void BedLevelTypeEqualToCellsWithMorphologyValidationError()
        {
            var model = CreateValidModel();
            var modelDefinition = model.ModelDefinition;
            var cellsValue = ((int)BedLevelLocation.Faces).ToString();
            modelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(cellsValue);
            modelDefinition.GetModelProperty(GuiProperties.UseMorSed).SetValueFromString("true");
            var issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(0, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);
        }

        static WaterFlowFMModel CreateSimpleModel()
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
                    {1, 2}, {2, 3}, {3, 4}, {4, 1}, {1, 3}
                };

            model.Grid = UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges);
            model.ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value = new DateOnly(2000, 1, 1);
            return model;
        }

        public static WaterFlowFMModel CreateValidModel()
        {
            var model = CreateSimpleModel();
            model.TimeStep = new TimeSpan(0, 0, 1, 0);
            model.StartTime = new DateTime(2000, 1, 1);
            model.StopTime = new DateTime(2000, 1, 2);
            model.OutputTimeStep = new TimeSpan(0, 0, 2, 0);
            return model;
        }

        [Test]
        [TestCase(Conveyance2DType.RisHU, false)]//R=HU
        [TestCase(Conveyance2DType.RisH, false)]//R=H
        [TestCase(Conveyance2DType.RisAperP, false)]//R=A/P
        [TestCase(Conveyance2DType.Kisanalytic1Dconv, true)]//K=analytic-1D conv
        [TestCase(Conveyance2DType.Kisanalytic2Dconv, true)]//K=analytic-2D conv
        public void CheckConveyance2DType(Conveyance2DType type, bool validationErrorThrown)
        {
            //please note the enum is validated with test LoadConveyance2dEnumAndVerifyThatItHasNotChanged
            var model = CreateValidModel();
            var sedFrac = new SedimentFraction() { Name = "Frac1" };
            model.SedimentFractions.Add(sedFrac);
            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).SetValueFromString("1");
            model.ModelDefinition.GetModelProperty(KnownProperties.Conveyance2d).SetValueFromString(((int)type).ToString());
            var report = model.Validate();
            var issues = string.Join(";", report.AllErrors.Where(e => e.Severity == ValidationSeverity.Error).Select(e => e.Message));
            Assert.That(issues.Contains(Resources.WaterFlowFMModelDefinitionValidator_Validate_), Is.EqualTo(validationErrorThrown));
        }
    }
}
