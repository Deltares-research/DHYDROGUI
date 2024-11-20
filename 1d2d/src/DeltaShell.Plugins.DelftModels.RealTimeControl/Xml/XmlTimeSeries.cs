using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Xml
{
    public class XmlTimeSeries : IXmlTimeSeries
    {
        private const string StrDatePattern = "yyyy-MM-dd";
        private const string StrTimePattern = "HH:mm:ss";

        /// <summary>
        /// InterpolationType = Constant, Linear, None
        /// piInterpolationOptionEnumStringType = "BLOCK" or "LINEAR"; optional default is None?
        /// </summary>
        public InterpolationType InterpolationType { get; set; }

        /// <summary>
        /// InterpolationType = Constant, Linear, Periodic, None
        /// piExtrapolationOptionEnumStringType = "BLOCK" or "PERIODIC"; optional default is None?
        /// </summary>
        public ExtrapolationTimeSeriesType ExtrapolationType { get; set; }

        public TimeSpan PeriodSpan { get; set; }
        public string Name { get; set; }
        public string LocationId { get; set; }
        public string ParameterId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TimeStep { get; set; }
        public TimeSeries TimeSeries { get; set; }

        public XElement GetTimeSeriesXElementForDataConfigFile(XNamespace xNamespace, bool headerOnly)
        {
            var timeSeriesElement = new XElement(xNamespace + "timeSeries", new XAttribute("id", Name));

            if (headerOnly)
            {
                return timeSeriesElement;
            }

            timeSeriesElement.Add(GetPiTimeSeriesXElement(xNamespace));

            return timeSeriesElement;
        }

        public XElement GetTimeSeriesXElementForTimeSeriesFile(XNamespace xNamespace, TimeSpan timeStep)
        {
            var seriesXElement = new XElement(xNamespace + "series");

            seriesXElement.Add(GetHeaderXElement(xNamespace, timeStep));
            seriesXElement.Add(GetEventXElements(xNamespace));

            return seriesXElement;
        }

        private IEnumerable<XElement> GetEventXElements(XNamespace xNamespace)
        {
            if (TimeSeries.Time.Values.Any())
            {
                foreach (DateTime dateTime in TimeSeries.Time.GetValues())
                {
                    yield return GetEventXElement(xNamespace, dateTime);
                }
            }

            // no times and constant extrapolation is constant time series.
            else if (ExtrapolationType == ExtrapolationTimeSeriesType.Constant)
            {
                yield return GetEventXElement(xNamespace, StartTime, true);
                yield return GetEventXElement(xNamespace, EndTime, true);
            }
        }

        private XElement GetHeaderXElement(XNamespace xNamespace, TimeSpan timeSpan)
        {
            TimeSpan timeStep = DetermineTimeStepAndEndDate(timeSpan, out DateTime endDate);

            var headerXElement = new XElement(xNamespace + "header");

            headerXElement.Add(new XElement(xNamespace + "type", "instantaneous"));
            headerXElement.Add(new XElement(xNamespace + "locationId", LocationId));
            headerXElement.Add(new XElement(xNamespace + "parameterId", ParameterId));
            headerXElement.Add(RealTimeControlXmlWriter.GetTimeStepXElement(xNamespace, timeStep));
            headerXElement.Add(new XElement(xNamespace + "startDate", GetDateTimeAttributes(StartTime)));
            headerXElement.Add(new XElement(xNamespace + "endDate", GetDateTimeAttributes(endDate)));
            headerXElement.Add(new XElement(xNamespace + "missVal", GetMissingValue()));
            headerXElement.Add(new XElement(xNamespace + "stationName"));
            headerXElement.Add(new XElement(xNamespace + "units"));

            return headerXElement;
        }

        private TimeSpan DetermineTimeStepAndEndDate(TimeSpan timeStep, out DateTime tableEnd)
        {
            TimeSpan xTimeStep = timeStep;

            tableEnd = EndTime;

            DateTime[] times = TimeSeries.Time.GetValues().ToArray();

            if (ExtrapolationType == ExtrapolationTimeSeriesType.Periodic && PeriodSpan.Ticks > 0)
            {
                // Check Time Steps
                TimeSpan minTableStep = PeriodSpan;

                for (var i = 0; i < times.Length - 1; i++)
                {
                    TimeSpan tableStep = times[i + 1] - times[i];
                    var bufStep = new TimeSpan();

                    if (tableStep.Seconds > 0)
                    {
                        switch (60 % tableStep.Seconds)
                        {
                            case 0:
                                bufStep = new TimeSpan(0, 0, 0, tableStep.Seconds);
                                break;
                            default:
                                bufStep = new TimeSpan(0, 0, 0, 1);
                                break;
                        }
                    }
                    else if (tableStep.Minutes > 0)
                    {
                        switch (60 % tableStep.Minutes)
                        {
                            case 0:
                                bufStep = new TimeSpan(0, 0, tableStep.Minutes, 0);
                                break;
                            default:
                                bufStep = new TimeSpan(0, 0, 1, 0);
                                break;
                        }
                    }
                    else if (tableStep.Hours > 0)
                    {
                        switch (24 % tableStep.Hours)
                        {
                            case 0:
                                bufStep = new TimeSpan(0, tableStep.Hours, 0, 0);
                                break;
                            default:
                                bufStep = new TimeSpan(0, 1, 0, 0);
                                break;
                        }
                    }
                    else if (tableStep.Days > 0)
                    {
                        bufStep = new TimeSpan(1, 0, 0, 0);
                    }

                    minTableStep = bufStep < minTableStep ? bufStep : minTableStep;
                }

                if (minTableStep.Days < 3650)
                {
                    tableEnd = (StartTime + PeriodSpan) - minTableStep;
                    xTimeStep = minTableStep;
                }
            }

            return xTimeStep;
        }

        private XElement GetPiTimeSeriesXElement(XNamespace xNamespace)
        {
            var piTimeSeriesXElement = new XElement(xNamespace + "PITimeSeries",
                                                    new XElement(xNamespace + "locationId", LocationId),
                                                    new XElement(xNamespace + "parameterId", ParameterId),
                                                    GetInterpolationXElement(xNamespace),
                                                    GetExtrapolationXElement(xNamespace)
            );

            return piTimeSeriesXElement;
        }

        private XElement GetInterpolationXElement(XNamespace xNamespace)
        {
            if (InterpolationType == InterpolationType.None)
            {
                return null;
            }

            string interpolationString = InterpolationType == InterpolationType.Constant ? "BLOCK" : "LINEAR";

            var interpolationXElement = new XElement(xNamespace + "interpolationOption", interpolationString);

            return interpolationXElement;
        }

        private XElement GetExtrapolationXElement(XNamespace xNamespace)
        {
            if (ExtrapolationType != ExtrapolationTimeSeriesType.Constant &&
                ExtrapolationType != ExtrapolationTimeSeriesType.Periodic)
            {
                return null;
            }

            string extrapolationString = ExtrapolationType == ExtrapolationTimeSeriesType.Constant ? "BLOCK" : "PERIODIC";

            var extrapolationXElement = new XElement(xNamespace + "extrapolationOption", extrapolationString);

            return extrapolationXElement;
        }

        private XElement GetEventXElement(XNamespace xNamespace, DateTime dateTime, bool useDefault = false)
        {
            XAttribute valueAttribute = useDefault
                                            ? new XAttribute("value", ConvertToDouble(TimeSeries.Components[0].DefaultValue).ToString(CultureInfo.InvariantCulture))
                                            : new XAttribute("value", ConvertToDouble(TimeSeries[dateTime]).ToString(CultureInfo.InvariantCulture));

            var eventElement = new XElement(xNamespace + "event", GetDateTimeAttributes(dateTime), valueAttribute);

            return eventElement;
        }

        private IList<XAttribute> GetDateTimeAttributes(DateTime dateTime)
        {
            var attributes = new List<XAttribute>
            {
                new XAttribute("date", dateTime.ToString(StrDatePattern, DateTimeFormatInfo.InvariantInfo)),
                new XAttribute("time", dateTime.ToString(StrTimePattern, DateTimeFormatInfo.InvariantInfo))
            };

            return attributes;
        }

        private string GetMissingValue()
        {
            if (TimeSeries.Components[0].ValueType == typeof(bool))
            {
                return "-999.0";
            }

            var missingValue = ConvertToDouble(TimeSeries.Components[0].NoDataValue)
                .ToString("0.0", CultureInfo.InvariantCulture);

            return missingValue;
        }

        private static double ConvertToDouble(object value)
        {
            if (value is bool boolean)
            {
                // rtcTools -> boolean true = 0, boolean false = 1
                return boolean ? 0.0 : 1.0;
            }

            return Convert.ToDouble(value);
        }
    }
}