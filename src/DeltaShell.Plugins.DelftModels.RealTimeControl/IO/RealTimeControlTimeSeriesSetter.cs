using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>
    /// Responsible for setting the time series from the Time Series elements on Time Dependent RTC Objects.
    /// </summary>
    public class RealTimeControlTimeSeriesSetter
    {
        private readonly ILogHandler logHandler;

        public RealTimeControlTimeSeriesSetter(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Sets the time series from the Time Series elements on Time Dependent RTC Objects.
        /// </summary>
        /// <param name="timeSeriesElements">The Time Series elements.</param>
        /// <param name="controlGroups">The control groups.</param>
        /// <remarks>If parameter timeSeriesElements or controlGroups is NULL, methods returns.</remarks>
        public void SetTimeSeries(IList<TimeSeriesComplexType> timeSeriesElements, IList<IControlGroup> controlGroups)
        {
            if (timeSeriesElements == null || controlGroups == null)
            {
                return;
            }

            foreach (TimeSeriesComplexType timeSeriesElement in timeSeriesElements)
            {
                HeaderComplexType timeSeriesItem = timeSeriesElement.header;
                string locationId = timeSeriesItem.locationId;
                double missingValue = timeSeriesItem.missVal;
                EventComplexType[] records = timeSeriesElement.@event;

                RtcBaseObject correspondingRuleOrCondition = GetCorrespondingRuleOrCondition(locationId, controlGroups);

                if (!(correspondingRuleOrCondition is ITimeDependentRtcObject timeDependentObject))
                {
                    logHandler.ReportWarningFormat(Resources.RealTimeControlTimeSeriesConnector_ConnectTimeSeries_Object_with_id___0___does_not_seem_to_be_a_Time_Rule_or_Time_Condition__See_file____1___, locationId, RealTimeControlXmlFiles.XmlTimeSeries);
                    continue;
                }

                if (!UsesTimeSeries(correspondingRuleOrCondition))
                {
                    logHandler.ReportWarningFormat(
                        Resources
                            .RealTimeControlTimeSeriesConnector_ConnectTimeSeries_Rule__with_id___0___does_not_seem_to_use_a_time_serie_as_setpoint__See_file____1___Therefore_the_time_serie_is_not_imported,
                        locationId, RealTimeControlXmlFiles.XmlTimeSeries);
                    continue;
                }

                if (correspondingRuleOrCondition is IntervalRule intervalRule)
                {
                    SetFixedOrVariableIntervalRule(records, intervalRule, missingValue);
                }
                else
                {
                    SetTimeSeriesFromXmlRecords(timeDependentObject.TimeSeries, records, missingValue);
                }
            }
        }

        private static bool UsesTimeSeries(RtcBaseObject rtcObject)
        {
            if (rtcObject is IntervalRule intervalRule &&
                intervalRule.SetPointType == IntervalRule.IntervalRuleSetPointType.Signal)
            {
                return false;
            }

            if (rtcObject is PIDRule pidRule &&
                pidRule.PidRuleSetpointType != PIDRule.PIDRuleSetpointType.TimeSeries)
            {
                return false;
            }

            return true;
        }

        private void SetFixedOrVariableIntervalRule(IReadOnlyCollection<EventComplexType> records, IntervalRule intervalRule, double missingValue)
        {
            double? possibleFixedValue = records?.FirstOrDefault()?.value;

            if (possibleFixedValue != null)
            {
                if (records.Select(r => r.value).Distinct().Count() == 1)
                {
                    intervalRule.SetPointType = IntervalRule.IntervalRuleSetPointType.Fixed;
                    intervalRule.TimeSeries.Components[0].DefaultValue = possibleFixedValue;
                }
                else
                {
                    intervalRule.SetPointType = IntervalRule.IntervalRuleSetPointType.Variable;
                    SetTimeSeriesFromXmlRecords(intervalRule.TimeSeries, records, missingValue);
                }
            }
            else
            {
                logHandler.ReportWarningFormat(
                    Resources
                        .RealTimeControlTimeSeriesSetter_For_interval_rule_with_id__0__there_is_no_time_data_found_in_file__1__for_setting_fixed_or_variable_setpoint_type_Setpoint_type_will_be_variable__,
                    intervalRule.Name, RealTimeControlXmlFiles.XmlTimeSeries);
            }
        }
        
        private RtcBaseObject GetCorrespondingRuleOrCondition(string locationId, IEnumerable<IControlGroup> controlGroups)
        {
            IControlGroup controlGroup = controlGroups?.GetControlGroupByElementId(locationId, logHandler);

            if (controlGroup == null)
            {
                return null;
            }

            string name = RealTimeControlXmlReaderHelper.GetComponentNameFromElementId(locationId);

            return controlGroup.Rules.Concat<RtcBaseObject>(controlGroup.Conditions).FirstOrDefault(o => o.Name == name);
        }

        private void SetTimeSeriesFromXmlRecords(TimeSeries timeSeries, IReadOnlyCollection<EventComplexType> records, double missingValue)
        {
            if (timeSeries == null || records == null)
            {
                return;
            }

            IEnumerable<DateTime> dates = records.Select(r => CreateDateTimeFromDateAndTime(r.date, r.time));
            IEnumerable<double> doubleValues = records.Select(r => r.value);

            timeSeries.Time.SetValues(dates);

            if (timeSeries.Components[0].ValueType == typeof(bool))
            {
                // because we write the opposite ( 0 = true, 1 = false)
                IEnumerable<bool> booleanValues = doubleValues.Select(e => !Convert.ToBoolean(e));
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
            TimeSpan timeTimeSpan = TimeSpan.Parse(timeString);
            DateTime dateTime = date.Add(timeTimeSpan);
            return dateTime;
        }
    }
}