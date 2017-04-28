using System;
using System.Collections.Generic;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    class MeteoDataTimeInterpolator: IMeteoTimeAggregator
    {
        public double[] GetTimeSeriesForPeriod(Variable<double> valueVariable, Variable<DateTime> timeVariable,
                                               DateTime startDate, DateTime endDate, TimeSpan timeStep)
        {
            if (valueVariable.Values.Count != timeVariable.Values.Count)
            {
                throw new ArgumentException("given values and times cannot originate from a time series");
            }

            var times = timeVariable.Values;
            if (times.Count > 1 && timeVariable.ExtrapolationType != ExtrapolationType.Periodic)
            {
                //shortcut: same timestep, assumption: input time series is equidistant
                var timeSeriesTimestep = times[1] - times[0];

                if (timeSeriesTimestep == timeStep)
                {
                    int startIndex = times.IndexOf(startDate);
                    int endIndex = times.IndexOf(endDate);
                    //shortcut: aligned series
                    if (startIndex > -1 && endIndex > -1)
                    {
                        var returnValues = new double[(endIndex - startIndex) + 1];
                        int j = 0;
                        for (int i = startIndex; i <= endIndex; i++)
                        {
                            returnValues[j++] = ((IList<double>) valueVariable.Values)[i];
                        }
                        return returnValues;
                    }
                }
            }

            var results = new List<double>();
            for (var currentTimeStep = startDate; currentTimeStep <= endDate; currentTimeStep += timeStep)
            {
                results.Add(
                    valueVariable.Evaluate<double>(new VariableValueFilter<DateTime>(timeVariable, currentTimeStep)));
            }
            return results.ToArray();
        }
    }
}
