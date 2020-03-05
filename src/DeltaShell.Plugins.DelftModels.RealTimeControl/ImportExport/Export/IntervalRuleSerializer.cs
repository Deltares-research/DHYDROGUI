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

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public class IntervalRuleSerializer : RuleSerializerBase
    {
        private IntervalRule IntervalRule { get; }

        public IntervalRuleSerializer(IntervalRule intervalRule) : base(intervalRule)
        {
            IntervalRule = intervalRule;
        }

        protected override string XmlTag { get; } = RtcXmlTag.IntervalRule;

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
        /// Converts the information of the interval rule needed for writing the tools config file to an xml element.
        /// </summary>
        /// <param name="xNamespace">The x namespace.</param>
        /// <param name="prefix">The control group name.</param>
        /// <returns>The Xml Element.</returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            var result = base.ToXml(xNamespace, prefix).First();

            foreach (var output in IntervalRule.Outputs)
            {
                output.IntegralPart = RtcXmlTag.Status + GetXmlNameWithoutTag(prefix);  // also in data export and statevector
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
                    IntervalRule.Inputs.OfType<Input>().Select(input =>
                    {
                        var serializer = new InputSerializer(input);
                        return serializer.ToXmlInputReference(xNamespace, "x", "setpoint");
                    }),
                       IntervalRule.Outputs.Select(output =>
                       {
                           var serializer = new OutputSerializer(output);
                           return serializer.ToXmlOutputReference(xNamespace, "y", "status");
                       })));
            yield return result;
        }

        //To be removed : Disk Schwanenberg: It is probably better to let RTCTools always determine the start values.
        //public override IEnumerable<XElement> ToImportState(XNamespace xNamespace)
        //{
        //    yield return (new XElement(xNamespace + "treeVectorLeaf",
        //                               new XAttribute("id", Status),
        //                               new XElement(xNamespace + "vector", 0.0)));

        //}

        public override IEnumerable<IXmlTimeSeries> XmlImportTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            yield return GetImportTimeSeries(prefix, start, stop, step);
        }

        public override IEnumerable<IXmlTimeSeries> XmlExportTimeSeries(string prefix)
        {
            yield return GetExportTimeSeries(prefix);
        }

        private IXmlTimeSeries GetImportTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            var startTime = start;
            var endTime = stop;
            var timeStep = step;

            var periodSpan = IntervalRule.TimeSeries.Time.Attributes.ContainsKey("PeriodSpan")
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

            if (IntervalRule.IntervalType == IntervalRule.IntervalRuleIntervalType.Fixed)
            {
                xmlTimeSeries.TimeSeries = IntervalRuleTimeSeriesCreator.Create();
                xmlTimeSeries.InterpolationType = xmlTimeSeries.TimeSeries.Time.InterpolationType;
                xmlTimeSeries.ExtrapolationType = (ExtrapolationTimeSeriesType)xmlTimeSeries.TimeSeries.Time.ExtrapolationType;

                xmlTimeSeries.TimeSeries.Components[0].DefaultValue = IntervalRule.TimeSeries.Components[0].DefaultValue;
                xmlTimeSeries.TimeSeries.Time.AddValues(new[] { start, stop });
            }
            else if (IntervalRule.IntervalType == IntervalRule.IntervalRuleIntervalType.Variable)
            {
                xmlTimeSeries.TimeSeries = (TimeSeries)IntervalRule.TimeSeries.Clone();
                xmlTimeSeries.InterpolationType = IntervalRule.TimeSeries.Time.InterpolationType;
                xmlTimeSeries.ExtrapolationType = (ExtrapolationTimeSeriesType)IntervalRule.TimeSeries.Time.ExtrapolationType;

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
                ParameterId = "Status",
            };

            return xmlTimeSeries;
        }
    }
}