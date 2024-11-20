using System;
using System.Linq;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Converters
{
    public static class XmlTimeSeriesTruncater
    {
        /// <summary>
        /// Truncates the given timeseries if needed. This is based on comparing the given start time and end time with
        /// the minimum and maximum times that are present in
        /// <param name="timeSeries"/>
        /// .
        /// </summary>
        /// <param name="timeSeries"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static void Truncate(IXmlTimeSeries xmlTimeSeries, DateTime startTime, DateTime endTime)
        {
            DateTime timeSeriesStart = xmlTimeSeries.TimeSeries.Time.Values.First();
            DateTime timeSeriesEnd = xmlTimeSeries.TimeSeries.Time.Values.Last();

            if (DateTime.Compare(timeSeriesStart, startTime) < 0 ||
                DateTime.Compare(endTime, timeSeriesEnd) < 0)
            {
                var startValue = xmlTimeSeries.TimeSeries.Evaluate<double>(startTime);
                var endValue = xmlTimeSeries.TimeSeries.Evaluate<double>(endTime);

                // filter out any values before startTime and/or after endTime
                xmlTimeSeries.TimeSeries.Time.Values.RemoveAllWhere(v => v < startTime || v > endTime);

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