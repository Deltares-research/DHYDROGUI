using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    /// <summary>
    /// Responsible for setting the time series from the Time Series XML elements on Time Dependent RTC Objects.
    /// </summary>
    public class RealTimeControlTimeSeriesSetter
    {
        private readonly ILogHandler logHandler;

        public RealTimeControlTimeSeriesSetter(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Sets the time series from the Time Series XML elements on Time Dependent RTC Objects.
        /// </summary>
        /// <param name="timeSeriesElements">The Time Series XML elements.</param>
        /// <param name="controlGroups">The control groups.</param>
        /// <remarks>If parameter timeSeriesElements or controlGroups is NULL, methods returns.</remarks>
        public void SetTimeSeries(IList<TimeSeriesComplexType> timeSeriesElements, IList<IControlGroup> controlGroups)
        {
            if (timeSeriesElements == null || controlGroups == null) return;

            foreach(var timeSeriesElement in timeSeriesElements)
            {
                var timeSeriesItem = timeSeriesElement.header;

                var locationId = timeSeriesItem.locationId;
                var correspondingRuleOrCondition = GetCorrespondingRuleOrCondition(locationId, controlGroups);

                if (!(correspondingRuleOrCondition is ITimeDependentRtcObject timeDependentObject))
                {
                    logHandler.ReportWarningFormat(Resources.RealTimeControlTimeSeriesConnector_ConnectTimeSeries_Object_with_id___0___does_not_seem_to_be_a_Time_Rule_or_Time_Condition__See_file____1___, locationId, RealTimeControlXMLFiles.XmlTimeSeries);
                    continue;
                }

                var missingValue = timeSeriesItem.missVal;
                var records = timeSeriesElement.@event;

                SetTimeSeriesFromXmlRecords(timeDependentObject.TimeSeries, records, missingValue);
            }
        }

        private RtcBaseObject GetCorrespondingRuleOrCondition(string locationId, IEnumerable<IControlGroup> controlGroups)
        {
            var controlGroup = controlGroups?.GetControlGroupByElementId(locationId, logHandler);

            if (controlGroup == null) return null;

            var name = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(locationId);

            return controlGroup.Rules.Concat<RtcBaseObject>(controlGroup.Conditions).FirstOrDefault(o => o.Name == name);
        }

        private void SetTimeSeriesFromXmlRecords(TimeSeries timeSeries, IReadOnlyCollection<EventComplexType> records, double missingValue)
        {
            if (timeSeries == null || records == null) return;

            var dates = records.Select(r => CreateDateTimeFromDateAndTime(r.date, r.time));
            var doubleValues = records.Select(r => r.value);

            timeSeries.Time.SetValues(dates);

            if (timeSeries.Components[0].ValueType == typeof(bool))
            {
                // because we write the opposite ( 0 = true, 1 = false)
                var booleanValues = doubleValues.Select(e => !Convert.ToBoolean(e));
                timeSeries.Components[0].SetValues(booleanValues);
            }
            else
            {
                timeSeries.Components[0].SetValues(doubleValues);
                timeSeries.Components[0].NoDataValue = missingValue;
            }
        }

        private DateTime CreateDateTimeFromDateAndTime(DateTime date, DateTime time)
        {
            var timeString = time.ToString("HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
            var timeTimeSpan = TimeSpan.Parse(timeString);
            var dateTime = date.Add(timeTimeSpan);
            return dateTime;
        }
    }
}
