using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;
using log4net;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// TimeCondition gives true or false based on a time
    /// The TimeCondition has effect on 3 generated xmls
    ///  - rtcToolsConfig.xml : same as standardcondition but input is times series found in
    ///  - rtcDataConfig.xml : time series definition
    ///  - timeseries_import.xml : the time series contents
    /// </summary>
    [Entity]
    public class TimeCondition : StandardCondition, IItemContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StandardCondition));

        public TimeCondition() : base(false)
        {
            Reference = ReferenceType.Implicit; // = IMPLICIT -> timeseries
            XmlTag = RtcXmlTag.TimeCondition;
        }

        private TimeSeries timeSeries;
        public TimeSeries TimeSeries
        {
            get
            {
                if (timeSeries == null)
                {
                    timeSeries = new TimeSeries { Name = "Time Series" };
                    timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
                    timeSeries.Time.InterpolationType = InterpolationType.Constant;
                    timeSeries.Components.Add(new Variable<bool>
                    {
                        Name = "On",
                        DefaultValue = false,
                        NoDataValue = false,
                        IsAutoSorted = false,
                        Unit = new Unit("true/false", "true/false")
                    });
                    timeSeries.Components[0].Attributes[FunctionAttributes.StandardName] = 
                        FunctionAttributes.StandardNames.RtcTimeCondition;
                    TimeSeries = timeSeries;
                }

                return timeSeries;
            }
            set { timeSeries = value; }
        }

        public override string GetDescription()
        {
            return ""; //todo: show compact notion of time range?
        }

        public override IEnumerable<IXmlTimeSeries> XmlImportTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            yield return GetTimeSeries(prefix, start, stop, step);
        }

        [NoNotifyPropertyChange]
        public InterpolationType InterpolationOptionsTime
        {
            get { return TimeSeries.Time.InterpolationType; }
            set { TimeSeries.Time.InterpolationType = value; }
        }

        [NoNotifyPropertyChange]
        public ExtrapolationType Extrapolation
        {
            get { return TimeSeries.Time.ExtrapolationType; }
            set { TimeSeries.Time.ExtrapolationType = value; }
        }

        private IXmlTimeSeries GetTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            var startTime = start;
            var endTime = stop;
            var timeStep = step;

            if (TimeSeries.Time.Values.Count > 0)
            {
                startTime = TimeSeries.Time.Values.First();
                endTime = TimeSeries.Time.Values.Last();
                timeStep = endTime - startTime;
            }

            var periodSpan = TimeSeries.Time.Attributes.ContainsKey("PeriodSpan")
                    ? TimeSpan.ParseExact(TimeSeries.Time.Attributes["PeriodSpan"], "c", null)
                    : new TimeSpan(0, 0, 0);

            var xmlTimeSeries = new XmlTimeSeries
            {
                StartTime = startTime,
                EndTime = endTime,
                Name = XmlTag + prefix +"/" + LocationId,
                LocationId = prefix + "/" + LocationId,
                ParameterId = QuantityId,
                TimeStep = timeStep,
                TimeSeries = (TimeSeries) TimeSeries.Clone(),
                InterpolationType = TimeSeries.Time.InterpolationType,
                ExtrapolationType = (ExtrapolationTimeSeriesType) TimeSeries.Time.ExtrapolationType,
                PeriodSpan = periodSpan
            };

            return xmlTimeSeries;
        }

        private string LocationId
        {
            get
            {
                return Name;
            }
        }
        private string QuantityId = "TimeSeries";

        public override XElement ToXml(XNamespace xNamespace, string prefix)
        {
            return ToXml(xNamespace, prefix, XmlTag + prefix + "/" + Name);
        }

        public override IEnumerable<XElement> ToDataConfigImportSeries(string prefix, XNamespace xNamespace)
        {
            foreach (var export in base.ToDataConfigImportSeries(prefix, xNamespace))
            {
                yield return export;
            }
            yield return new XElement(xNamespace + "timeSeries", new XAttribute("id", XmlTag + prefix + "/" + Name),
                new XElement(xNamespace + "PITimeSeries",
                    new XElement(xNamespace + "locationId", prefix + "/" + LocationId),
                    new XElement(xNamespace + "parameterId",QuantityId),
                    new XElement(xNamespace + "interpolationOption",InterpolationOptionsTime == InterpolationType.Constant ? "BLOCK" : "LINEAR"),
                    new XElement(xNamespace + "extrapolationOption", TimeSeries.Time.ExtrapolationType == ExtrapolationType.Periodic ? "PERIODIC" : "BLOCK")
                ));

        }

        [ValidationMethod]
        public static void Validate(TimeCondition timeCondition)
        {
            var exceptions = new List<ValidationException>();

            if (timeCondition.Input != null)
            {
                exceptions.Add(new ValidationException(
                        string.Format("Condition '{0}' has input; this is not supported for time conditions.", timeCondition.Name)));
            }
            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }
    
        public override object Clone()
        {
            var timeCondition = (TimeCondition)Activator.CreateInstance(GetType());
            timeCondition.CopyFrom(this);
            return timeCondition;
        }

        public override void CopyFrom(object source)
        {
            var timeCondition = source as TimeCondition;
            if (timeCondition != null)
            {
                base.CopyFrom(source);
                Reference = timeCondition.Reference;
                TimeSeries = (TimeSeries)timeCondition.TimeSeries.Clone();
                InterpolationOptionsTime = timeCondition.InterpolationOptionsTime;
                Extrapolation = timeCondition.Extrapolation;
            }

        }

        public IEnumerable<object> GetDirectChildren()
        {
            if (timeSeries != null)
                yield return timeSeries;
        }
    }
}
