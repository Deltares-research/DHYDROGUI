using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
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
            var report = RainfallRunoffModelValidator.Validate(model);

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
            c.IsGeometryDerivedFromAreaSize = false;
            model.Basin.Catchments.Add(c);

            var boundary = new RunoffBoundary();
            model.Basin.Boundaries.Add(boundary);
            c.LinkTo(boundary);

            var unpavedData = model.GetCatchmentModelData(c);
            
            // larger
            unpavedData.CalculationArea = c.GeometryArea*3.0;
            var report = validator.Validate(model, null);
            var issue = report.GetAllIssuesRecursive().First(i => i.Severity == ValidationSeverity.Info);
            Assert.IsTrue(issue.Message.Contains("significantly larger"));

            // smaller
            unpavedData.CalculationArea = c.GeometryArea * 0.4;
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

            var report = RainfallRunoffModelValidator.Validate(model);

            // expect no warning: we're not working with meteo stations
            Assert.AreEqual(ValidationSeverity.None, report.Severity());

            // switch to per-station
            model.Precipitation.DataDistributionType = MeteoDataDistributionType.PerStation;
            FillMeteoDataTimes(model);
            report = RainfallRunoffModelValidator.Validate(model);
            Assert.AreEqual(ValidationSeverity.Warning, report.Severity()); // expect warning: we are working with meteo stations

            model.MeteoStations.Add("blah");
            report = RainfallRunoffModelValidator.Validate(model);
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
        [Category("Quarantine")]
        [Ignore("Sobek RR is not implemented yet.")]
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