using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation
{
    /// <summary>
    /// <see cref="RainfallRunoffMeteoValidator"/> implements the <see cref="IValidator{T,S}"/> for the
    /// <see cref="MeteoData"/> of a <see cref="RainfallRunoffModel"/>.
    /// </summary>
    public static class RainfallRunoffMeteoValidator
    {
        public static ValidationReport Validate(RainfallRunoffModel rainfallRunoffModel)
        {
            var reports = new List<ValidationReport>();

            MeteoData precipitation = rainfallRunoffModel.Precipitation;
            MeteoData evaporation = rainfallRunoffModel.Evaporation;
            MeteoData temperature = rainfallRunoffModel.Temperature;

            reports.Add(new ValidationReport(Resources.RainfallRunoffMeteoValidator_Validate_Precipitation,
                                             ValidateMeteoData(precipitation,
                                                               rainfallRunoffModel.StartTime,
                                                               rainfallRunoffModel.StopTime,
                                                               rainfallRunoffModel.TimeStep,
                                                               true).ToList()));
            reports.Add(new ValidationReport(Resources.RainfallRunoffMeteoValidator_Validate_Evaporation,
                                             ValidateMeteoData(evaporation,
                                                               rainfallRunoffModel.StartTime,
                                                               rainfallRunoffModel.StopTime,
                                                               rainfallRunoffModel.TimeStep,
                                                               true).ToList()));

            if (precipitation.DataDistributionType == MeteoDataDistributionType.PerStation) //always for both precip & evap
            {
                reports.Add(new ValidationReport(Resources.RainfallRunoffMeteoValidator_Validate_Meteo_stations,
                                                 ValidateMeteoStations(rainfallRunoffModel)));
            }

            if (rainfallRunoffModel.ModelNeedsTemperatureData)
            {
                reports.Add(new ValidationReport(Resources.RainfallRunoffMeteoValidator_Validate_Temperature,
                                                 ValidateMeteoData(temperature,
                                                                   rainfallRunoffModel.StartTime,
                                                                   rainfallRunoffModel.StopTime,
                                                                   rainfallRunoffModel.TimeStep).ToList()));

                if (temperature.DataDistributionType == MeteoDataDistributionType.PerStation)
                {
                    reports.Add(new ValidationReport(Resources.RainfallRunoffMeteoValidator_Validate_Temperature_stations,
                                                     ValidateTemperatureStations(rainfallRunoffModel)));
                }
            }

            return new ValidationReport(Resources.RainfallRunoffMeteoValidator_Validate_Meteo, reports);
        }

        private static IEnumerable<ValidationIssue> ValidateMeteoStations(RainfallRunoffModel model)
        {
            var stationIssues = new List<ValidationIssue>();
            if (model.MeteoStations.Count == 0)
            {
                stationIssues.Add(new ValidationIssue(model.Precipitation,
                                                      ValidationSeverity.Error,
                                                      Resources.RainfallRunoffMeteoValidator_Validate_No_meteo_stations_defined));
            }
            return stationIssues;
        }

        private static IEnumerable<ValidationIssue> ValidateTemperatureStations(RainfallRunoffModel model)
        {
            var stationIssues = new List<ValidationIssue>();
            if (model.TemperatureStations.Count == 0)
            {
                stationIssues.Add(new ValidationIssue(model.Temperature,
                                                      ValidationSeverity.Error,
                                                      Resources.RainfallRunoffMeteoValidator_Validate_No_temperature_stations_defined));
            }
            return stationIssues;
        }

        private static IEnumerable<ValidationIssue> ValidateMeteoData(MeteoData meteoData,
                                                                      DateTime startTime,
                                                                      DateTime stopTime,
                                                                      TimeSpan timeStep,
                                                                      bool addTimeStep = false)
        {
            ValidationIssue CreateSingleIssue(string msg) =>
                new ValidationIssue(meteoData, ValidationSeverity.Error, msg);

            Variable<DateTime> timeArgument =
                meteoData.Data?.Arguments?.OfType<Variable<DateTime>>().FirstOrDefault();

            if (timeArgument == null)
                yield break;

            if (!timeArgument.HasCorrectNumberValues(startTime, stopTime))
            {
                string msg = Resources.RainfallRunoffMeteoValidator_Validate_Not_enough_values_defined;
                yield return CreateSingleIssue(msg);
                yield break;
            }

            if (timeArgument.ExtrapolationType != ExtrapolationType.None)
                yield break;

            if (!timeArgument.HasCorrectStartingTime(startTime))
            {
                string msg = string.Format(Resources.RainfallRunoffMeteoValidator_Validate_Time_series_starts___0___after_start_of_model___1__,
                                           timeArgument.Values[0],
                                           startTime);
                yield return CreateSingleIssue(msg);
            }

            if (!timeArgument.HasCorrectStopTime(stopTime, addTimeStep))
            {
                string msg = string.Format(Resources.RainfallRunoffMeteoValidator_Validate_Time_series_stops___0___before_end_of_model___1__,
                                           timeArgument.GetMeteoEnd(addTimeStep),
                                           stopTime);
                yield return CreateSingleIssue(msg);
            }

            if (!timeArgument.HasCorrectTimeStep(timeStep))
            {
                string msg = string.Format(Resources.RainfallRunoffMeteoValidator_ValidateMeteoData_Time_step_of_time_series___0___should_be_a_multiple_of_the_computation_time_step__1_, 
                                           timeArgument.GetMeteoTimeStep(), 
                                           timeStep);
                yield return CreateSingleIssue(msg);
            }
        }
    }
}