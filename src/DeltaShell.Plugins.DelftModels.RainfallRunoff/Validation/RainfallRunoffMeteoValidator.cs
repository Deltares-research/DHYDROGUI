using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public static class RainfallRunoffMeteoValidator
    {
        public static ValidationReport Validate(RainfallRunoffModel rainfallRunoffModel)
        {
            var reports = new List<ValidationReport>();

            var precipitation = rainfallRunoffModel.Precipitation;
            var evaporation = rainfallRunoffModel.Evaporation;
            var temperature = rainfallRunoffModel.Temperature;

            reports.Add(new ValidationReport("Precipitation",
                                             ValidateMeteoData(precipitation, rainfallRunoffModel.StartTime, rainfallRunoffModel.StopTime, rainfallRunoffModel.TimeStep, true).ToList()));
            reports.Add(new ValidationReport("Evaporation",
                                             ValidateMeteoData(evaporation, rainfallRunoffModel.StartTime, rainfallRunoffModel.StopTime, rainfallRunoffModel.TimeStep, true).ToList()));
            
            if (precipitation.DataDistributionType == MeteoDataDistributionType.PerStation) //always for both precip & evap
            {
                reports.Add(new ValidationReport("Meteo stations", ValidateMeteoStations(rainfallRunoffModel)));
            }

            if (rainfallRunoffModel.ModelNeedsTemperatureData)
            {
                reports.Add(new ValidationReport("Temperature",
                                                 ValidateMeteoData(temperature, rainfallRunoffModel.StartTime, rainfallRunoffModel.StopTime, rainfallRunoffModel.TimeStep).ToList()));

                if (temperature.DataDistributionType == MeteoDataDistributionType.PerStation)
                {
                    reports.Add(new ValidationReport("Temperature stations", ValidateTemperatureStations(rainfallRunoffModel)));
                }
            }

            return new ValidationReport("Meteo", reports);
        }

        private static IEnumerable<ValidationIssue> ValidateMeteoStations(RainfallRunoffModel model)
        {
            var stationIssues = new List<ValidationIssue>();
            if (model.MeteoStations.Count == 0)
            {
                stationIssues.Add(new ValidationIssue(model.Precipitation, ValidationSeverity.Error, "No meteo stations defined"));
            }
            return stationIssues;
        }

        private static IEnumerable<ValidationIssue> ValidateTemperatureStations(RainfallRunoffModel model)
        {
            var stationIssues = new List<ValidationIssue>();
            if (model.TemperatureStations.Count == 0)
            {
                stationIssues.Add(new ValidationIssue(model.Temperature, ValidationSeverity.Error, "No temperature stations defined"));
            }
            return stationIssues;
        }

        private static IEnumerable<ValidationIssue> ValidateMeteoData(
            MeteoData meteoData,
            DateTime startTime,
            DateTime stopTime,
            TimeSpan timeStep,
            bool addtimestep = false)
        {
            var issues = new List<ValidationIssue>();
            var timeArgument = meteoData.Data?.Arguments?.OfType<Variable<DateTime>>().FirstOrDefault();
            if (timeArgument == null) return issues;
            
            int startEndDateDaysDifference = startTime.Date.CompareTo(stopTime.Date);
            int valuesCount = timeArgument.Values.Count;
            if (valuesCount == 0 || valuesCount < 2 && startEndDateDaysDifference != 0)
            {
                issues.Add(new ValidationIssue(meteoData, ValidationSeverity.Error, "Not enough values defined"));
                return issues;
            }

            if (timeArgument.ExtrapolationType != ExtrapolationType.None) //TODO: check if extrapolation does not lead to negative precipitation/evaporation...
            {
                return issues;
            }

            DateTime meteoStart = timeArgument.Values[0];

            if (meteoStart > startTime)
            {
                issues.Add(new ValidationIssue(meteoData, ValidationSeverity.Error,
                                               $"Time series starts ({meteoStart}) after start of model ({startTime})"));
            }

            if (valuesCount <= 1)
            {
                return issues;
            }

            DateTime timeSeriesEnd = timeArgument.Values[valuesCount - 1];
            TimeSpan meteoTimeStep = timeArgument.Values[1] - timeArgument.Values[0];
            DateTime meteoEnd = addtimestep ? timeSeriesEnd.Add(meteoTimeStep) : timeSeriesEnd;

            if (meteoEnd < stopTime)
            {
                issues.Add(new ValidationIssue(meteoData, ValidationSeverity.Error,
                                               $"Time series stops ({meteoEnd}) before end of model ({stopTime})"));
            }

            if (timeStep.TotalSeconds > 0 && meteoTimeStep.TotalSeconds % timeStep.TotalSeconds != 0)
            {
                issues.Add(new ValidationIssue(meteoData, ValidationSeverity.Error,
                                               $"Time step of time series ({meteoTimeStep}) should be a multiple of the computation time step {timeStep}"));

            }

            return issues;
        }
    }
}