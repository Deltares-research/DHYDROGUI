using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Converters;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for an <see cref="IntervalRule"/>.
    /// </summary>
    /// <seealso cref="RuleSerializerBase"/>
    public class IntervalRuleSerializer : RuleSerializerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalRuleSerializer"/> class.
        /// </summary>
        /// <param name="intervalRule">The interval rule to serialize.</param>
        public IntervalRuleSerializer(IntervalRule intervalRule) : base(intervalRule)
        {
            IntervalRule = intervalRule;
        }

        // Example of ToXmlInputReference:
        //  <interval id ="[IntervalRule]control_group_1/interval_rule">
        //      <settingBelow>4</settingBelow>
        //      <settingAbove>3</settingAbove>
        //      <settingMaxSpeed>1</settingMaxSpeed >
        //      <deadbandSetpointRelative>5</deadbandSetpointRelative>
        //      <input>
        //          <x ref="EXPLICIT">[Input]ObservationPoint1/Water level(op)</x>
        //          <setpoint>[SP]control_group_1/interval_rule</setpoint>
        //      </input>
        //      <output>
        //          <y>[Output]Weir1/Crest level(s)</y>
        //          <status>[Status]control_group_1/interval_rule</status>
        //      </output>
        //  </interval>

        /// <summary>
        /// Converts the interval rule to a collection of <see cref="XElement"/>
        /// to be written to the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            XElement result = base.ToXml(xNamespace, prefix).First();

            foreach (Output output in IntervalRule.Outputs)
            {
                output.IntegralPart = RtcXmlTag.Status + GetXmlNameWithoutTag(prefix); // also in data export and statevector
            }

            var deadBandSetpoint = "deadbandSetpointAbsolute";
            var settingMax = "";
            var settingMaxValue = 0.0;

            if (IntervalRule.DeadBandType == IntervalRule.IntervalRuleDeadBandType.PercentageDischarge)
            {
                deadBandSetpoint = "deadbandSetpointRelative";
            }

            if (IntervalRule.IntervalType == IntervalRule.IntervalRuleIntervalType.Fixed)
            {
                settingMax = "settingMaxStep";
                settingMaxValue = Math.Abs(IntervalRule.FixedInterval);
            }
            else
            {
                settingMax = "settingMaxSpeed";
                settingMaxValue = Math.Abs(IntervalRule.Setting.MaxSpeed);
            }

            result.Add(new XElement(xNamespace + "interval",
                                    new XAttribute("id", GetXmlNameWithTag(prefix)),
                                    new XElement(xNamespace + "settingBelow", IntervalRule.Setting.Below.ToString(CultureInfo.InvariantCulture)),
                                    new XElement(xNamespace + "settingAbove", IntervalRule.Setting.Above.ToString(CultureInfo.InvariantCulture)),
                                    new XElement(xNamespace + settingMax, settingMaxValue.ToString(CultureInfo.InvariantCulture)),
                                    new XElement(xNamespace + deadBandSetpoint, IntervalRule.DeadbandAroundSetpoint.ToString(CultureInfo.InvariantCulture)),
                                    IntervalRule.Inputs.Select(input =>
                                    {
                                        var serializer = SerializerCreator.CreateSerializerType<InputSerializerBase>(input);
                                        return serializer.ToXmlInputReference(xNamespace, "x", "setpoint");
                                    }),
                                    IntervalRule.Outputs.Select(output =>
                                    {
                                        var serializer = new OutputSerializer(output);
                                        return serializer.ToXmlOutputReference(xNamespace, "y", "status");
                                    })));
            yield return result;
        }

        /// <summary>
        /// Converts the interval rule to a collection of <see cref="XElement"/>
        /// to be written to the import series in the data config xml file
        /// and the time series import xml file.
        /// </summary>
        /// <param name="prefix"> The prefix. </param>
        /// <param name="start"> The start time of the model. </param>
        /// <param name="stop"> The stop time of the model. </param>
        /// <param name="step"> The time step of the model. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<IXmlTimeSeries> XmlImportTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            yield return GetImportTimeSeries(prefix, start, stop, step);
        }

        /// <summary>
        /// Converts the interval rule to a collection of <see cref="XElement"/>
        /// to be written to the export series in the data config xml file.
        /// </summary>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<IXmlTimeSeries> XmlExportTimeSeries(string prefix)
        {
            yield return GetExportTimeSeries(prefix);
        }

        protected override string XmlTag { get; } = RtcXmlTag.IntervalRule;
        private IntervalRule IntervalRule { get; }

        private IXmlTimeSeries GetImportTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            DateTime startTime = start;
            DateTime endTime = stop;
            TimeSpan timeStep = step;

            TimeSpan periodSpan = IntervalRule.TimeSeries.Time.Attributes.ContainsKey("PeriodSpan")
                                      ? TimeSpan.ParseExact(IntervalRule.TimeSeries.Time.Attributes["PeriodSpan"], "c", null)
                                      : new TimeSpan(0, 0, 0);

            var xmlTimeSeries = new XmlTimeSeries
            {
                StartTime = startTime,
                EndTime = endTime,
                Name = RtcXmlTag.SP + GetXmlNameWithoutTag(prefix),
                LocationId = GetXmlNameWithTag(prefix),
                ParameterId = "SP",
                TimeStep = timeStep,
                PeriodSpan = periodSpan
            };

            if (IntervalRule.SetPointType == IntervalRule.IntervalRuleSetPointType.Fixed)
            {
                xmlTimeSeries.TimeSeries = IntervalRuleTimeSeriesCreator.Create();
                xmlTimeSeries.InterpolationType = xmlTimeSeries.TimeSeries.Time.InterpolationType;
                xmlTimeSeries.ExtrapolationType = (ExtrapolationTimeSeriesType) xmlTimeSeries.TimeSeries.Time.ExtrapolationType;

                xmlTimeSeries.TimeSeries.Components[0].DefaultValue = IntervalRule.TimeSeries.Components[0].DefaultValue;
                xmlTimeSeries.TimeSeries.Time.AddValues(new[]
                {
                    start,
                    stop
                });
            }
            else if (IntervalRule.SetPointType == IntervalRule.IntervalRuleSetPointType.Variable)
            {
                xmlTimeSeries.TimeSeries = (TimeSeries) IntervalRule.TimeSeries.Clone();
                xmlTimeSeries.InterpolationType = IntervalRule.TimeSeries.Time.InterpolationType;
                xmlTimeSeries.ExtrapolationType = (ExtrapolationTimeSeriesType) IntervalRule.TimeSeries.Time.ExtrapolationType;

                XmlTimeSeriesTruncater.Truncate(xmlTimeSeries, startTime, endTime);
            }
            else // Should never happen, since for signal setpoints this method should never be called.
            {
                throw new InvalidOperationException(Resources.RealTimeControlModelIntervalRule_Import_time_series_for_signals_are_not_existing_export_failed);
            }

            return xmlTimeSeries;
        }

        private IXmlTimeSeries GetExportTimeSeries(string prefix)
        {
            var xmlTimeSeries = new XmlTimeSeries
            {
                Name = RtcXmlTag.Status + GetXmlNameWithoutTag(prefix),
                LocationId = GetXmlNameWithTag(prefix),
                ParameterId = "Status"
            };

            return xmlTimeSeries;
        }
    }
}