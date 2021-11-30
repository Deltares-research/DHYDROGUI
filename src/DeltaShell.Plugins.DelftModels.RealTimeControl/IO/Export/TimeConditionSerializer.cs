using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for a <see cref="TimeCondition"/>.
    /// </summary>
    /// <seealso cref="StandardConditionSerializer"/>
    public class TimeConditionSerializer : StandardConditionSerializer
    {
        private const string quantityId = "TimeSeries";
        private readonly TimeCondition timeCondition;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeConditionSerializer"/> class.
        /// </summary>
        /// <param name="timeCondition"> The time condition to serialize. </param>
        public TimeConditionSerializer(TimeCondition timeCondition) : base(timeCondition)
        {
            this.timeCondition = timeCondition;
            XmlTag = RtcXmlTag.TimeCondition;
        }

        // Example of ToXmlInputReference:
        //       <standard id="[TimeCondition]control_group_1/time_condition">
        //         <condition>
        //             <x1Series ref="IMPLICIT">control_group_1/time_condition</x1Series>
        //             <relationalOperator>Equal</relationalOperator>
        //             <x2Value>0</x2Value>
        //         </condition>
        //         <true>
        //             <trigger>
        //                 <ruleReference>[HydraulicRule]control_group_1/lookup_table_rule</ruleReference>
        //             </trigger>
        //         </true>
        //         <output>
        //             <status>[Status]control_group_1/time_condition</status>
        //         </output>
        //     </standard>

        /// <summary>
        /// Converts the time condition to a collection of <see cref="XElement"/>
        /// to be written to the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            return ToXml(xNamespace, prefix, GetXmlNameWithoutTag(prefix));
        }

        /// <summary>
        /// Converts the condition to a collection of <see cref="XElement"/>
        /// to be written to the import series in the data config xml file.
        /// </summary>
        /// <param name="prefix"> The prefix. </param>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToDataConfigImportSeries(string prefix, XNamespace xNamespace)
        {
            foreach (XElement export in base.ToDataConfigImportSeries(prefix, xNamespace))
            {
                yield return export;
            }

            yield return new XElement(xNamespace + "timeSeries", new XAttribute("id", GetXmlNameWithoutTag(prefix)),
                                      new XElement(xNamespace + "PITimeSeries",
                                                   new XElement(xNamespace + "locationId", GetXmlNameWithTag(prefix)),
                                                   new XElement(xNamespace + "parameterId", quantityId),
                                                   new XElement(xNamespace + "interpolationOption",
                                                                timeCondition.InterpolationOptionsTime ==
                                                                InterpolationType.Constant
                                                                    ? "BLOCK"
                                                                    : "LINEAR"),
                                                   new XElement(xNamespace + "extrapolationOption",
                                                                timeCondition.TimeSeries.Time.ExtrapolationType ==
                                                                ExtrapolationType.Periodic
                                                                    ? "PERIODIC"
                                                                    : "BLOCK")
                                      ));
        }

        /// <summary>
        /// Converts the time condition to a collection of <see cref="XElement"/>
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

        private IXmlTimeSeries GetTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            DateTime startTime = start;
            DateTime endTime = stop;
            TimeSpan timeStep = step;

            if (timeCondition.TimeSeries.Time.Values.Count > 0)
            {
                startTime = timeCondition.TimeSeries.Time.Values.First();
                endTime = timeCondition.TimeSeries.Time.Values.Last();
                timeStep = endTime - startTime;
            }

            TimeSpan periodSpan = timeCondition.TimeSeries.Time.Attributes.ContainsKey("PeriodSpan")
                                      ? TimeSpan.ParseExact(timeCondition.TimeSeries.Time.Attributes["PeriodSpan"], "c",
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
                TimeSeries = (TimeSeries) timeCondition.TimeSeries.Clone(),
                InterpolationType = timeCondition.TimeSeries.Time.InterpolationType,
                ExtrapolationType = (ExtrapolationTimeSeriesType) timeCondition.TimeSeries.Time.ExtrapolationType,
                PeriodSpan = periodSpan
            };

            return xmlTimeSeries;
        }
    }
}