using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
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

            ValidationReport report = model.Validate();

            Assert.AreEqual(1,
                            report.GetAllIssuesRecursive()
                                  .Count(i => i.Severity == ValidationSeverity.Info && i.Message.Contains("coordinate system")));
        }

        [Test]
        public void CheckThatInitializingFmModelWithSalinityAndTemperatureEnabledNoWarningMessagesAreGiven()
        {
            var model = new WaterFlowFMModel();
            //Enable Salinity and Temperature checkboxes
            WaterFlowFMProperty salinityProperty = model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            WaterFlowFMProperty temperatureProperty = model.ModelDefinition.GetModelProperty(KnownProperties.Temperature);
            //Create a grid
            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);

            //Validate model
            ValidationReport report = model.Validate();
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
            model.Area.Pumps.Add(new Pump()
            {
                Name = "A",
                Capacity = -1.2,
            });

            ValidationReport report = model.Validate();

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
            var pump = new Pump()
            {
                Name = "A",
                UseCapacityTimeSeries = true
            };
            pump.CapacityTimeSeries[new DateTime(2000, 1, 2)] = -1.2;
            model.Area.Pumps.Add(pump);

            ValidationReport report = model.Validate();

            IList<ValidationIssue> issues = report.GetAllIssuesRecursive();
            Assert.AreEqual(1, issues.Count(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message.Contains("pump 'A': capacity time series values must be greater than or equal to 0.")));
            Assert.AreEqual(1, issues.Count(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message.Contains("pump 'A': capacity time series does not span the model run interval.")));
        }

        [Test]
        public void CheckCoordinateSystemValidation()
        {
            var model = new WaterFlowFMModel {CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(3824)};

            ValidationReport report = model.Validate();

            Assert.AreEqual(1,
                            report.GetAllIssuesRecursive()
                                  .Count(i => i.Severity == ValidationSeverity.Warning && i.Message.Contains("coordinate system")));
        }

        [Test]
        public void CheckSolverTypeValidation()
        {
            var model = new WaterFlowFMModel {CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(3824)};
            model.ModelDefinition.GetModelProperty(KnownProperties.SolverType).SetValueFromString("7");

            ValidationReport report = model.Validate();

            Assert.AreEqual(1,
                            report.GetAllIssuesRecursive()
                                  .Count(i => i.Severity == ValidationSeverity.Error && i.Message.Contains("parallel run")));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ValidateWithSpaciallyVariantFullCoverage()
        {
            //Arrange
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"spatiallyVariantSediment\fullGridCoverage\FlowFM.mdu"));

            //Act
            ValidationReport report = model.Validate();

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
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"spatiallyVariantSediment\fullGridCoverage\FlowFM.mdu"));

            //Act
            ValidationReport report = model.Validate();

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
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            fmModel.Grid = grid;
            var thickProp = new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 5, 0, false, 0, true, "cc", "mydoubledescription", true, false)
            {
                SpatiallyVaryingName = "mysedimentName_IniSedThick",
                Value = 12.3
            };
            thickProp.IsSpatiallyVarying = true;

            CreateSedimentFraction(thickProp, fmModel);

            var coverage = (UnstructuredGridCoverage) fmModel.AllDataItems.First(di => di.Name == thickProp.SpatiallyVaryingName).Value;
            coverage.SetValues(new[]
            {
                1.0,
                2.0,
                3.0,
                4.0
            });

            //Act
            ValidationReport report = fmModel.Validate();
            IList<ValidationIssue> recursive = report.GetAllIssuesRecursive();

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
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            fmModel.Grid = grid;
            var thickProp = new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 5, 0, false, 0, true, "cc", "mydoubledescription", true, false)
            {
                SpatiallyVaryingName = "mysedimentName_IniSedThick",
                Value = 12.3
            };
            thickProp.IsSpatiallyVarying = true;

            CreateSedimentFraction(thickProp, fmModel);

            var coverage = (UnstructuredGridCoverage) fmModel.AllDataItems.First(di => di.Name == thickProp.SpatiallyVaryingName).Value;
            coverage.SetValues(new[]
            {
                -999.0,
                2.0,
                3.0,
                4.0
            });

            //Act
            ValidationReport report = fmModel.Validate();
            IList<ValidationIssue> recursive = report.GetAllIssuesRecursive();

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
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            fmModel.Grid = grid;

            var thickProp = new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 5, 0, true, 0, true, "cc", "mydoubledescription", false, true, sediment1 => true, sediment2 => true)
            {
                SpatiallyVaryingName = "mysedimentName_IniSedThick",
                Value = 12.3,
                Name = "de",
                Description = "",
                DefaultValue = 1,
                IsEnabled = true,
                IsSpatiallyVarying = false
            };

            CreateSedimentFraction(thickProp, fmModel);

            //Act
            ValidationReport report = fmModel.Validate();
            IList<ValidationIssue> recursive = report.GetAllIssuesRecursive();

            //Assert
            Assert.IsFalse(recursive.Any(m => m.Message.Contains("SedimentThickness is not fully covering the grid, please cover entire grid")));
        }

        [Test]
        public void ValidateRestartInput_WhenRestartWillNotBeUsed_ThenValidationReportShouldContainAnEmptyRestartSubReport()
        {
            // Given
            var fmModel = new WaterFlowFMModel();

            // When
            ValidationReport validationReport = WaterFlowFmModelValidationExtensions.Validate(fmModel);

            // Assert
            Assert.IsFalse(fmModel.UseRestart);

            ValidationReport subReport = validationReport.SubReports.FirstOrDefault(sr => sr.Category ==
                                                                                          Resources.WaterFlowFmModelValidationExtensions_ValidateRestartInput_Input_restart_state);
            Assert.IsNotNull(subReport);
            Assert.IsTrue(subReport.IsEmpty);
        }

        [Test]
        public void ValidateRestartInput_WhenRestartInputFileDoesNotExist_ShouldReportErrorInValidationReport()
        {
            // Given
            var fmModel = new WaterFlowFMModel {RestartInput = new WaterFlowFMRestartFile("test")};

            // When
            ValidationReport validationReport = WaterFlowFmModelValidationExtensions.Validate(fmModel);

            //Assert
            Assert.IsTrue(fmModel.UseRestart);

            ValidationReport subReport = validationReport.SubReports.FirstOrDefault(sr => sr.Category ==
                                                                                          Resources.WaterFlowFmModelValidationExtensions_ValidateRestartInput_Input_restart_state);
            Assert.IsNotNull(subReport);

            IList<ValidationIssue> allIssues = subReport.GetAllIssuesRecursive();
            Assert.AreEqual(1, allIssues.Count);

            ValidationIssue issue = allIssues[0];
            Assert.AreEqual(Resources.WaterFlowFmModelValidationExtensions_ValidateRestartInput_Input_restart_file_does_not_exist_cannot_restart, issue.Message);
            Assert.AreEqual(ValidationSeverity.Error, issue.Severity);
            Assert.AreEqual(Resources.WaterFlowFmModelValidationExtensions_ValidateRestartInput_Input_restart_state, issue.Subject);
        }

        [Test]
        public void ValidateRestartInput_WhenRestartInputFileExists_ThenValidationReportShouldContainAnEmptyRestartSubReport()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                string restartFilePath = Path.Combine(tempDirectory.Path, "test_rst.nc");
                File.WriteAllText(restartFilePath, "test");

                var fmModel = new WaterFlowFMModel {RestartInput = new WaterFlowFMRestartFile(restartFilePath)};

                // When
                ValidationReport validationReport = WaterFlowFmModelValidationExtensions.Validate(fmModel);

                // Assert
                Assert.IsTrue(fmModel.UseRestart);

                ValidationReport subReport = validationReport.SubReports.FirstOrDefault(sr => sr.Category ==
                                                                                              Resources.WaterFlowFmModelValidationExtensions_ValidateRestartInput_Input_restart_state);
                Assert.IsNotNull(subReport);
                Assert.IsTrue(subReport.IsEmpty);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Validate_WhenRestartTimeStepIsNotCorrect_ShouldGiveWriteRestartSubReportWithError()
        {
            // Given
            var fmModel = new WaterFlowFMModel
            {
                WriteRestart = true,
                TimeStep = new TimeSpan(0, 2, 0, 0)
            };
            fmModel.ModelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value = new TimeSpan(0, 3, 0, 0);

            // When
            ValidationReport validationReport = WaterFlowFmModelValidationExtensions.Validate(fmModel);

            // Then
            ValidationReport writeRestartSubValidationReport = validationReport.SubReports.FirstOrDefault(sr =>
                                                                                                              sr.Category == NGHS.Common.Properties.Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_Restart_time_range_settings);

            Assert.IsNotNull(writeRestartSubValidationReport);
            ValidationIssue restartValidationIssue = writeRestartSubValidationReport.GetAllIssuesRecursive().FirstOrDefault(i =>
                                                                                                                                i.Message == NGHS.Common.Properties.Resources.RestartTimeRangeValidator_ValidateRestartTimeRangeSettings_The_restart_time_step_must_be_an_integer_multiple_of_the_output_time_step_);
            Assert.IsNotNull(restartValidationIssue);
            object viewData = restartValidationIssue.ViewData;
            Assert.IsInstanceOf<FmValidationShortcut>(viewData);
            Assert.AreSame(fmModel, ((FmValidationShortcut) viewData).FlowFmModel);
            Assert.AreEqual("Output Parameters", ((FmValidationShortcut) viewData).TabName);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Validate_PhysicalProcesses_CompositeHeatFluxModelTypeWithoutMeteoData_AddsExpectedValidationIssueToValidationReport()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                string compositeHeatFluxModelType = ((int) HeatFluxModelType.Composite).ToString();
                model.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString(compositeHeatFluxModelType);
                
                // Precondition
                Assert.That(model.ModelDefinition.HeatFluxModel.MeteoData.GetValues<double>().Any(), Is.False);
                
                // Call
                ValidationReport report = model.Validate();

                // Assert
                Assert.That(report, Is.Not.Null);

                ValidationReport physicalProcessesReport = report.SubReports.SingleOrDefault(r => r.Category.Equals("Physical Processes"));
                Assert.That(physicalProcessesReport, Is.Not.Null);

                const string expectedError = "Composite Model option is selected for Temperature, however no Meteo Data was specified.";
                ValidationIssue heatFluxModelMeteoDataIssue = physicalProcessesReport.GetAllIssuesRecursive()
                                                                                     .SingleOrDefault(i => i.Message == expectedError);
                Assert.That(heatFluxModelMeteoDataIssue, Is.Not.Null);
                Assert.That(heatFluxModelMeteoDataIssue.Severity, Is.EqualTo(ValidationSeverity.Error));
                
                object viewData = heatFluxModelMeteoDataIssue.ViewData;
                Assert.IsInstanceOf<HeatFluxModel>(viewData);
            }
        }

        private static void CreateSedimentFraction(SpatiallyVaryingSedimentProperty<double> thickProp, WaterFlowFMModel fmModel)
        {
            var testSedimentType = new SedimentType
            {
                Key = "sand",
                Properties = new EventedList<ISedimentProperty> {thickProp}
            };

            var overallProp = new SedimentProperty<double>("Cref", 0, 0, true, 0, false, "km", "myoveralldescription", false) {Value = 80.1};

            fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>() {overallProp};

            var fraction = new SedimentFraction
            {
                Name = "mysedimentName",
                CurrentSedimentType = testSedimentType
            };

            fmModel.SedimentFractions.Add(fraction);
        }
    }
}