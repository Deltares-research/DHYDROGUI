using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Validation
{
    [TestFixture]
    public class RainfallRunoffMeteoValidatorTest
    {
        [Test]
        public void NoValuesForMeteo()
        {
            var rrm = new RainfallRunoffModel();
            
            //prepare
            
            var report = RainfallRunoffMeteoValidator.Validate(rrm);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            var issues = report.GetAllIssuesRecursive();

            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual("Not enough values defined", issues[0].Message);
            Assert.AreEqual(rrm.Precipitation, issues[0].Subject);
        }

        [Test]
        public void NoValuesForPerStationMeteo()
        {
            var rrm = new RainfallRunoffModel
                {
                    Precipitation = {DataDistributionType = MeteoDataDistributionType.PerStation}
                };
            rrm.MeteoStations.Add("station1");
            rrm.TemperatureStations.Add("station1");

            //prepare

            var report = RainfallRunoffMeteoValidator.Validate(rrm);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            var issues = report.GetAllIssuesRecursive();

            Assert.AreEqual(2, issues.Count);
            Assert.AreEqual("Not enough values defined", issues[0].Message);
            Assert.AreEqual(rrm.Precipitation, issues[0].Subject);
            Assert.AreEqual("Not enough values defined", issues[1].Message);
            Assert.AreEqual(rrm.Evaporation, issues[1].Subject);
        }

        [Test]
        public void NoStations()
        {
            var rrm = new RainfallRunoffModel
                {
                    Precipitation = {DataDistributionType = MeteoDataDistributionType.PerStation},
                    Temperature = {DataDistributionType = MeteoDataDistributionType.PerStation}
                };

            //prepare
            var generator = new TimeSeriesGenerator();
            generator.GenerateTimeSeries(rrm.Precipitation.Data, rrm.StartTime, rrm.StopTime,
                                         new TimeSpan(0, 1, 0, 0));

            generator.GenerateTimeSeries(rrm.Evaporation.Data, rrm.StartTime, rrm.StopTime,
                                         new TimeSpan(1, 0, 0, 0));

            var report = RainfallRunoffMeteoValidator.Validate(rrm);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            var issues = report.GetAllIssuesRecursive();

            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual("No meteo stations defined", issues[0].Message);
            Assert.AreEqual(rrm.Precipitation, issues[0].Subject);
        }

        [Test]
        public void StartAndStopTimeTheSame()
        {
            var rrm = new RainfallRunoffModel
                {
                    Precipitation = {DataDistributionType = MeteoDataDistributionType.Global},
                    Temperature = {DataDistributionType = MeteoDataDistributionType.Global},
                    StartTime = new DateTime(2000, 1, 1),
                    StopTime = new DateTime(2000, 1, 1)
                };

            var report = RainfallRunoffModelValidator.ValidateTimers(rrm);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            var issues = report.GetAllIssuesRecursive();

            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual("The calculation period must be positive.", issues[0].Message);
        }
        
        [Test]
        public void TimeStepNotAMultiple()
        {
            var rrm = new RainfallRunoffModel
            {
                Precipitation = { DataDistributionType = MeteoDataDistributionType.Global },
                Temperature = { DataDistributionType = MeteoDataDistributionType.Global },
                Evaporation = { DataDistributionType = MeteoDataDistributionType.Global},
                StartTime = new DateTime(2000, 1, 1),
                StopTime = new DateTime(2000, 1, 2),
                TimeStep = new TimeSpan(1,0,0)
            };

            var generator = new TimeSeriesGenerator();
            generator.GenerateTimeSeries(rrm.Precipitation.Data, rrm.StartTime, rrm.StopTime,
                                         new TimeSpan(0, 1, 30, 0));

            generator.GenerateTimeSeries(rrm.Evaporation.Data, rrm.StartTime, rrm.StopTime,
                                         new TimeSpan(0, 0, 10, 0));

            generator.GenerateTimeSeries(rrm.Temperature.Data, rrm.StartTime, rrm.StopTime,
                                         new TimeSpan(0, 2, 0, 0));

            var report = RainfallRunoffMeteoValidator.Validate(rrm);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            var issues = report.GetAllIssuesRecursive();

            Assert.AreEqual(2, issues.Count);
            Assert.AreEqual("Time step of time series (01:30:00) should be a multiple of the computation time step 01:00:00", issues[0].Message);
            Assert.AreEqual("Time step of time series (00:10:00) should be a multiple of the computation time step 01:00:00", issues[1].Message);
        }

        [Test]
        public void MeteoDoesNotMatchWithModel()
        {
            var rrm = new RainfallRunoffModel
                          {
                              StartTime = new DateTime(2000, 1, 1),
                              StopTime = new DateTime(2000, 1, 5)
                          };

            //prepare
            var generator = new TimeSeriesGenerator();
            var precipStart = rrm.StartTime.AddDays(1);
            generator.GenerateTimeSeries(rrm.Precipitation.Data, precipStart, rrm.StopTime,
                                         new TimeSpan(0, 1, 0, 0));
            var evapEnd = rrm.StopTime.AddDays(-1);
            generator.GenerateTimeSeries(rrm.Evaporation.Data, rrm.StartTime, evapEnd,
                                         new TimeSpan(0, 1, 0, 0));

            var effectiveEvapEnd = evapEnd.AddHours(1);

            var report = RainfallRunoffMeteoValidator.Validate(rrm);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            var issues = report.GetAllIssuesRecursive();

            Assert.AreEqual(2, issues.Count);
            Assert.AreEqual(String.Format("Time series starts ({0}) after start of model ({1})", precipStart, rrm.StartTime),
                issues[0].Message);
            Assert.AreEqual(rrm.Precipitation, issues[0].Subject);
            Assert.AreEqual(String.Format("Time series stops ({0}) before end of model ({1})", effectiveEvapEnd, rrm.StopTime),
                            issues[1].Message);
            Assert.AreEqual(rrm.Evaporation, issues[1].Subject);
        }

        [Test]
        public void ValidationOfTemperatureDataSkippedWithoutHbvCatchments()
        {
            var basin = new DrainageBasin();
            
            var rrm = new RainfallRunoffModel
            {
                Basin = basin,
                StartTime = new DateTime(2000, 1, 1),
                StopTime = new DateTime(2000, 1, 10)
            };

            //prepare
            var generator = new TimeSeriesGenerator();

            var start = rrm.StartTime;
            var end = rrm.StopTime;

            generator.GenerateTimeSeries(rrm.Precipitation.Data, start, end,
                                         new TimeSpan(1, 0, 0, 0));

            generator.GenerateTimeSeries(rrm.Evaporation.Data, start, end,
                                         new TimeSpan(1, 0, 0, 0));

            generator.GenerateTimeSeries(rrm.Temperature.Data, start.AddDays(2), end.AddDays(-2),
                                        new TimeSpan(1, 0, 0, 0));

            var report = RainfallRunoffMeteoValidator.Validate(rrm);
            var issues = report.GetAllIssuesRecursive();
            Assert.AreEqual(0, issues.Count());

            basin.Catchments.Add(new Catchment() { CatchmentType = CatchmentType.Hbv });

            report = RainfallRunoffMeteoValidator.Validate(rrm);
            issues = report.GetAllIssuesRecursive();
            Assert.AreEqual(2, issues.Count());
            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            Assert.AreEqual(rrm.Temperature, issues[0].Subject);
        }

        [Test]
        public void ValidationOfTemperatureDoesNotAddExtraTimeStep()
        {
            var basin = new DrainageBasin();
            basin.Catchments.Add(new Catchment { CatchmentType = CatchmentType.Hbv });

            var rrm = new RainfallRunoffModel
            {
                Basin = basin,
                StartTime = new DateTime(2000, 1, 1),
                StopTime = new DateTime(2000, 1, 10)
            };

            //prepare
            var generator = new TimeSeriesGenerator();

            var start = rrm.StartTime;
            var end = rrm.StopTime.AddDays(-1);

            generator.GenerateTimeSeries(rrm.Precipitation.Data, start, end,
                                         new TimeSpan(1, 0, 0, 0));

            generator.GenerateTimeSeries(rrm.Evaporation.Data, start, end,
                                         new TimeSpan(1, 0, 0, 0));

            generator.GenerateTimeSeries(rrm.Temperature.Data, start, end,
                                        new TimeSpan(1, 0, 0, 0));

            var report = RainfallRunoffMeteoValidator.Validate(rrm);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            var issues = report.GetAllIssuesRecursive();

            Assert.AreEqual(1, issues.Count);
            Assert.AreEqual($"Time series stops ({end}) before end of model ({rrm.StopTime})", issues[0].Message);
            Assert.AreEqual(rrm.Temperature, issues[0].Subject);
        }
    }
}