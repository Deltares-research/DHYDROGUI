using System;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
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

            Assert.AreEqual(4, validationReport.ErrorCount);
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
        [Ignore] // no priority
        [Category(TestCategory.WorkInProgress)] // See TOOLS-20091
        public void Conveyance2DOutOfRangeYieldsValidationError()
        {
            var model = CreateValidModel();

            var modelDefinition = model.ModelDefinition;
            modelDefinition.GetModelProperty("conveyance2d").SetValueAsString("4"); // This method now throws. For TOOLS-20091 this should not happen any more.
            var issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
            Assert.AreEqual(0, issues.WarningCount);
            Assert.AreEqual(0, issues.InfoCount);
        }

        [Test]
        public void Teta0OutOfRangeYieldsValidationError()
        {
            var model = CreateValidModel();

            var modelDefinition = model.ModelDefinition;
            modelDefinition.GetModelProperty("teta0").SetValueAsString("1.1");
            var issues = WaterFlowFMModelDefinitionValidator.Validate(model);

            Assert.AreEqual(1, issues.ErrorCount);
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
            model.ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value = new DateTime(2000, 1, 1);
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

    }
}
