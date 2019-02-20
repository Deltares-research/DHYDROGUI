using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Converters;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;
using log4net;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// Time  rule does not use IEventedList<Input> Inputs { get; set; }
    /// </summary>
    [Entity]
    public class TimeRule : RuleBase, IItemContainer, ITimeDependentRtcObject
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TimeRule));

        private string QuantityId = "TimeSeries";

        /// <summary>
        /// valid values are "EXPLICIT" "IMPLICIT"; default is EXPLICIT
        /// </summary>
        public string Reference { get; set; }

        private TimeSeries timeSeries;
        public TimeSeries TimeSeries
        {
            get
            {
                // create default time series only when it is really necessary (speeds-up app initialization)
                if (timeSeries == null)
                {
                    timeSeries = new TimeSeries();
                    timeSeries.Components.Add(new Variable<double> { Name = "Value", NoDataValue = -999.0 });
                    timeSeries.Components[0].Attributes[FunctionAttributes.StandardName] =
                        FunctionAttributes.StandardNames.RtcTimeRule;
                    timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
                    timeSeries.Name = "Time series";
                }

                return timeSeries;
            }

            set { timeSeries = value; }
        }

        public TimeRule(): this(null){}

        public TimeRule(string name)
        {
            if (name != null) Name = name;
            Reference = string.Empty; // = default EXPLICIT
            XmlTag = RtcXmlTag.TimeRule;
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
        public ExtrapolationType Periodicity
        {
            get { return TimeSeries.Time.ExtrapolationType; }
            set
            {
                if (!Enum.IsDefined(typeof(ExtrapolationTimeSeriesType), (ExtrapolationTimeSeriesType)value))
                {
                    throw new ArgumentException(string.Format("Extrapolation for time rule does not support {0}", value));
                }
                TimeSeries.Time.ExtrapolationType = value;
            }
        }

        private IXmlTimeSeries GetTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            var startTime = start;
            var endTime = stop;
            var timeStep = step;

            var periodSpan = TimeSeries.Time.Attributes.ContainsKey("PeriodSpan")
                ? TimeSpan.ParseExact(TimeSeries.Time.Attributes["PeriodSpan"], "c", null)
                : new TimeSpan(0, 0, 0);

            var xmlTimeSeries = new XmlTimeSeries
            {
                StartTime = startTime,
                EndTime = endTime,
                Name = GetXmlNameWithoutTag(prefix),
                LocationId = GetXmlNameWithTag(prefix),
                ParameterId = QuantityId,
                TimeStep = timeStep,
                TimeSeries = (TimeSeries) TimeSeries.Clone(),
                InterpolationType = TimeSeries.Time.InterpolationType,
                ExtrapolationType = (ExtrapolationTimeSeriesType) TimeSeries.Time.ExtrapolationType,
                PeriodSpan = periodSpan
            };

            if (TimeSeries.Time.Values.Count > 0)
            {
                XmlTimeSeriesTruncater.Truncate(xmlTimeSeries, startTime, endTime);
            }

            return xmlTimeSeries;
        }

        // Example of ToXml:
        //  <timeAbsolute id = "[TimeRule]control_group_1/time_rule">
        //      <input>
        //          <x> control_group_1/time_rule </x>
        //      </input>
        //      <output>
        //          <y>[Output]Weir1/Crest level(s)</y>
        //      </output>
        //  </timeAbsolute>

        /// <summary>
        /// Converts the information of the time rule needed for writing the tools config file to an xml element.
        /// </summary>
        /// <param name="xNamespace">The x namespace.</param>
        /// <param name="prefix">The control group name.</param>
        /// <returns>The Xml Element.</returns>
        public override XElement ToXml(XNamespace xNamespace, string prefix)
        {
            var result = base.ToXml(xNamespace, prefix);
            result.Add(new XElement(xNamespace + "timeAbsolute",
                                    new XAttribute("id", GetXmlNameWithTag(prefix)),
                                    new XElement(xNamespace + "input",
                                                 new XElement(xNamespace + "x",
                                                              Reference == string.Empty ? null : new XAttribute("ref", Reference),
                                                              GetXmlNameWithoutTag(prefix))),
                                    Outputs.Select(output => output.ToXml(xNamespace, "y", null))));
            return result;
        }

        [ValidationMethod]
        public static void Validate(TimeRule timeRule)
        {
            var exceptions = new List<ValidationException>();

            if (timeRule.Inputs.Count > 0)
            {
                exceptions.Add(new ValidationException(string.Format("Time rule '{0}' does not support input items.", timeRule.Name)));
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }

        public override object Clone()
        {
            var timeRule = (TimeRule)Activator.CreateInstance(GetType());
            timeRule.CopyFrom(this);
            return timeRule;
        }

        public override void CopyFrom(object source)
        {
            var timeRule = source as TimeRule;
            if (timeRule != null)
            {
                base.CopyFrom(source);
                InterpolationOptionsTime = timeRule.InterpolationOptionsTime;
                Periodicity = timeRule.Periodicity;
                TimeSeries = (TimeSeries) timeRule.TimeSeries.Clone();
                Reference = timeRule.Reference;
            }
        }

        public IEnumerable<object> GetDirectChildren()
        {
            if (timeSeries != null)
                yield return timeSeries;
        }
    }
}
