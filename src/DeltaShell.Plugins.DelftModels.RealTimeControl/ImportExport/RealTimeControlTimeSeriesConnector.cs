using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlTimeSeriesConnector
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlTimeSeriesConnector));

        public static void ConnectTimeSeries(IList<TimeSeriesComplexType> timeSeriesElements, IList<ControlGroup> controlGroups)
        {
            if (timeSeriesElements == null || controlGroups == null) return;

            foreach(var timeSeriesElement in timeSeriesElements)
            {
                var timeSeriesItem = timeSeriesElement.header;

                var locationId = timeSeriesItem.locationId;
                var correspondingRuleOrCondition = GetCorrespondingRuleOrCondition(locationId, controlGroups);

                var timeDependentObject = correspondingRuleOrCondition as ITimeDependentRtcObject;

                if (timeDependentObject == null)
                {
                    Log.WarnFormat(Resources.RealTimeControlTimeSeriesConnector_ConnectTimeSeries_Object_with_id___0___does_not_seem_to_be_a_Time_Rule_or_Time_Condition__See_file____1___, locationId, RealTimeControlXMLFiles.XmlTimeSeries);
                    continue;
                }

                var missingValue = timeSeriesItem.missVal;
                var records = timeSeriesElement.@event;

                SetTimeSeriesFromXmlRecords(timeDependentObject.TimeSeries, records, missingValue);
            }
        }

        private static RtcBaseObject GetCorrespondingRuleOrCondition(string locationId, IList<ControlGroup> controlGroups)
        {
            if (controlGroups == null) return null;

            var controlGroup = RealTimeControlXmlReaderHelper.GetControlGroupByElementId(locationId, controlGroups);
            var name = RealTimeControlXmlReaderHelper.GetRuleOrConditionNameFromElementId(locationId);

            return controlGroup.Rules.Concat<RtcBaseObject>(controlGroup.Conditions).FirstOrDefault(o => o.Name == name);
        }

        private static void SetTimeSeriesFromXmlRecords(TimeSeries timeSeries, List<EventComplexType> records,  double missingValue)
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

        private static DateTime CreateDateTimeFromDateAndTime(DateTime date, DateTime time)
        {
            var timeString = time.ToString("HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
            var timeTimeSpan = TimeSpan.Parse(timeString);
            var dateTime = date.Add(timeTimeSpan);
            return dateTime;
        }
    }
}
