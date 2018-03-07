using System;
using System.Collections.Generic;
using nl.wldelft.util.timeseries;

namespace Deltares.IO.FewsPI
{
    public class TimeSeries
    {
        private readonly TimeSeriesArray timeSeries;

        public TimeSeries(TimeSeriesArray timeSeries)
        {
            this.timeSeries = timeSeries;
        }

        public void Add(DateTime dateTime, double value)
        {
            long javaTimeInMillies = Java2DotNetHelper.JavaMilliesFromDotNetDateTime(dateTime);
            timeSeries.put(javaTimeInMillies, (float)value);
        }

        public IEnumerable<TimeEvent> Events
        {
            get
            {
                double[] doubleArray = timeSeries.toDoubleArray();
                long[] timesAsMillisArray = timeSeries.toTimesArray();
                for (int index = 0; index < timesAsMillisArray.Length; index++)
                {
                    if (!timeSeries.isMissingValue(index))
                    {
                        DateTime dateTime = Java2DotNetHelper.DotNetDateTimeFromJavaMillies(timesAsMillisArray[index]);
                        double value = doubleArray[index];
                        yield return new TimeEvent{ Time = dateTime, Value = value };
                    }
                }
            }
        }

        public string ParameterId 
        {
            get { return timeSeries.getHeader().getParameterId(); }
        }

        public string LocationId
        {
            get { return timeSeries.getHeader().getLocationId(); }
        }

        public int Count
        {
            get { return timeSeries.size(); }
        }

        /// <summary>
        /// Gets a CONSTANT VALUE of -999! TODO: return the missing value from the pi run info
        /// </summary>
        public double MissingValue 
        {
            get { return -999.0; }
        }

        public TimeSpan TimeStep
        {
            get { return Java2DotNetHelper.DotNetTimeSpanFromJavaMillies(timeSeries.getTimeStep().getStepMillis());  }
        }

        public string Unit
        {
            get { return timeSeries.getHeader().getUnit(); }
        }

        public void Clear()
        {
            timeSeries.clear();
        }
    }
}