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

        public ValidationReport Validate(RainfallRunoffModel rootObject, RainfallRunoffModel model)
        {
            var reports = new List<ValidationReport>();

            var precipitation = model.Precipitation;
            var evaporation = model.Evaporation;
            var temperature = model.Temperature;

            reports.Add(new ValidationReport("Precipitation",
                                             ValidateMeteoData(precipitation, model.StartTime, model.StopTime, true).ToList()));
            reports.Add(new ValidationReport("Evaporation",
                                             ValidateMeteoData(evaporation, model.StartTime, model.StopTime, true).ToList()));
            
            if (precipitation.DataDistributionType == MeteoDataDistributionType.PerStation) //always for both precip & evap
            {
                reports.Add(new ValidationReport("Meteo stations", ValidateMeteoStations(model)));
            }

            if (model.ModelNeedsTemperatureData)
            {
                reports.Add(new ValidationReport("Temperature",
                                                 ValidateMeteoData(temperature, model.StartTime, model.StopTime).ToList()));

                if (temperature.DataDistributionType == MeteoDataDistributionType.PerStation)
                {
                    reports.Add(new ValidationReport("Temperature stations", ValidateTemperatureStations(model)));
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
            var timeArgument = meteoData.Data.Arguments.OfType<Variable<DateTime>>().FirstOrDefault();
            var issues = new List<ValidationIssue>();

            if (timeArgument.Values.Count < 2)
            {
                issues.Add(new ValidationIssue(meteoData, ValidationSeverity.Error,
                                               "Not enough values defined"));
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
                                               String.Format("Time series starts ({0}) after start of model ({1})",
                                                             meteoStart, startTime)));
            }

            DateTime timeSeriesEnd = timeArgument.Values[timeArgument.Values.Count - 1];
            TimeSpan timestep = timeArgument.Values[1] - timeArgument.Values[0];
            DateTime meteoEnd = addtimestep ? timeSeriesEnd.Add(timestep) : timeSeriesEnd;

            if (meteoEnd < stopTime)
            {
                issues.Add(new ValidationIssue(meteoData, ValidationSeverity.Error,
                                               String.Format("Time series stops ({0}) before end of model ({1})",
                                                             meteoEnd, stopTime)));
            }
            return issues;
        }
    }
}