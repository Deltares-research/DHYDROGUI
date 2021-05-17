using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    public class RainfallRunoffMeteoValidator : IValidator<RainfallRunoffModel, RainfallRunoffModel>
    {
        #region IValidator<RainfallRunoffModel,RainfallRunoffModel> Members

        public ValidationReport Validate(RainfallRunoffModel rootObject, RainfallRunoffModel target)
        {
            var reports = new List<ValidationReport>();

            var precipitation = target.Precipitation;
            var evaporation = target.Evaporation;
            var temperature = target.Temperature;

            reports.Add(new ValidationReport("Precipitation",
                                             ValidateMeteoData(precipitation, target.StartTime, target.StopTime, true).ToList()));
            reports.Add(new ValidationReport("Evaporation",
                                             ValidateMeteoData(evaporation, target.StartTime, target.StopTime, true).ToList()));
            
            if (precipitation.DataDistributionType == MeteoDataDistributionType.PerStation) //always for both precip & evap
            {
                reports.Add(new ValidationReport("Meteo stations", ValidateMeteoStations(target)));
            }

            if (target.ModelNeedsTemperatureData)
            {
                reports.Add(new ValidationReport("Temperature",
                                                 ValidateMeteoData(temperature, target.StartTime, target.StopTime).ToList()));

                if (temperature.DataDistributionType == MeteoDataDistributionType.PerStation)
                {
                    reports.Add(new ValidationReport("Temperature stations", ValidateTemperatureStations(target)));
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

        #endregion

        private static IEnumerable<ValidationIssue> ValidateMeteoData(
            MeteoData meteoData,
            DateTime startTime,
            DateTime stopTime,
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

            if (valuesCount > 1)
            {
                DateTime timeSeriesEnd = timeArgument.Values[valuesCount - 1];
                TimeSpan timestep = timeArgument.Values[1] - timeArgument.Values[0];
                DateTime meteoEnd = addtimestep ? timeSeriesEnd.Add(timestep) : timeSeriesEnd;

                if (meteoEnd < stopTime)
                {
                    issues.Add(new ValidationIssue(meteoData, ValidationSeverity.Error,
                                                   $"Time series stops ({meteoEnd}) before end of model ({stopTime})"));
                }
            }

            return issues;
        }
    }
}