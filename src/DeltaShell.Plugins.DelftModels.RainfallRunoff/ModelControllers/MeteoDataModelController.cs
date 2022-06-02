using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using GeoAPI.Extensions.Coverages;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers
{
    public static class MeteoDataModelController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (MeteoDataModelController));

        public static void AddMeteoData(IRRModelHybridFileWriter writer, MeteoData evaporationData, DateTime startDate, DateTime endDate, TimeSpan timeStepModel)
        {
            writer.SetMeteoDataStartTimeAndInterval(RRModelEngineHelper.DateToInt(startDate),
                                                    RRModelEngineHelper.TimeToInt(startDate),
                                                    (int) timeStepModel.TotalSeconds);

            if (!AddEvaporationData(writer, evaporationData, startDate, endDate))
            {
                log.ErrorFormat("It's not possible to set the evaporation data. Check the validation report.");
            }
            
        }

        private static bool AddEvaporationData(IRRModelHybridFileWriter writer, MeteoData evaporationData, DateTime startDate, DateTime endDate)
        {
            if (!ValidateTimeRangeOfMeteoData(evaporationData, startDate, endDate))
                return false;

            double[] evaporationValues;

            switch (evaporationData.DataDistributionType)
            {
                case MeteoDataDistributionType.Global:
                    evaporationValues = evaporationData.Data.GetValues<double>().ToArray();
                    writer.AddEvaporationStation("Global", evaporationValues);
                    return true;
                case MeteoDataDistributionType.PerFeature:
                    if (!(evaporationData.Data is IFeatureCoverage featureCoverage))
                    {
                        return false;
                    }

                    foreach (var feature in featureCoverage.Features)
                    {
                        string featureName = feature is INameable nameable ? nameable.Name : feature.ToString();

                        evaporationValues = evaporationData.MeteoDataDistributed.GetTimeSeries(feature).GetValues<double>().ToArray();
                        writer.AddEvaporationStation(featureName, evaporationValues);
                    }
                    return true;
                case MeteoDataDistributionType.PerStation:
                    var function = evaporationData.Data;
                    if (function == null)
                    {
                        return false;
                    }

                    foreach (var id in function.Arguments[1].Values.Cast<string>())
                    {
                        evaporationValues = evaporationData.MeteoDataDistributed.GetTimeSeries(id).GetValues<double>().ToArray();
                        writer.AddEvaporationStation(id, evaporationValues);
                    }
                    return true;
                default:
                    throw new NotSupportedException("Unknown evaporation data type");
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
    }
}