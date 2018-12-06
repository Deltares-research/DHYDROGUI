using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Xml
{
    public class XmlTimeSeries: IXmlTimeSeries
    {
        public string Name { get; set; }
        public string LocationId { get; set; }
        public string ParameterId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TimeStep { get; set; }
        public TimeSeries TimeSeries { get; set; }

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

        public XElement ToDataConfigXml(XNamespace xNamespace, bool headerOnly)
        {
            if (headerOnly)
            {
                return new XElement(xNamespace + "timeSeries", new XAttribute("id", Name));
            }
            var timeSeriesElement = new XElement(xNamespace + "timeSeries", new XAttribute("id", Name));
            var piTimeSeriesElement = new XElement(xNamespace + "PITimeSeries",
                                                   new XElement(xNamespace + "locationId", LocationId),
                                                   new XElement(xNamespace + "parameterId", ParameterId));
            timeSeriesElement.Add(piTimeSeriesElement);
            if ((InterpolationType == InterpolationType.Constant) || (InterpolationType == InterpolationType.Linear))
            {
                piTimeSeriesElement.Add(new XElement(xNamespace + "interpolationOption",
                                                   (InterpolationType == InterpolationType.Constant)
                                                       ? "BLOCK"
                                                       : "LINEAR"));
            }
            if ((ExtrapolationType == ExtrapolationTimeSeriesType.Constant) || (ExtrapolationType == ExtrapolationTimeSeriesType.Periodic))
            {
                piTimeSeriesElement.Add(new XElement(xNamespace + "extrapolationOption",
                                                   (ExtrapolationType == ExtrapolationTimeSeriesType.Constant)
                                                       ? "BLOCK"
                                                       : "PERIODIC"));
            }
            return timeSeriesElement;
        }

        public XElement ToTimeSeriesXml(XNamespace xNamespace, TimeSpan timeStep)
        {
            const string strDatePattern = "yyyy-MM-dd";
            const string strTimePattern = "HH:mm:ss";

            string missingValue = GetMissingValue();

            var xTimeStep = timeStep;
            var tableEnd = EndTime;

            var times = TimeSeries.Time.GetValues().ToArray();

            if (ExtrapolationType == ExtrapolationTimeSeriesType.Periodic  &&
                PeriodSpan.Ticks > 0)
            {

                // Check Time Steps
                var minTableStep = PeriodSpan;

                for (var i = 0; i < times.Count() - 1; i++)
                {
                    var tableStep = (times[i + 1] - times[i]);
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
                    tableEnd = StartTime + PeriodSpan - minTableStep;
                    xTimeStep = minTableStep;
                }
            }
            else
            {
                xTimeStep = timeStep;
                tableEnd = EndTime;
            }

            var xElementHeader = new XElement(xNamespace + "header",
                                        new XElement(xNamespace + "type", "instantaneous"),
                                        new XElement(xNamespace + "locationId", LocationId),
                                        new XElement(xNamespace + "parameterId", ParameterId),
                                        // time in time series has to match time step in model
                                        RealTimeControlXmlWriter.TimeStepToXml(xNamespace, xTimeStep),
                                        new XElement(xNamespace + "startDate",
                                            new XAttribute("date", StartTime.ToString(strDatePattern, DateTimeFormatInfo.InvariantInfo)),
                                            new XAttribute("time", StartTime.ToString(strTimePattern, DateTimeFormatInfo.InvariantInfo))),
                                        new XElement(xNamespace + "endDate",
                                            new XAttribute("date", tableEnd.ToString(strDatePattern, DateTimeFormatInfo.InvariantInfo)),
                                            new XAttribute("time", tableEnd.ToString(strTimePattern, DateTimeFormatInfo.InvariantInfo))),
                                        new XElement(xNamespace + "missVal", missingValue),
                                        new XElement(xNamespace + "stationName"),
                                        new XElement(xNamespace + "units")
                                        );

            var returnXElement = new XElement(xNamespace + "series");
            returnXElement.Add(xElementHeader);

            // TimeSeries Data
            var xElementEvents = TimeSeries.Time.GetValues().Select(timestep =>
                new XElement(xNamespace + "event", 
                    new XAttribute("date", timestep.ToString(strDatePattern, DateTimeFormatInfo.InvariantInfo)),
                    new XAttribute("time", timestep.ToString(strTimePattern, DateTimeFormatInfo.InvariantInfo)),
                    new XAttribute("value", (ConvertToDouble(TimeSeries[timestep])).ToString(CultureInfo.InvariantCulture)))).ToList();
            returnXElement.Add(xElementEvents);

            // No times and constant extrapolation is constant time series.
            if ((ExtrapolationType == ExtrapolationTimeSeriesType.Constant) && (times.Count() == 0))
            {
                returnXElement.Add(new XElement(xNamespace + "event",
                    new XAttribute("date", StartTime.ToString(strDatePattern, DateTimeFormatInfo.InvariantInfo)),
                    new XAttribute("time", StartTime.ToString(strTimePattern, DateTimeFormatInfo.InvariantInfo)),
                    new XAttribute("value", (ConvertToDouble(TimeSeries.Components[0].DefaultValue)).ToString(CultureInfo.InvariantCulture))));

                returnXElement.Add(new XElement(xNamespace + "event",
                    new XAttribute("date", EndTime.ToString(strDatePattern, DateTimeFormatInfo.InvariantInfo)),
                    new XAttribute("time", EndTime.ToString(strTimePattern, DateTimeFormatInfo.InvariantInfo)),
                    new XAttribute("value", (ConvertToDouble(TimeSeries.Components[0].DefaultValue)).ToString(CultureInfo.InvariantCulture))));
            }

            return returnXElement;
        }

        private string GetMissingValue()
        {
            var missingValue = (ConvertToDouble(TimeSeries.Components[0].NoDataValue)).ToString("0.0",
                                                                                                CultureInfo.
                                                                                                    InvariantCulture);
            if(TimeSeries.Components[0].ValueType == typeof(bool))
            {
                missingValue = "-999.0";
            }
            return missingValue;
        }

        private double ConvertToDouble(object value)
        {
            if(value is bool)
            {
                //   !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //   rtcTools -> boolean true = 0, boolean false = 1
                //   !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                return ((bool) value) ? 0.0 : 1.0;
            }

            return Convert.ToDouble(value);
        }
    }
}
