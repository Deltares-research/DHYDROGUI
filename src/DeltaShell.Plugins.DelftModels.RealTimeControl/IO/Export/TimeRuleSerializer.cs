using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Converters;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for a <see cref="TimeRule"/>.
    /// </summary>
    /// <seealso cref="RuleSerializerBase"/>
    public class TimeRuleSerializer : RuleSerializerBase
    {
        private const string quantityId = "TimeSeries";
        private readonly TimeRule timeRule;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeRuleSerializer"/> class.
        /// </summary>
        /// <param name="timeRule">The time rule to serialize.</param>
        public TimeRuleSerializer(TimeRule timeRule) : base(timeRule)
        {
            this.timeRule = timeRule;
        }

        // Example of ToXmlInputReference:
        //  <timeAbsolute id = "[TimeRule]control_group_1/time_rule">
        //      <input>
        //          <x> control_group_1/time_rule </x>
        //      </input>
        //      <output>
        //          <y>[Output]Weir1/Crest level(s)</y>
        //      </output>
        //  </timeAbsolute>

        /// <summary>
        /// Converts the time rule to a collection of <see cref="XElement"/>
        /// to be written to the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            XElement result = base.ToXml(xNamespace, prefix).First();
            result.Add(new XElement(xNamespace + "timeAbsolute",
                                    new XAttribute("id", GetXmlNameWithTag(prefix)),
                                    new XElement(xNamespace + "input",
                                                 new XElement(xNamespace + "x",
                                                              timeRule.Reference == string.Empty
                                                                  ? null
                                                                  : new XAttribute("ref", timeRule.Reference),
                                                              GetXmlNameWithoutTag(prefix))),
                                    timeRule.Outputs.Select(
                                        output =>
                                        {
                                            var serializer = new OutputSerializer(output);
                                            return serializer.ToXmlOutputReference(xNamespace, "y", null);
                                        })));

            yield return result;
        }

        /// <summary>
        /// Converts the time rule to a collection of <see cref="XElement"/>
        /// to be written to the import series in the data config xml file
        /// and the time series import xml file.
        /// </summary>
        /// <param name="prefix"> The prefix. </param>
        /// <param name="start"> The start time of the model. </param>
        /// <param name="stop"> The stop time of the model. </param>
        /// <param name="step"> The time step of the model. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<IXmlTimeSeries> XmlImportTimeSeries(string prefix, DateTime start, DateTime stop,
                                                                        TimeSpan step)
        {
            yield return GetTimeSeries(prefix, start, stop, step);
        }

        protected override string XmlTag { get; } = RtcXmlTag.TimeRule;

        private IXmlTimeSeries GetTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            DateTime startTime = start;
            DateTime endTime = stop;
            TimeSpan timeStep = step;

            TimeSpan periodSpan = timeRule.TimeSeries.Time.Attributes.ContainsKey("PeriodSpan")
                                      ? TimeSpan.ParseExact(timeRule.TimeSeries.Time.Attributes["PeriodSpan"], "c",
                                                            null)
                                      : new TimeSpan(0, 0, 0);

            var xmlTimeSeries = new XmlTimeSeries
            {
                StartTime = startTime,
                EndTime = endTime,
                Name = GetXmlNameWithoutTag(prefix),
                LocationId = GetXmlNameWithTag(prefix),
                ParameterId = quantityId,
                TimeStep = timeStep,
                TimeSeries = (TimeSeries) timeRule.TimeSeries.Clone(),
                InterpolationType = timeRule.TimeSeries.Time.InterpolationType,
                ExtrapolationType = (ExtrapolationTimeSeriesType) timeRule.TimeSeries.Time.ExtrapolationType,
                PeriodSpan = periodSpan
            };

            if (timeRule.TimeSeries.Time.Values.Count > 0)
            {
                XmlTimeSeriesTruncater.Truncate(xmlTimeSeries, startTime, endTime);
            }

            return xmlTimeSeries;
        }
    }
}