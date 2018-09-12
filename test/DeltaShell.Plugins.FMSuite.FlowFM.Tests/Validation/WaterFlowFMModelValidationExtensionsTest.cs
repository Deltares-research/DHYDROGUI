using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFmModelValidationExtensionsTest
    {
        [Test]
        public void CheckModelValidatesIfNoGridDefinedButNetworkIsValid()
        {
            var model = new WaterFlowFMModel();
            var reportErrors = WaterFlowFMGridValidator.Validate(model);

            Assert.AreEqual(1, reportErrors.ErrorCount);

            string expectedErrorMessage = Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_defined_;
            var errorFound = reportErrors.AllErrors.First();

            Assert.AreEqual(ValidationSeverity.Error, errorFound.Severity);
            Assert.AreEqual(expectedErrorMessage, errorFound.Message);

            WaterFlowFMTestHelper.ConfigureDemoNetwork(model.Network);
            var firstChannel = model.Network.Channels.First();
            model.NetworkDiscretization[new NetworkLocation(firstChannel, 0.2)] = 0.0;

            reportErrors = WaterFlowFMGridValidator.Validate(model);
            Assert.AreEqual(0, reportErrors.ErrorCount);

            model.Network = new HydroNetwork();
            reportErrors = WaterFlowFMGridValidator.Validate(model);
            Assert.AreEqual(1, reportErrors.ErrorCount);

            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            reportErrors = WaterFlowFMGridValidator.Validate(model);
            Assert.AreEqual(0, reportErrors.ErrorCount);
        }

        [Test]
        public void Check1D2DValidationCategoriesAreCreated()
        {
            var model = new WaterFlowFMModel();
            var report = model.Validate();

            Assert.NotNull(report.SubReports.First(r => r.Category.Equals(WaterFlowFMModelComputationalGridValidator.CategoryName)));
            Assert.NotNull(report.SubReports.First(r => r.Category.Equals(WaterFlowFMModelNetworkValidator.CategoryName)));
        }

        [Test]
        public void CheckCoordinateSystemInfoMessageIsGivenIfNoCoordinateSystemIsSpecified()
        {
            var model = new WaterFlowFMModel();

            var report = model.Validate();

            Assert.AreEqual(1,
                report.GetAllIssuesRecursive()
                    .Count(i => i.Severity == ValidationSeverity.Info && i.Message.Contains("coordinate system")));
        }

        [Test]
        public void CheckThatInitializingFmModelWithSalinityAndTemperatureEnabledNoWarningMessagesAreGiven()
        {

            var model = new WaterFlowFMModel();
            //Enable Salinity and Temperature checkboxes
            var salinityProperty = model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            var temperatureProperty = model.ModelDefinition.GetModelProperty(KnownProperties.Temperature);
            //Create a grid
            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            model.UseRestart = true;

            //Validate model
            var report = model.Validate();
            salinityProperty.Value = true;
            temperatureProperty.SetValueAsString("1");

            Assert.AreEqual(0,
                report.GetAllIssuesRecursive()
                    .Count(i => i.Severity == ValidationSeverity.Warning && i.Message.Contains("Initial")));
        }


        [Test]
        public void CheckPumpCapacityIsNotNegative()
        {
            var model = new WaterFlowFMModel();
            model.Area.Pumps.Add(new Pump2D("A", true){ Capacity = -1.2, Branch = null});

            var report = model.Validate();

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
            var pump = new Pump2D("A", true) {Branch = null, UseCapacityTimeSeries = true};
            pump.CapacityTimeSeries[new DateTime(2000, 1, 2)] = -1.2;
            model.Area.Pumps.Add(pump);

            var report = model.Validate();

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
            var pump = new Pump2D("A", true) { 
                Branch = null, ControlDirection = PumpControlDirection.SuctionAndDeliverySideControl,
                StartDelivery = 1.2, StopDelivery = -1.2,
                StartSuction = -1.2, StopSuction = 1.2
            };
            model.Area.Pumps.Add(pump);

            var report = model.Validate();

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

            var report = model.Validate();

            Assert.AreEqual(1,
                report.GetAllIssuesRecursive()
                    .Count(i => i.Severity == ValidationSeverity.Warning && i.Message.Contains("coordinate system")));
        }

        [Test]
        public void CheckSolverTypeValidation()
        {
            var model = new WaterFlowFMModel { CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(3824) };
            model.ModelDefinition.GetModelProperty(KnownProperties.SolverType).SetValueAsString("7");

            var report = model.Validate();

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

            var report = model.Validate();
            Assert.AreEqual(1, report.ErrorCount);
            Assert.That(report.AllErrors.First(i => i.Severity == ValidationSeverity.Error).Message,
                Is.EqualTo("Input restart state is empty; cannot restart."));
            
        }
    }
}
