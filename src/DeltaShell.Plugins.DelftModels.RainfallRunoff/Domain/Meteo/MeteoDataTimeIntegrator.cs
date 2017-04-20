using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    public class MeteoDataTimeIntegrator : IMeteoTimeAggregator
    {
        public double[] GetTimeSeriesForPeriod(Variable<double> valueVariable, Variable<DateTime> timeVariable,
                                               DateTime startDate, DateTime endDate, TimeSpan timeStep)
        {
            if (valueVariable.Values.Count != timeVariable.Values.Count)
            {
                throw new ArgumentException("given values and times cannot originate from a time series");
            }

            var times = timeVariable.Values;
            bool isPeriodic = timeVariable.ExtrapolationType == ExtrapolationType.Periodic;

            if (times.Count > 1 && !isPeriodic)
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
                            returnValues[j++] = valueVariable.Values[i];
                        }
                        return returnValues;
                    }
                }
            }
            return GetMeteoForPeriodInternal(startDate, endDate, timeStep, times, valueVariable.Values, isPeriodic);
        }

        

        internal static double[] GetMeteoForPeriodInternal(DateTime startDate, DateTime endDate, TimeSpan timeStep,
                                                           IList<DateTime> times, IList<double> values, bool isPeriodic)
        {
            
            if (isPeriodic)
            {
                ExtendToLeapPeriod(ref times, ref values);
            }

            var results = new List<double>();
            for (var currentTimeStep = startDate; currentTimeStep <= endDate; currentTimeStep += timeStep)
            {
                var endTimePeriod = !isPeriodic && currentTimeStep >= endDate
                                        ? currentTimeStep.Add(times[1] - times[0])
                                        : currentTimeStep.Add(timeStep);

                results.Add(GetIntegralPerPeriod(times, values, currentTimeStep, endTimePeriod, isPeriodic));
            }
            return results.ToArray();
        }

        internal static double GetIntegralPerPeriod(IList<DateTime> times, IList<double> values,
                                                       DateTime startTimePeriod, DateTime endTimePeriod, bool isPeriodic)
        {
            double startFraction;
            double endFraction;

            var startIndex = GetIndexFor(startTimePeriod, times, isPeriodic, out startFraction);
            var endIndex = GetIndexFor(endTimePeriod, times, isPeriodic, out endFraction);

            var timeStep = times[1] - times[0];
            var totalTime = times[times.Count - 1] - times[0] + timeStep;
                // Add timestep because last value has a duration of 1 timestep

            var numberOfCompletePeriods = (int) ((endTimePeriod - startTimePeriod).Ticks/totalTime.Ticks);

            var indices = GetIndices(startIndex, endIndex, numberOfCompletePeriods, times.Count, isPeriodic).ToList();

            if (indices.Count == 1)
            {
                if (endFraction < startFraction)
                {
                    return values[startIndex];
                }
                return (endFraction - startFraction)*values[startIndex];
            }

            var startValue = (1 - startFraction)*values[startIndex];
            var endValue = endFraction*values[endIndex];
            var returnValues = indices.Select(i => values[i]).Sum();

            // substitute the start and end value with the fraction value
            returnValues -= values[startIndex];
            returnValues -= values[endIndex];
            returnValues += startValue + endValue;

            return returnValues;
        }

        private static void ExtendToLeapPeriod(ref IList<DateTime> times, ref IList<double> values)
        {
            var timeStep = times[1] - times[0];

            // If the time step is not a divisor of a single day, return.
            if (new TimeSpan(1, 0, 0, 0).Ticks % timeStep.Ticks != 0)
            {
                return;
            }

            var timeRange = times.Last() - times[0] + timeStep;

            var startYear = times[0].Year;

            var startYears = new[] { startYear, startYear + 1, startYear + 2, startYear + 3 };

            // Reference data is non-leap year:
            if (timeRange.TotalDays == 365)
            {
                if (startYears.Any(DateTime.IsLeapYear))
                {
                    var timesList = new List<DateTime>(times);

                    var nonLeapTimes = new List<DateTime>(times);
                    timesList.AddRange(nonLeapTimes.Select(t => t.AddYears(1)));
                    timesList.AddRange(nonLeapTimes.Select(t => t.AddYears(2)));
                    timesList.AddRange(nonLeapTimes.Select(t => t.AddYears(3)));

                    var valuesList = new List<double>(values);
                    valuesList.AddRange(values);
                    valuesList.AddRange(values);
                    valuesList.AddRange(values);

                    var leapIndex = startYears.ToList().FindIndex(DateTime.IsLeapYear);

                    if (times[0].Month > 2)
                    {
                        leapIndex--;
                    }

                    var startIndex = timesList.FindIndex(d => (d.Day == 28 && d.Month == 2)) + leapIndex * 365;
                    var endIndex = timesList.FindIndex(d => d.Month == 3) + leapIndex * 365;

                    if (startIndex == -1 || endIndex == -1)
                    {
                        return;
                    }

                    var elementsToCopy =
                        timesList.GetRange(startIndex, endIndex - startIndex).Select(t => t.AddDays(1)).ToList();
                    timesList.InsertRange(endIndex, elementsToCopy);

                    var valuesToCopy = valuesList.GetRange(startIndex, endIndex - startIndex).ToList();
                    valuesList.InsertRange(endIndex, valuesToCopy);

                    times = timesList;
                    values = valuesList;
                }
            }

            // Reference data is leap year
            if (timeRange.TotalDays == 366)
            {
                var timesList = new List<DateTime>(times);
                var startIndex = timesList.FindIndex(d => (d.Day == 29 && d.Month == 2));
                var endIndex = timesList.FindIndex(d => d.Month == 3);

                var nonLeapTimes = new List<DateTime>(times);
                nonLeapTimes.RemoveRange(startIndex, endIndex - startIndex);

                timesList.AddRange(nonLeapTimes.Select(t => t.AddYears(1)));
                timesList.AddRange(nonLeapTimes.Select(t => t.AddYears(2)));
                timesList.AddRange(nonLeapTimes.Select(t => t.AddYears(3)));

                var valuesList = new List<double>(values);
                var nonLeapValues = values.ToList();
                nonLeapValues.RemoveRange(startIndex, endIndex - startIndex);

                valuesList.AddRange(nonLeapValues);
                valuesList.AddRange(nonLeapValues);
                valuesList.AddRange(nonLeapValues);

                times = timesList;
                values = valuesList;
            }
        }

        private static IEnumerable<int> GetIndices(int startIndex, int endIndex, int numberOfCompletePeriods,
                                                   int numberOfTimes, bool isPeriodic)
        {
            var fullPeriodIndices =
                Enumerable.Range(1, numberOfCompletePeriods).SelectMany(n => Enumerable.Range(0, numberOfTimes - 1));

            return !isPeriodic || (numberOfCompletePeriods == 0 && endIndex > startIndex)
                       ? (endIndex != startIndex
                              ? Enumerable.Range(startIndex, endIndex - startIndex + 1)
                              : new[] {startIndex})
                       : Enumerable.Range(startIndex, numberOfTimes - startIndex)
                                   .Concat(fullPeriodIndices)
                                   .Concat(Enumerable.Range(0, endIndex + 1));
        }

        internal static int GetIndexFor(DateTime time, IList<DateTime> times, bool periodic, out double fraction)
        {
            var first = times[0];
            var last = times[times.Count - 1];
            var timeStep = times[1] - first;

            double fractionalIndex;

            if (!periodic)
            {
                if (time < first)
                {
                    throw new ArgumentException("No value specified for time : " + time);
                }

                if (time >= (last + timeStep))
                {
                    // repeat last index 
                    fraction = 1;
                    return times.Count - 1;
                }

                fractionalIndex = ((double) (time - first).Ticks)/timeStep.Ticks;
            }
            else
            {
                var totalTime = (last - first) + timeStep;

                var timeTicks = time.Ticks - first.Ticks;

                var shiftedTimeTicks = timeTicks%totalTime.Ticks;
                if (shiftedTimeTicks < 0)
                {
                    shiftedTimeTicks += totalTime.Ticks;
                }
                fractionalIndex = shiftedTimeTicks/((double)timeStep.Ticks);
            }

            var index = (int) fractionalIndex;
            fraction = fractionalIndex - index;

            return index;
        }
    }
}
