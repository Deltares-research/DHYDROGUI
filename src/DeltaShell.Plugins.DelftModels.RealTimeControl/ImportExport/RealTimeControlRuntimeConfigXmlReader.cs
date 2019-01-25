using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using log4net;
using System;
using System.Globalization;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport
{
    public static class RealTimeControlRuntimeConfigXmlReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlRuntimeConfigXmlReader));

        public static void Read(string runtimeConfigFilePath, RealTimeControlModel rtcModel)
        {
            if (!File.Exists(runtimeConfigFilePath))
            {
                Log.ErrorFormat(Resources.RealTimeControlRuntimeConfigXmlReader_Read_File___0___does_not_exist_, runtimeConfigFilePath);
                return;
            }

            if (rtcModel == null) return;

            var runtimeConfigObject = DelftConfigXmlFileParser.Read<RtcRuntimeConfigXML>(runtimeConfigFilePath);
            var settings = runtimeConfigObject.period.Item as UserDefinedRuntimeXML;

            if (settings == null)
            {
                Log.ErrorFormat(Resources.RealTimeControlRuntimeConfigXmlReader_Read_There_is_no_time_data_for_the_RTC_model_in_the_file___0____Time_data_is_set_with_default_values_, RealTimeControlXMLFiles.XmlRuntime);
                return;
            }

            rtcModel.SetStartTime(settings);
            rtcModel.SetStopTime(settings);
            rtcModel.SetTimeStep(settings);
            rtcModel.SetLimitedMemory(runtimeConfigObject);
        }

        private static void SetTimeStep(this ITimeDependentModel rtcModel, UserDefinedRuntimeXML settings)
        {
            var timeStepElement = settings.timeStep;
            var timeStep = GetTimeSpanFromTimeUnit(timeStepElement);
            var timeMultiplier = Convert.ToInt32(timeStepElement.multiplier);
            var timeDivider = Convert.ToInt32(timeStepElement.divider);

            rtcModel.TimeStep = timeStep.MultiplyAndDivideBy(timeMultiplier, timeDivider);
        }

        private static TimeSpan GetTimeSpanFromTimeUnit(TimeStepXML timeStepXml)
        {
            var timeUnit = timeStepXml.unit;
            TimeSpan timeStep;

            switch (timeUnit)
            {
                case timeStepUnitEnumStringType.second:
                    timeStep = new TimeSpan(0, 0, 0, 1);
                    break;
                case timeStepUnitEnumStringType.minute:
                    timeStep = new TimeSpan(0, 0, 1, 0);
                    break;
                case timeStepUnitEnumStringType.hour:
                    timeStep = new TimeSpan(0, 1, 0, 0);
                    break;
                case timeStepUnitEnumStringType.day:
                    timeStep = new TimeSpan(1, 0, 0, 0);
                    break;
                case timeStepUnitEnumStringType.week:
                    timeStep = new TimeSpan(7, 0, 0, 0);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return timeStep;
        }

        private static TimeSpan MultiplyAndDivideBy(this TimeSpan t, int multiplier, int divider)
        {
            return new TimeSpan(t.Ticks * multiplier / divider);
        }

        private static void SetStartTime(this ITimeDependentModel rtcModel, UserDefinedRuntimeXML settings)
        {
            var startDateElement = settings.startDate;
            rtcModel.StartTime = CreateDateTimeFromDateAndTime(startDateElement.date, startDateElement.time);
        }

        private static void SetStopTime(this ITimeDependentModel rtcModel, UserDefinedRuntimeXML settings)
        {
            var endDateElement = settings.endDate;
            rtcModel.StopTime = CreateDateTimeFromDateAndTime(endDateElement.date, endDateElement.time);
        }

        private static DateTime CreateDateTimeFromDateAndTime(DateTime date, DateTime time)
        {
            var timeString = time.ToString("HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
            var timeTimeSpan = TimeSpan.Parse(timeString);
            var dateTime = date.Add(timeTimeSpan);
            return dateTime;
        }

        private static void SetLimitedMemory(this RealTimeControlModel rtcModel, RtcRuntimeConfigXML runtimeConfigObject)
        {
            var mode = runtimeConfigObject.Item as ModeXML;
            var simulationMode = mode?.Item as ModeSimulationXML;

            if (simulationMode != null) rtcModel.LimitMemory = simulationMode.limitedMemory;
        }
    }
}
