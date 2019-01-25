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
    [Entity]
    public class PIDRule : RuleBase, IItemContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PIDRule));

        public PIDRule() : this(null)
        {
            XmlTag = RtcXmlTag.PIDRule;
        }

        private TimeSeries timeSeries;

        public PIDRule(string name)
        {
            if (name != null) Name = name;

            Setting = new Setting {MaxSpeed = 0};
        }

        public PIDRuleSetpointType PidRuleSetpointType { get; set; }

        public Setting Setting { get; set; }
        public double Kp { get; set; }
        public double Ki { get; set; }
        public double Kd { get; set; }

        public override bool CanBeLinkedFromSignal()
        {
            return true;
        }

        public override bool IsLinkedFromSignal()
        {
            return PidRuleSetpointType == PIDRuleSetpointType.Signal;
        }

        [NoNotifyPropertyChange]
        public double ConstantValue
        {
            get
            {
                return (double) TimeSeries.Components[0].DefaultValue;
            }
            set
            {
                TimeSeries.Components[0].DefaultValue = value;
            }
        }

        public TimeSeries TimeSeries
        {
            get
            {
                // create default time series only when it is really necessary (speeds-up app initialization)
                if(timeSeries == null)
                {
                    timeSeries = new TimeSeries {Name = "Set points"};
                    timeSeries.Components.Add(new Variable<double> { Name = "Value", NoDataValue = -999.0 });
                    timeSeries.Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.RtcPidRule;
                    ExtrapolationOptionsTime = ExtrapolationType.Constant;
                }

                return timeSeries;
            }

            set { timeSeries = value; }
        }

        public override IEnumerable<IXmlTimeSeries> XmlImportTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
        {
            yield return GetImportTimeSeries(prefix, start, stop, step);
        }

        public override IEnumerable<IXmlTimeSeries> XmlExportTimeSeries(string prefix)
        {
            yield return GetExportTimeSeries(RtcXmlTag.IP + prefix + Name);
            yield return GetExportTimeSeries(RtcXmlTag.DP + prefix + Name);
        }


        public override XElement ToXml(XNamespace xNamespace, string prefix)
        {
            var result = base.ToXml(xNamespace, prefix);

            foreach (var output in Outputs)
            {
                output.IntegralPart = RtcXmlTag.IP + prefix + Name;
                output.DifferentialPart = RtcXmlTag.DP + prefix + Name;
            }
            result.Add(new XElement(xNamespace + "pid",
                                    new XAttribute("id", XmlTag + prefix + Name),
                                    new XElement(xNamespace + "mode", "PIDVEL"),
                                    new XElement(xNamespace + "settingMin", Setting.Min),
                                    new XElement(xNamespace + "settingMax", Setting.Max),
                                    new XElement(xNamespace + "settingMaxSpeed", Math.Abs(Setting.MaxSpeed)),
                                    new XElement(xNamespace + "kp", Kp),
                                    new XElement(xNamespace + "ki", Ki),
                                    new XElement(xNamespace + "kd", Kd),
                                    Inputs.Select(input => PidRuleSetpointType == PIDRuleSetpointType.Constant ?
                                        GenerateConstantValueSetPointXml(xNamespace, input.XmlName) :
                                        input.ToXml(xNamespace, "x", "setpointSeries")),
                                    Outputs.Select(output => output.ToXml(xNamespace, "y", "integralPart", "differentialPart"))));
            return result;
        }

        private XElement GenerateConstantValueSetPointXml(XNamespace xNamespace, string name)
        {
            var result = new XElement(xNamespace + "input");
            result.Add(new XElement(xNamespace + "x", name));
            result.Add(new XElement(xNamespace + "setpointValue", ConstantValue));
            return result;
        }


        /// <summary>
        /// The PID rule requires the input as parameter to calculate the output value. The 
        /// output should be set as input exchange item
        /// </summary>
        /// <param name="xNamespace"></param>
        /// <returns></returns>
        public override IEnumerable<XElement> OutputAsInputToDataConfigXml(XNamespace xNamespace)
        {
            yield break;
        }

        private IXmlTimeSeries GetImportTimeSeries(string prefix, DateTime start, DateTime stop, TimeSpan step)
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
                                        Name = RtcXmlTag.SP + prefix + Name,
                                        LocationId = XmlTag + prefix + Name,
                                        ParameterId = "SP",
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
            else
            {
                xmlTimeSeries.StartTime = startTime;
                xmlTimeSeries.EndTime = endTime;
                xmlTimeSeries.TimeStep = timeStep;
                xmlTimeSeries.TimeSeries.Time.AddValues(new[] { start, stop });
            }

            return xmlTimeSeries;
        }

        /// <summary>
        /// Returns a IXmlTimeSeries that is written to rtcDataConfig.xml and only used internally by RTCTools.
        /// Only Name is required for this series.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static IXmlTimeSeries GetExportTimeSeries(string name)
        {
            return new XmlTimeSeries {Name = name };
        }

        [NoNotifyPropertyChange]
        public InterpolationType InterpolationOptionsTime
        {
            get { return TimeSeries.Time.InterpolationType; }
            set
            {
                if (!Enum.IsDefined(typeof(InterpolationHydraulicType), (InterpolationHydraulicType)value))
                {
                    throw new ArgumentException(string.Format("Interpolation for PID rule does not support {0}", value));
                }
                TimeSeries.Time.InterpolationType = value;
            }
        }

        [NoNotifyPropertyChange]
        public ExtrapolationType ExtrapolationOptionsTime
        {
            get { return TimeSeries.Time.ExtrapolationType; }
            set
            {
                if (!Enum.IsDefined(typeof(ExtrapolationTimeSeriesType), (ExtrapolationTimeSeriesType)value))
                {
                    throw new ArgumentException(string.Format("Extrapolation for PID rule does not support {0}", value));
                }
                TimeSeries.Time.ExtrapolationType = value;
            }
        }


        [ValidationMethod]
        public static void Validate(PIDRule pidRule)
        {
            var exceptions = new List<ValidationException>();

            if ((pidRule.PidRuleSetpointType == PIDRuleSetpointType.TimeSeries) && (pidRule.TimeSeries.Arguments[0].Values.Count == 0))
            {
                exceptions.Add(new ValidationException(string.Format("pid rule '{0}' has empty time series.", pidRule.Name)));
            }

            if (pidRule.Inputs.Count != 1)
            {
                exceptions.Add(new ValidationException(string.Format("pid rule '{0}' requires 1 input", pidRule.Name)));
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }

        public override object Clone()
        {
            var pidRule = (PIDRule)Activator.CreateInstance(GetType());
            pidRule.CopyFrom(this);
            return pidRule;
        }

        public override void CopyFrom(object source)
        {
            var pidRule = source as PIDRule;
            if (pidRule != null)
            {
                base.CopyFrom(source);
                Kp = pidRule.Kp;
                Ki = pidRule.Ki;
                Kd = pidRule.Kd;
                PidRuleSetpointType = pidRule.PidRuleSetpointType;
                Setting.CopyFrom(pidRule.Setting);
                TimeSeries = (TimeSeries) pidRule.TimeSeries.Clone();
            }
        }

        public enum PIDRuleSetpointType
        {
            Constant = 0,
            Signal = 1,
            TimeSeries = 2
        }

        public IEnumerable<object> GetDirectChildren()
        {
            if (timeSeries != null)
                yield return timeSeries;
        }
    }
}
