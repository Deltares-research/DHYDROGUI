using System;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFmModelValidationExtensionsTest
    {
        [Test]
        public void CheckCoordinateSystemInfoMessageIsGivenIfNoCoordinateSystemIsSpecified()
        {
            var model = new WaterFlowFMModel();

            var report = model.Validate(model);

            Assert.AreEqual(1,
                report.GetAllIssuesRecursive()
                    .Count(i => i.Severity == ValidationSeverity.Info && i.Message.Contains("coordinate system")));
        }

        [Test]
        public void CheckPumpCapacityIsNotNegative()
        {
            var model = new WaterFlowFMModel();
            model.Area.Pumps.Add(new Pump("A", true){ Capacity = -1.2, Branch = null});

            var report = model.Validate(model);

            Assert.AreEqual(1,
                            report.GetAllIssuesRecursive()
                                  .Count(i =>
                                         i.Severity == ValidationSeverity.Error &&
                                         i.Message.Contains("pump 'A': Capacity must be greater than or equal to 0.")));
        }

        [Test]
        public void CheckPumpCapacityTimeSeriesIsNotNegative()
        {
            var model = new WaterFlowFMModel();
            var pump = new Pump("A", true) {Branch = null, UseCapacityTimeSeries = true};
            pump.CapacityTimeSeries[new DateTime(2000, 1, 2)] = -1.2;
            model.Area.Pumps.Add(pump);

            var report = model.Validate(model);

            var issues = report.GetAllIssuesRecursive();
            Assert.AreEqual(1, issues.Count(i =>
                i.Severity == ValidationSeverity.Error &&
                i.Message.Contains("pump 'A': capacity time series values must be greater than or equal to 0.")));
            Assert.AreEqual(1, issues.Count(i =>
                i.Severity == ValidationSeverity.Error &&
                i.Message.Contains("pump 'A': capacity time series does not span the model run interval.")));
        }

        [Test]
        public void CheckPumpSuctionAndDeliverySideControl()
        {
            var model = new WaterFlowFMModel();
            var pump = new Pump("A", true) { 
                Branch = null, ControlDirection = PumpControlDirection.SuctionAndDeliverySideControl,
                StartDelivery = 1.2, StopDelivery = -1.2,
                StartSuction = -1.2, StopSuction = 1.2
            };
            model.Area.Pumps.Add(pump);

            var report = model.Validate(model);

            Assert.AreEqual(1,
                            report.GetAllIssuesRecursive()
                                  .Count(i =>
                                         i.Severity == ValidationSeverity.Error &&
                                         i.Message.Contains("pump 'A': Delivery start level must be less than or equal to delivery stop level.")));
            Assert.AreEqual(1,
                            report.GetAllIssuesRecursive()
                                  .Count(i =>
                                         i.Severity == ValidationSeverity.Error &&
                                         i.Message.Contains("pump 'A': Suction start level must be greater than or equal to suction stop level.")));
        }

        [Test]
        public void CheckCoordinateSystemValidation()
        {
            var model = new WaterFlowFMModel {CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(3824)};

            var report = model.Validate(model);

            Assert.AreEqual(1,
                report.GetAllIssuesRecursive()
                    .Count(i => i.Severity == ValidationSeverity.Warning && i.Message.Contains("coordinate system")));
        }

        [Test]
        public void CheckSolverTypeValidation()
        {
            var model = new WaterFlowFMModel { CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(3824) };
            model.ModelDefinition.GetModelProperty(KnownProperties.SolverType).SetValueAsString("7");

            var report = model.Validate(model);

            Assert.AreEqual(1,
                report.GetAllIssuesRecursive()
                    .Count(i => i.Severity == ValidationSeverity.Error && i.Message.Contains("parallel run")));
        }

        [Test]
        public void ValidateRestartInputReportTestRestartIsEmpty()
        {
            var model = new WaterFlowFMModel();
            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            model.UseRestart = true;

            var report = model.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.That(report.AllErrors.First(i => i.Severity == ValidationSeverity.Error).Message,
                Is.EqualTo("Input restart state is empty; cannot restart."));
            
        }
    }
}
