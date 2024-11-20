using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
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
            var reportErrors = ComputationalGridValidator.Validate(model.NetworkDiscretization,model.Grid,model.MinimumSegmentLength);

            Assert.AreEqual(1, reportErrors.ErrorCount);

            string expectedErrorMessage = Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_defined_;
            var errorFound = reportErrors.AllErrors.First();

            Assert.AreEqual(ValidationSeverity.Error, errorFound.Severity);
            Assert.AreEqual(expectedErrorMessage, errorFound.Message);

            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            reportErrors = ComputationalGridValidator.Validate(model.NetworkDiscretization, model.Grid, model.MinimumSegmentLength);
            Assert.AreEqual(0, reportErrors.ErrorCount);
        }

        [Test]
        public void ValidateDiscretizationWithADoubleCalcPointTest()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var discretization = new Discretization() { Network = network };
            var channel = network.Channels.First();
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                                                      true, 10.0, new List<IChannel> { channel });
            var discretizationLocations = discretization.Locations;
            var locations = discretizationLocations.Values;

            discretizationLocations.SkipUniqueValuesCheck = true;
            discretizationLocations.IsAutoSorted  = false;

            locations[1].Branch = locations[2].Branch;
            locations[1].Chainage = locations[2].Chainage;

            discretizationLocations.IsAutoSorted = true;
            discretizationLocations.SkipUniqueValuesCheck = false;

            var report = ComputationalGridValidator.Validate(discretization,null, 1);

            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.Error));
            Assert.That(report.AllErrors.Count(), Is.EqualTo(1));
            Assert.That(report.AllErrors.Select(i => i.Message), Contains.Item($"There are duplicate calculation points at same the location. Kernel cannot handle this. Please remove one of the points."));
        }

        [Test]
        public void Check1D2DValidationCategoriesAreCreated()
        {
            var model = new WaterFlowFMModel();
            var report = model.Validate();

            Assert.NotNull(report.SubReports.First(r => r.Category.Equals(ComputationalGridValidator.CategoryName)));
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
            
            //Validate model
            var report = model.Validate();
            salinityProperty.Value = true;
            temperatureProperty.SetValueFromString("1");

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
            model.ModelDefinition.GetModelProperty(KnownProperties.SolverType).SetValueFromString("7");

            var report = model.Validate();

            Assert.AreEqual(1,
                report.GetAllIssuesRecursive()
                    .Count(i => i.Severity == ValidationSeverity.Error && i.Message.Contains("parallel run")));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateWithSpaciallyVariantFullCoverage()
        {
            //Arrange
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"spatiallyVariantSediment\fullGridCoverage\FlowFM.mdu"));

            //Act
            var report = model.Validate();

            //Assert
            Assert.AreEqual(0,
                report.GetAllIssuesRecursive()
                    .Count(i => i.Severity == ValidationSeverity.Error && i.Message.Contains("SedimentThickness is not fully covering the grid, please cover entire grid")));
    }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateWithSpaciallyVariantPartialCoverage()
        {
            //Arrange
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"spatiallyVariantSediment\fullGridCoverage\FlowFM.mdu"));

            //Act
            var report = model.Validate();

            //Assert
            Assert.IsEmpty(report.AllErrors);
            Assert.AreEqual(report.Issues, new List<ValidationIssue>());
        }

        [Test]
        public void ModelBuildUpWithFullCoverage()
        {
            //Arrange
            var fmModel = new WaterFlowFMModel();
            fmModel.ModelDefinition.UseMorphologySediment = true;
            var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            fmModel.Grid = grid;
            var thickProp = new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 5, 0, false, 0, true, "cc", "mydoubledescription", true, false)
            {
                SpatiallyVaryingName = "mysedimentName_IniSedThick",
                Value = 12.3
            };
            thickProp.IsSpatiallyVarying = true;
           
            CreateSedimentFraction(thickProp, fmModel);

            var coverage = (UnstructuredGridCoverage) fmModel.DataItems.First(di => di.Name == thickProp.SpatiallyVaryingName).Value;
            coverage.SetValues(new [] {1.0,2.0,3.0,4.0});

            //Act
            var report = fmModel.Validate();
            var recursive = report.GetAllIssuesRecursive();
        
            //Assert
            Assert.IsFalse(recursive.Any(m => m.Message.Contains($"SedimentThickness {thickProp.SpatiallyVaryingName} is not fully covering the grid, please cover entire grid")));
        }

        [Test]
        public void ModelBuildUpWithPartialCoverage()
        {
            //Arrange
            //Arrange
            var fmModel = new WaterFlowFMModel();
            fmModel.ModelDefinition.UseMorphologySediment = true;
            var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            fmModel.Grid = grid;
            var thickProp = new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 5, 0, false, 0, true, "cc", "mydoubledescription", true, false)
            {
                SpatiallyVaryingName = "mysedimentName_IniSedThick",
                Value = 12.3
            };
            thickProp.IsSpatiallyVarying = true;

            CreateSedimentFraction(thickProp, fmModel);

            var coverage = (UnstructuredGridCoverage)fmModel.DataItems.First(di => di.Name == thickProp.SpatiallyVaryingName).Value;
            coverage.SetValues(new[] { -999.0, 2.0, 3.0, 4.0 });

            //Act
            var report = fmModel.Validate();
            var recursive = report.GetAllIssuesRecursive();

            //Assert
            Assert.IsTrue(recursive.Any(m =>
                m.Message.Contains($"SedimentThickness {thickProp.SpatiallyVaryingName} is not fully covering the grid, please cover entire grid")));
        }

        [Test]
        public void ModelBuildUpSpaciallyVaryingOff()
        {
            //Arrange
            var fmModel = new WaterFlowFMModel();
            fmModel.ModelDefinition.UseMorphologySediment = true;
            var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            fmModel.Grid = grid;

            var thickProp = new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 5, 0, true, 0, true, "cc", "mydoubledescription", false, true, sediment1=>true, sediment2=> true)
            {
                SpatiallyVaryingName = "mysedimentName_IniSedThick",
                Value = 12.3,
                Name ="de",
                Description = "",
                DefaultValue = 1,
                IsEnabled = true,
                IsSpatiallyVarying = false
            };

            CreateSedimentFraction(thickProp, fmModel);

            //Act
            var report = fmModel.Validate();
            var recursive = report.GetAllIssuesRecursive();

            //Assert
            Assert.IsFalse(recursive.Any(m => m.Message.Contains("SedimentThickness is not fully covering the grid, please cover entire grid")));
        }

        private static void CreateSedimentFraction(SpatiallyVaryingSedimentProperty<double> thickProp, WaterFlowFMModel fmModel)
        {
            var testSedimentType = new SedimentType
            {
                Key = "sand",
                Properties = new EventedList<ISedimentProperty> { thickProp }
            };

            var overallProp = new SedimentProperty<double>("Cref", 0, 0, true, 0, false, "km", "myoveralldescription", false)
            {
                Value = 80.1
            };

            fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>() { overallProp };

            var fraction = new SedimentFraction
            {
                Name = "mysedimentName",
                CurrentSedimentType = testSedimentType
            };

            fmModel.SedimentFractions.Add(fraction);
        }
    }
}
