using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop;
using log4net;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public class PIDRule : RuleBase, ITimeDependentRtcObject
    {
        public enum PIDRuleSetpointTypes
        {
            Constant = 0,
            Signal = 1,
            TimeSeries = 2
        }

        private TimeSeries timeSeries;

        public PIDRule() : this(null) {}

        public PIDRule(string name)
        {
            if (name != null)
            {
                Name = name;
            }

            Setting = new Setting {MaxSpeed = 0};
        }

        public PIDRuleSetpointTypes PidRuleSetpointType { get; set; }

        public Setting Setting { get; set; }
        public double Kp { get; set; }
        public double Ki { get; set; }
        public double Kd { get; set; }

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

        [NoNotifyPropertyChange]
        public InterpolationType InterpolationOptionsTime
        {
            get
            {
                return TimeSeries.Time.InterpolationType;
            }
            set
            {
                if (!Enum.IsDefined(typeof(InterpolationHydraulicType), (InterpolationHydraulicType) value))
                {
                    throw new ArgumentException(string.Format("Interpolation for PID rule does not support {0}", value));
                }

                TimeSeries.Time.InterpolationType = value;
            }
        }

        [NoNotifyPropertyChange]
        public ExtrapolationType ExtrapolationOptionsTime
        {
            get
            {
                return TimeSeries.Time.ExtrapolationType;
            }
            set
            {
                if (!Enum.IsDefined(typeof(ExtrapolationTimeSeriesType), (ExtrapolationTimeSeriesType) value))
                {
                    throw new ArgumentException(string.Format("Extrapolation for PID rule does not support {0}", value));
                }

                TimeSeries.Time.ExtrapolationType = value;
            }
        }

        public TimeSeries TimeSeries
        {
            get
            {
                // create default time series only when it is really necessary (speeds-up app initialization)
                if (timeSeries == null)
                {
                    timeSeries = new TimeSeries {Name = "Set points"};
                    timeSeries.Components.Add(new Variable<double>
                    {
                        Name = "Value",
                        NoDataValue = -999.0
                    });
                    timeSeries.Components[0].Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.RtcPidRule;
                    ExtrapolationOptionsTime = ExtrapolationType.Constant;
                }

                return timeSeries;
            }

            set
            {
                timeSeries = value;
            }
        }

        [ValidationMethod]
        public static void Validate(PIDRule pidRule)
        {
            var exceptions = new List<ValidationException>();

            if (pidRule.PidRuleSetpointType == PIDRuleSetpointTypes.TimeSeries && pidRule.TimeSeries.Arguments[0].Values.Count == 0)
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

        public override bool CanBeLinkedFromSignal()
        {
            return true;
        }

        public override bool IsLinkedFromSignal()
        {
            return PidRuleSetpointType == PIDRuleSetpointTypes.Signal;
        }

        public override object Clone()
        {
            var pidRule = new PIDRule();
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

        public override IEnumerable<object> GetDirectChildren()
        {
            if (timeSeries != null)
            {
                yield return timeSeries;
            }
        }
    }
}