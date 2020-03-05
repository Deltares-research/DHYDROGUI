using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Converters;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public class TimeRuleSerializer : RuleSerializerBase
    {
        private const string quantityId = "TimeSeries";
        private readonly TimeRule timeRule;

        public TimeRuleSerializer(TimeRule timeRule) : base(timeRule)
        {
            this.timeRule = timeRule;
        }

        protected override string XmlTag { get; } = RtcXmlTag.TimeRule;

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
        /// Converts the information of the time rule needed for writing the tools config file to an xml element.
        /// </summary>
        /// <param name="xNamespace"> The x namespace. </param>
        /// <param name="prefix"> The control group name. </param>
        /// <returns> The Xml Element. </returns>
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

        public override IEnumerable<IXmlTimeSeries> XmlImportTimeSeries(string prefix, DateTime start, DateTime stop,
                                                                        TimeSpan step)
        {
            yield return GetTimeSeries(prefix, start, stop, step);
        }

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