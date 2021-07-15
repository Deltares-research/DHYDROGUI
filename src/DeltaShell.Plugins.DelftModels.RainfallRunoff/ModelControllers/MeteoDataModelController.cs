using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers
{
    public static class MeteoDataModelController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (MeteoDataModelController));

        public static bool AddMeteoData(IRRModelHybridFileWriter writer, MeteoData evaporationData, MeteoData tempData, DateTime startDate, DateTime endDate, TimeSpan timeStepModel)
        {
            bool returnValue = true;

            writer.SetMeteoDataStartTimeAndInterval(RRModelEngineHelper.DateToInt(startDate),
                                                      RRModelEngineHelper.TimeToInt(startDate),
                                                      (int) timeStepModel.TotalSeconds);

            if (!AddEvaporationData(writer, evaporationData, startDate, endDate))
            {
                log.ErrorFormat("It's not possible to set the evaporation data. Check the validation report.");
                returnValue = false;
            }
            if (tempData != null && !AddTemperatureData(writer, tempData, startDate, endDate, timeStepModel))
            {
                log.ErrorFormat("It's not possible to set the temperature data. Check the validation report.");
                returnValue = false;
            }

            return returnValue;
        }

        private static bool AddEvaporationData(IRRModelHybridFileWriter writer, MeteoData evaporationData, DateTime startDate, DateTime endDate)
        {
            if (!ValidateTimeRangeOfMeteoData(evaporationData, startDate, endDate))
                return false;

            double[] evapor;

            switch (evaporationData.DataDistributionType)
            {
                case MeteoDataDistributionType.Global:
                    evapor = GetMeteoForPeriod(evaporationData, startDate, endDate, new TimeSpan(1, 0, 0, 0));
                    writer.AddEvaporationStation("Global", evapor.Concat(new[] {0.0}).ToArray());
                    return true;
                case MeteoDataDistributionType.PerFeature:
                    var featureCoverage = evaporationData.Data as IFeatureCoverage;
                    if (featureCoverage != null)
                    {
                        foreach (var feature in featureCoverage.Features)
                        {
                            evapor = GetMeteoForPeriod(evaporationData, startDate, endDate, new TimeSpan(1, 0, 0, 0), feature);
                            writer.AddEvaporationStation(((INameable) feature).Name,evapor.Concat(new[] {0.0}).ToArray());
                        }
                        return true;
                    }
                    return false;
                case MeteoDataDistributionType.PerStation:
                    var function = evaporationData.Data;
                    if (function != null)
                    {
                        foreach (var id in function.Arguments[1].Values.Cast<string>())
                        {
                            evapor = GetMeteoForPeriod(evaporationData, startDate, endDate, new TimeSpan(1, 0, 0, 0), id);
                            writer.AddEvaporationStation(id, evapor.Concat(new[] {0.0}).ToArray());
                        }
                        return true;
                    }
                    return false;
                default:
                    throw new NotSupportedException("Unknown evaporation data type");
            }
        }

        private static bool AddTemperatureData(IRRModelHybridFileWriter writer, MeteoData temperatureData, DateTime startDate, DateTime endDate, TimeSpan timeStep)
        {
            if (!ValidateTimeRangeOfMeteoData(temperatureData, startDate, endDate))
                return false;

            double[] temp;

            switch (temperatureData.DataDistributionType)
            {
                case MeteoDataDistributionType.Global:
                    temp = GetMeteoForPeriod(temperatureData, startDate, endDate, timeStep);
                    writer.AddTemperatureStation("Global", temp);
                    return true;
                case MeteoDataDistributionType.PerFeature:
                    var featureCoverage = temperatureData.Data as IFeatureCoverage;
                    if (featureCoverage != null)
                    {
                        foreach (var feature in featureCoverage.Features)
                        {
                            temp = GetMeteoForPeriod(temperatureData, startDate, endDate, timeStep, feature);
                            writer.AddTemperatureStation(((INameable)feature).Name, temp);
                        }
                        return true;
                    }
                    return false;
                case MeteoDataDistributionType.PerStation:
                    var function = temperatureData.Data;
                    if (function != null)
                    {
                        foreach (var id in function.Arguments[1].Values.Cast<string>())
                        {
                            temp = GetMeteoForPeriod(temperatureData, startDate, endDate, timeStep, id);
                            writer.AddTemperatureStation(id, temp);
                        }
                        return true;
                    }
                    return false;
                default:
                    throw new NotSupportedException("Unknown temperature data type");
            }
        }

        private static bool ValidateTimeRangeOfMeteoData(MeteoData meteoData, DateTime modelStartDateTime, DateTime modelEndDateTime)
        {
            var timeArgument = (IVariable<DateTime>) meteoData.Data.Arguments.FirstOrDefault(a => a.ValueType == typeof (DateTime));

            if (timeArgument == null || timeArgument.Values.Count < 2)
            {
                return false;
            }

            DateTime lastTime = timeArgument.Values[timeArgument.Values.Count - 1];
            TimeSpan meteoTimestep = lastTime.Subtract(timeArgument.Values[timeArgument.Values.Count - 2]);
            DateTime meteoEnd = lastTime.Add(meteoTimestep);

            if (timeArgument.ExtrapolationType == ExtrapolationType.None &&
                (timeArgument.MinValue > modelStartDateTime || meteoEnd < modelEndDateTime))
            {
                return false;
            }

            return true;
        }

        private static double[] GetMeteoForPeriod(MeteoData meteoData, DateTime startDate, DateTime endDate, TimeSpan timeStep, object feature = null)
        {
            return meteoData.GetMeteoForPeriod(startDate, endDate, timeStep, feature);
        }
    }
}