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
    /// Serializer for a <see cref="PidRule"/>.
    /// </summary>
    /// <seealso cref="RuleSerializerBase"/>
    public class PidRuleSerializer : RuleSerializerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PidRuleSerializer"/> class.
        /// </summary>
        /// <param name="pidRule"> The pid rule to serialize. </param>
        public PidRuleSerializer(PIDRule pidRule) : base(pidRule)
        {
            PidRule = pidRule;
        }

        /// <summary>
        /// Converts the pid rule to a collection of <see cref="XElement"/>
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
            if (PidRule.PidRuleSetpointType == PIDRule.PIDRuleSetpointType.TimeSeries)
            {
                yield return GetImportTimeSeries(prefix, start, stop, step);
            }
        }

        /// <summary>
        /// Converts the pid rule to a collection of <see cref="IXmlTimeSeries"/>
        /// to be written to the export time series in the data config xml file.
        /// </summary>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<IXmlTimeSeries> XmlExportTimeSeries(string prefix)
        {
            yield return GetExportTimeSeries(GetIntegralPartId(prefix));
            yield return GetExportTimeSeries(GetDifferentialPartId(prefix));
        }

        // Example of ToXmlInputReference:
        //  <pid id ="[PID]control_group_1/pid_rule">
        //      <mode>PIDVEL</mode>
        //      <settingMin>4</settingMin>
        //      <settingMax>5</settingMax>
        //      <settingMaxSpeed>6</settingMaxSpeed>
        //      <kp>1</kp>
        //      <ki>2</ki>
        //      <kd>3</kd>
        //      <input>
        //          <x>[Input]ObservationPoint1/Water level(op)</x>
        //          <setpointSeries>[SP]control_group_1/pid_rule</setpointSeries>
        //      </input>
        //      <output>
        //          <y>[Output]Weir1/Crest level(s)</y>
        //          <integralPart>[IP]control_group_1/pid_rule</integralPart>
        //          <differentialPart>[DP]control_group_1/pid_rule</differentialPart>
        //      </output>
        //  </pid>

        /// <summary>
        /// Converts the pid rule to a collection of <see cref="XElement"/>
        /// to be written to the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            XElement result = base.ToXml(xNamespace, prefix).First();

            foreach (Output output in PidRule.Outputs)
            {
                output.IntegralPart = GetIntegralPartId(prefix);
                output.DifferentialPart = GetDifferentialPartId(prefix);
            }

            result.Add(new XElement(xNamespace + "pid",
                                    new XAttribute("id", GetXmlNameWithTag(prefix)),
                                    new XElement(xNamespace + "mode", "PIDVEL"),
                                    new XElement(xNamespace + "settingMin", PidRule.Setting.Min),
                                    new XElement(xNamespace + "settingMax", PidRule.Setting.Max),
                                    new XElement(xNamespace + "settingMaxSpeed", Math.Abs(PidRule.Setting.MaxSpeed)),
                                    new XElement(xNamespace + "kp", PidRule.Kp),
                                    new XElement(xNamespace + "ki", PidRule.Ki),
                                    new XElement(xNamespace + "kd", PidRule.Kd),
                                    PidRule.Inputs.Select(
                                        input =>
                                        {
                                            var serializer = SerializerCreator.CreateSerializerType<InputSerializerBase>(input);
                                            return PidRule.PidRuleSetpointType == PIDRule.PIDRuleSetpointType.Constant
                                                       ? GenerateConstantValueSetPointXml(xNamespace, serializer.GetXmlName())
                                                       : serializer.ToXmlInputReference(xNamespace, "x", "setpointSeries");
                                        }),
                                    PidRule.Outputs.Select(
                                        output =>
                                        {
                                            var serializer = new OutputSerializer(output);
                                            return serializer.ToXmlOutputReference(xNamespace, "y", "integralPart", "differentialPart");
                                        })));
            yield return result;
        }

        /// <summary>
        /// The PID rule requires the input as parameter to calculate the output value. The
        /// output should be set as input exchange item
        /// </summary>
        /// <param name="xNamespace"> </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> OutputAsInputToDataConfigXml(XNamespace xNamespace)
        {
            yield break;
        }

        protected override string XmlTag { get; } = RtcXmlTag.PIDRule;
        private PIDRule PidRule { get; }

        private string GetIntegralPartId(string prefix)
        {
            return RtcXmlTag.IP + GetXmlNameWithoutTag(prefix);
        }

        private string GetDifferentialPartId(string prefix)
        {
            return RtcXmlTag.DP + GetXmlNameWithoutTag(prefix);
        }

        private XElement GenerateConstantValueSetPointXml(XNamespace xNamespace, string name)
        {
            var result = new XElement(xNamespace + "input");
            result.Add(new XElement(xNamespace + "x", name));
            result.Add(new XElement(xNamespace + "setpointValue", PidRule.ConstantValue));
            return result;
        }

        private IXmlTimeSeries GetImportTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            DateTime startTime = start;
            DateTime endTime = stop;
            TimeSpan timeStep = step;
            TimeSpan periodSpan = PidRule.TimeSeries.Time.Attributes.ContainsKey("PeriodSpan")
                                      ? TimeSpan.ParseExact(PidRule.TimeSeries.Time.Attributes["PeriodSpan"], "c", null)
                                      : new TimeSpan(0, 0, 0);

            var xmlTimeSeries = new XmlTimeSeries
            {
                StartTime = startTime,
                EndTime = endTime,
                Name = RtcXmlTag.SP + GetXmlNameWithoutTag(prefix),
                LocationId = GetXmlNameWithTag(prefix),
                ParameterId = "SP",
                TimeStep = timeStep,
                TimeSeries = (TimeSeries) PidRule.TimeSeries.Clone(),
                InterpolationType = PidRule.TimeSeries.Time.InterpolationType,
                ExtrapolationType = (ExtrapolationTimeSeriesType) PidRule.TimeSeries.Time.ExtrapolationType,
                PeriodSpan = periodSpan
            };

            if (PidRule.TimeSeries.Time.Values.Count > 0)
            {
                XmlTimeSeriesTruncater.Truncate(xmlTimeSeries, startTime, endTime);
            }
            else
            {
                xmlTimeSeries.StartTime = startTime;
                xmlTimeSeries.EndTime = endTime;
                xmlTimeSeries.TimeStep = timeStep;
                xmlTimeSeries.TimeSeries.Time.AddValues(new[]
                {
                    start,
                    stop
                });
            }

            return xmlTimeSeries;
        }

        /// <summary>
        /// Returns a IXmlTimeSeries that is written to rtcDataConfig.xml and only used internally by RTCTools.
        /// Only Name is required for this series.
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> The xml time series element. </returns>
        private static IXmlTimeSeries GetExportTimeSeries(string name)
        {
            return new XmlTimeSeries {Name = name};
        }
    }
}