using System;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Converters
{
    public class XmlTimeSeriesTruncater
    {
        /// <summary>
        /// Truncates the given timeseries if needed. This is based on comparing the given start time and end time with
        /// the minimum and maximum times that are present in <param name="timeSeries" />. 
        /// </summary>
        /// <param name="timeSeries"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static void Truncate(IXmlTimeSeries xmlTimeSeries, DateTime startTime, DateTime endTime)
        {
            var timeSeriesStart = xmlTimeSeries.TimeSeries.Time.Values.First();
            var timeSeriesEnd = xmlTimeSeries.TimeSeries.Time.Values.Last();

            if ((DateTime.Compare(timeSeriesStart, startTime) < 0) ||
                (DateTime.Compare(endTime, timeSeriesEnd) < 0))
            {
                var startValue = xmlTimeSeries.TimeSeries.Evaluate<double>(startTime);
                var endValue = xmlTimeSeries.TimeSeries.Evaluate<double>(endTime);

                // filter out any values before startTime
                bool proceed = true;
                while (proceed)
                {
                    if (!xmlTimeSeries.TimeSeries.Time.Values.Any()) proceed = false;
                    else
                    {
                        if (xmlTimeSeries.TimeSeries.Time.Values.First() < startTime)
                            xmlTimeSeries.TimeSeries.Time.Values.RemoveAt(0);
                        else proceed = false;
                    }
                }
                // filter out any values after endTime
                proceed = true;
                while (proceed)
                {
                    if (!xmlTimeSeries.TimeSeries.Time.Values.Any()) proceed = false;
                    else
                    {
                        if (xmlTimeSeries.TimeSeries.Time.Values.Last() > endTime)
                        {
                            int len = xmlTimeSeries.TimeSeries.Time.Values.Count;
                            xmlTimeSeries.TimeSeries.Time.Values.RemoveAt(len - 1);
                        }
                        else proceed = false;
                    }
                }
                // (re)set values at startTime and endTime
                xmlTimeSeries.TimeSeries[startTime] = startValue;
                xmlTimeSeries.TimeSeries[endTime] = endValue;
            }
            else
            {
                xmlTimeSeries.StartTime = xmlTimeSeries.TimeSeries.Time.Values.First();
                xmlTimeSeries.EndTime = xmlTimeSeries.TimeSeries.Time.Values.Last();
            }

            xmlTimeSeries.TimeStep = xmlTimeSeries.TimeSeries.Time.Values.Last() - xmlTimeSeries.TimeSeries.Time.Values.First();
        }
    }
}
