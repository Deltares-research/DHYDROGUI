using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// The dead band rule is a discrete rule for suppressing the output of another rule until the
    /// output’s gradient becomes higher than a certain threshold. It reads
    /// 
    ///              k         k+1    k
    ///           - Y     if |Y    - Y | lt treshold
    ///   k+1     |  
    /// y      =  | 
    ///           |  k+1 
    ///           - y       otherwise 
    /// It is often applied to limit the number of adjustments to movable elements of hydraulic
    /// structures in order to increase their life time.
    /// </summary>
    [Entity]
    public class IntervalRule : RuleBase, IItemContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(IntervalRule));

        public IntervalRuleIntervalType IntervalType { get; set; }

        public double FixedInterval { get; set; }

        public IntervalRuleDeadBandType DeadBandType { get; set; }

        public Setting Setting { get; set; }

        /// <summary>
        /// A margin around the setpoint (Timeseries) to avoid unnecessary control actions of the output structure (e.g. avoid 
        /// continous on/off switching of pump).
        /// </summary>
        public double DeadbandAroundSetpoint { get; set; }

        public override bool CanBeLinkedFromSignal()
        {
            return true;
        }

        public override bool IsLinkedFromSignal()
        {
            return IntervalType == IntervalRuleIntervalType.Signal;
        }

        [NoNotifyPropertyChange]
        public double ConstantValue
        {
            get
            {
                return (double)TimeSeries.Components[0].DefaultValue;
            }
            set
            {
                TimeSeries.Components[0].DefaultValue = value;
            }
        }


        /// <summary>
        /// Time series or constant that is used as input for the interval rule. The RTC will try to achieve
        /// that input will have the values set in TimeSeries by controlling the output.
        /// </summary>
        private TimeSeries timeSeries;
        public TimeSeries TimeSeries
        {
            get
            {
                // create default time series only when it is really necessary (speeds-up app initialization)
                if (timeSeries == null)
                {
                    timeSeries = new TimeSeries { Name = "Setpoints" };
                    timeSeries.Time.InterpolationType = InterpolationType.Constant;
                    timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
                    timeSeries.Components.Add(new Variable<double>
                                                  {
                                                      Name = "Value", 
                                                      NoDataValue = -999.0
                                                  });
                    timeSeries.Components[0].Attributes[FunctionAttributes.StandardName] =
                        FunctionAttributes.StandardNames.RtcIntervalRule;
                }

                return timeSeries;
            }

            set { timeSeries = value; }
        }
        
        public string Status
        {
            get
            {
                return Name + "_status";
            }
        }
        public string SetPoint
        {
            get
            {
                return Name + "_SP";
            }
        }

        public IntervalRule()
            : this(null)
        {
        }

        public IntervalRule(string name)
        {
            if (name != null) Name = name;
            Setting = new Setting { MaxSpeed = 0 };
            XmlTag = RtcXmlTag.IntervalRule;
        }

        public override XElement ToXml(XNamespace xNamespace, string prefix)
        {
            var result = base.ToXml(xNamespace, prefix);
            foreach (var input in Inputs)
            {
                input.SetPoint = prefix + SetPoint;
            }
            foreach (var output in Outputs)
            {
                output.IntegralPart = prefix + Status;  // also in data export and statevector
            }

            var deadBandSetpoint = "deadbandSetpointAbsolute";
            var settingMax = "";
            var settingMaxValue = 0.0;

            if(DeadBandType == IntervalRuleDeadBandType.PercentageDischarge)
            {
                deadBandSetpoint = "deadbandSetpointRelative";
            }

            if(IntervalType == IntervalRuleIntervalType.Fixed)
            {
                settingMax = "settingMaxStep";
                settingMaxValue = Math.Abs(FixedInterval);
            }
            else
            {
                settingMax = "settingMaxSpeed";
                settingMaxValue = Math.Abs(Setting.MaxSpeed);
            }

            result.Add(new XElement(xNamespace + "interval",
                    new XAttribute("id", prefix + "/" + Name),
                    new XElement(xNamespace + "settingBelow", Setting.Below.ToString(CultureInfo.InvariantCulture)),
                    new XElement(xNamespace + "settingAbove", Setting.Above.ToString(CultureInfo.InvariantCulture)),
                    new XElement(xNamespace + settingMax, settingMaxValue.ToString(CultureInfo.InvariantCulture)),
                    new XElement(xNamespace + deadBandSetpoint, DeadbandAroundSetpoint.ToString(CultureInfo.InvariantCulture)),
                    Inputs.Select(input => input.ToXml(xNamespace, "x", "setpoint")),
                    Outputs.Select(output => output.ToXml(xNamespace, "y", "status"))));
            return result;
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

            var periodSpan = TimeSeries.Time.Attributes.ContainsKey("PeriodSpan")
                ? TimeSpan.ParseExact(TimeSeries.Time.Attributes["PeriodSpan"], "c", null) 
                : new TimeSpan(0, 0, 0);

            var xmlTimeSeries = new XmlTimeSeries
            {
                StartTime = startTime,
                EndTime = endTime,
                Name = prefix + SetPoint,
                LocationId = prefix+Name,
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

                xmlTimeSeries.TimeSeries.Time.AddValues(new [] { start, stop });
            }


            return xmlTimeSeries;
        }

        private IXmlTimeSeries GetExportTimeSeries(string prefix)
        {
            var xmlTimeSeries = new XmlTimeSeries
            {
                Name = prefix + Status,
                LocationId = prefix+Name,
                ParameterId = "Status",
            };

            return xmlTimeSeries;
        }

        [NoNotifyPropertyChange]
        public InterpolationType InterpolationOptionsTime
        {
            get { return TimeSeries.Time.InterpolationType; }
            set
            {
                if (!Enum.IsDefined(typeof(InterpolationHydraulicType), (InterpolationHydraulicType)value))
                {
                    throw new ArgumentException(string.Format("Interpolation for interval rule does not support {0}", value));
                }
                TimeSeries.Time.InterpolationType = value;
            }
        }

        [NoNotifyPropertyChange]
        public ExtrapolationType Extrapolation
        {
            get { return TimeSeries.Time.ExtrapolationType; }
            set
            {
                if (!Enum.IsDefined(typeof(ExtrapolationTimeSeriesType), (ExtrapolationTimeSeriesType)value))
                {
                    throw new ArgumentException(string.Format("Extrapolation for interval rule does not support {0}", value));
                }
                TimeSeries.Time.ExtrapolationType = value;
            }
        }

        [ValidationMethod]
        public static void Validate(IntervalRule intervalRule)
        {

            //TimeSeries can be empty. The Default value of the componentt can be used as a constant.
            //So... no validation at the moment
            return;
        }

        public override object Clone()
        {
            var intervalRule = (IntervalRule)Activator.CreateInstance(GetType());
            intervalRule.CopyFrom(this);
            return intervalRule;
        }

        public override void CopyFrom(object source)
        {
            var intervalRule = source as IntervalRule;
            if (intervalRule != null)
            {
                base.CopyFrom(source);
                InterpolationOptionsTime = intervalRule.InterpolationOptionsTime;
                DeadbandAroundSetpoint = intervalRule.DeadbandAroundSetpoint;
                TimeSeries = (TimeSeries) intervalRule.TimeSeries.Clone();
                Setting.CopyFrom(intervalRule.Setting);
                IntervalType = intervalRule.IntervalType;
                FixedInterval = intervalRule.FixedInterval;
                DeadBandType= intervalRule.DeadBandType;
                ConstantValue = intervalRule.ConstantValue;
                Extrapolation = intervalRule.Extrapolation;
            }
        }

        public enum IntervalRuleDeadBandType
        {
            Fixed = 0,
            PercentageDischarge = 1
        }

        public enum IntervalRuleIntervalType
        {
            Fixed = 0,
            Variable = 1,
            Signal = 2
        }

        public IEnumerable<object> GetDirectChildren()
        {
            if (timeSeries != null)
                yield return timeSeries;
        }
    }
}
