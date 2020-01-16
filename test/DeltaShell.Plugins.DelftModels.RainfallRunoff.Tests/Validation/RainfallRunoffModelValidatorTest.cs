using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Validation
{
    [TestFixture]
    public class RainfallRunoffModelValidatorTest
    {
        [Test]
        public void ValidateEmptyModel()
        {
            var model = new RainfallRunoffModel();

            var validator = new RainfallRunoffModelValidator();

            var report = validator.Validate(model);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            Assert.GreaterOrEqual(report.ErrorCount, 1);
        }

        [Test]
        public void ValidateCalculationAreas()
        {
            var validator = new RainfallRunoffCatchmentDataValidator();
            var model = new RainfallRunoffModel();

            var c = Catchment.CreateDefault();
            c.CatchmentType = CatchmentType.Unpaved;
            model.Basin.Catchments.Add(c);

            var boundary = new RunoffBoundary();
            model.Basin.Boundaries.Add(boundary);
            c.LinkTo(boundary);

            var unpavedData = model.GetCatchmentModelData(c);
            
            // larger
            unpavedData.CalculationArea = c.AreaSize*3.0;
            var report = validator.Validate(model, null);
            var issue = report.GetAllIssuesRecursive().First(i => i.Severity == ValidationSeverity.Info);
            Assert.IsTrue(issue.Message.Contains("significantly larger"));

            // smaller
            unpavedData.CalculationArea = c.AreaSize * 0.4;
            report = validator.Validate(model, null);
            issue = report.GetAllIssuesRecursive().First(i => i.Severity == ValidationSeverity.Info);
            Assert.IsTrue(issue.Message.Contains("significantly smaller"));

            // zero
            unpavedData.CalculationArea = 0.0;
            report = validator.Validate(model, null);
            issue = report.GetAllIssuesRecursive().First(i => i.Severity == ValidationSeverity.Warning);
            Assert.IsTrue(issue.Message.Contains("zero"));
        }

        [Test]
        public void ValidateNonExistingMeteoString()
        {
            var model = new RainfallRunoffModel();

            var c = Catchment.CreateDefault();
            var boundary = new RunoffBoundary();
            c.CatchmentType = CatchmentType.Unpaved;
            model.Basin.Catchments.Add(c);
            model.Basin.Boundaries.Add(boundary);
            c.LinkTo(boundary);

            var unpavedData = model.GetCatchmentModelData(c);
            unpavedData.MeteoStationName = "blah";
            model.MeteoStations.Add("a");

            FillMeteoDataTimes(model);

            var validator = new RainfallRunoffModelValidator();
            var report = validator.Validate(model);

            // expect no warning: we're not working with meteo stations
            Assert.AreEqual(ValidationSeverity.None, report.Severity());

            // switch to per-station
            model.Precipitation.DataDistributionType = MeteoDataDistributionType.PerStation;
            FillMeteoDataTimes(model);
            report = validator.Validate(model);
            Assert.AreEqual(ValidationSeverity.Warning, report.Severity()); // expect warning: we are working with meteo stations

            model.MeteoStations.Add("blah");
            report = validator.Validate(model);
            Assert.AreEqual(ValidationSeverity.None, report.Severity()); // expect no more warning: we fix it
        }

        private static void FillMeteoDataTimes(RainfallRunoffModel model)
        {
            var generator = new TimeSeriesGenerator();
            generator.GenerateTimeSeries(model.Precipitation.Data, model.StartTime, model.StopTime,
                                         new TimeSpan(0, 1, 0, 0));
            generator.GenerateTimeSeries(model.Evaporation.Data, model.StartTime, model.StopTime,
                                         new TimeSpan(1, 0, 0, 0));
        }

        [Test]
        public void ValidateWithConsistentState()
        {
            var validRestartFilePath = TestHelper.GetTestFilePath("valid_state_RR.zip");
            var model = CreateValidMiniModel();
            model.RestartInput = new FileBasedRestartState("test", validRestartFilePath);
            model.UseRestart = true;

            var validator = new RainfallRunoffModelValidator();

            var report = validator.Validate(model);
            Assert.AreEqual(0, report.ErrorCount);
            Assert.AreEqual(0, report.WarningCount);
            Assert.AreEqual(0, report.InfoCount);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ValidateConsistentStateWithoutMetadataFile()
        {
            var validRestartFilePath = TestHelper.GetTestFilePath("valid_state_without_meta_RR.zip");
            var model = CreateValidMiniModel();
            model.RestartInput = new FileBasedRestartState("test", validRestartFilePath);
            model.UseRestart = true;

            var validator = new RainfallRunoffModelValidator();

            var report = validator.Validate(model);
            Assert.AreEqual(0, report.ErrorCount);
            Assert.AreEqual(0, report.WarningCount);
            Assert.AreEqual(0, report.InfoCount);
        }

        [Test]
        public void ValidateWithInconsistentState()
        {
            var invalidRestartFilePath = TestHelper.GetTestFilePath("invalid_state_RR.zip");
            var model = CreateValidMiniModel();

            var validator = new RainfallRunoffModelValidator();
            model.RestartInput = new FileBasedRestartState("test", invalidRestartFilePath);
            model.UseRestart = true;

            var report = validator.Validate(model);
            Assert.AreEqual(9, report.ErrorCount);
            var validationIssues = report.AllErrors;
            Assert.IsTrue(validationIssues.All(vi => vi.Subject == "Input restart state"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfGreenHouseCatchments: Value of '1' in restart state not matching expected value of '0' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfNoneCatchments: Value of '3' in restart state not matching expected value of '0' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfOpenWaterCatchments: Value of '4' in restart state not matching expected value of '0' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfPavedCatchments: Value of '5' in restart state not matching expected value of '0' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfPolderCatchments: Value of '6' in restart state not matching expected value of '0' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfUnpavedCatchments: Value of '7' in restart state not matching expected value of '2' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfSacramentoCatchments: Value of '8' in restart state not matching expected value of '0' of current situation")); 
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfHbvCatchments: Value of '9' in restart state not matching expected value of '0' of current situation"));
            Assert.IsNotNull(validationIssues.Single(vi => vi.Message == "NrOfBoundaries: Value of '10' in restart state not matching expected value of '0' of current situation"));
            Assert.AreEqual(0, report.WarningCount);
            Assert.AreEqual(0, report.InfoCount);
        }

        [Test]
        public void ValidateStateWithInvalidModelType()
        {
            var invalidRestartFilePath = TestHelper.GetTestFilePath("invalid_ModelType_state_RR.zip");
            var model = CreateValidMiniModel();

            var validator = new RainfallRunoffModelValidator();
            model.RestartInput = new FileBasedRestartState("test", invalidRestartFilePath);
            model.UseRestart = true;

            var report = validator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.AreEqual("Model type of 'test' is not compatible.", report.AllErrors.First().Message);
            Assert.AreEqual(0, report.WarningCount);
            Assert.AreEqual(0, report.InfoCount);
        }

        [Test]
        public void ValidateStateWithInvalidVersion()
        {
            var invalidRestartFilePath = TestHelper.GetTestFilePath("invalid_Version_state_RR.zip");
            var model = CreateValidMiniModel();

            var validator = new RainfallRunoffModelValidator();
            model.RestartInput = new FileBasedRestartState("test", invalidRestartFilePath);
            model.UseRestart = true;

            var report = validator.Validate(model);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.AreEqual("Version 2 is not supported.", report.AllErrors.First().Message);
            Assert.AreEqual(0, report.WarningCount);
            Assert.AreEqual(0, report.InfoCount);
        }

        [Test]
        public void ValidateRainfallRunoffModelInputRestartStatePathIncorect()
        {
            var model = CreateValidMiniModel();

            const string invalidPath = "invalidPath";
            var fileBasedRestartState = new FileBasedRestartState("test", invalidPath);
            ((IFileBased)fileBasedRestartState).Path = invalidPath;
            model.RestartInput = fileBasedRestartState;
            model.UseRestart = true;

            var validator = new RainfallRunoffModelValidator();
            var report = validator.Validate(model);
            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            var issues = report.GetAllIssuesRecursive();

            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual("Model state file does not exist: " + invalidPath, issues[0].Message);
        }

        [Test]
        public void ValidateRainfallRunoffModelInputRestartStatePathToNonZip()
        {
            var filePathToNonZipFile =
                TestHelper.GetTestFilePath("T25RRSA.EVP");
            var model = CreateValidMiniModel();

            var fileBasedRestartState = new FileBasedRestartState("test", filePathToNonZipFile);
            ((IFileBased)fileBasedRestartState).Path = filePathToNonZipFile;
            model.RestartInput = fileBasedRestartState;
            model.UseRestart = true;

            var validator = new RainfallRunoffModelValidator();
            var report = validator.Validate(model);
            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            var issues = report.GetAllIssuesRecursive();

            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual("Model state file should be zip file and have the extension .zip", issues[0].Message);
        }

        [Test]
        public void ValidateRainfallRunoffWithoutHydroLinkReportError()
        {
            var model = CreateValidMiniModel();

            var unpavedDatas = model.GetAllModelData().OfType<UnpavedData>().ToList();
            unpavedDatas.ForEach( ud => ud.Catchment.Links.Clear());
            Assert.IsFalse( unpavedDatas.Any( ud => ud.Catchment.Links.Any()));
            var report = new UnpavedDataValidator().Validate(model, unpavedDatas);
            var errMssg =
                string.Format("No runoff target has been defined (concept: {0}); an implicit boundary will be used.",
                    unpavedDatas[0].GetType().Name);
            Assert.IsTrue(report.AllErrors.Any( err => err.Severity == ValidationSeverity.Error 
                                                        && err.Message == errMssg));
        }


        private RainfallRunoffModel CreateValidMiniModel()
        {
            var file = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"RRMiniTestModels\DRRSA.lit\2\NETWORK.TP");
            var composite = RainfallRunoffIntegrationTestHelper.ImportModel(file);
            return composite.Activities.OfType<RainfallRunoffModel>().First();
        }
    }
}