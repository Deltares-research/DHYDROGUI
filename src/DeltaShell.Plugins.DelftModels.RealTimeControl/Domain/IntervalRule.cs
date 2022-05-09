using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// The dead band rule is a discrete rule for suppressing the output of another rule until the
    /// output’s gradient becomes higher than a certain threshold. It reads
    /// k         k+1    k
    /// - Y     if |Y    - Y | lt treshold
    /// k+1     |
    /// y      =  |
    /// |  k+1
    /// - y       otherwise
    /// It is often applied to limit the number of adjustments to movable elements of hydraulic
    /// structures in order to increase their life time.
    /// </summary>
    [Entity]
    public class IntervalRule : RuleBase, ITimeDependentRtcObject
    {
        public enum IntervalRuleDeadBandType
        {
            Fixed = 0,
            PercentageDischarge = 1
        }

        public enum IntervalRuleIntervalType
        {
            Fixed = 0,
            Variable = 1
        }

        public enum IntervalRuleSetPointType
        {
            Fixed = 0,
            Variable = 1,
            Signal = 2
        }

        /// <summary>
        /// Time series or constant that is used as input for the interval rule. The RTC will try to achieve
        /// that input will have the values set in TimeSeries by controlling the output.
        /// </summary>
        private TimeSeries timeSeries;

        public IntervalRule()
            : this(null) {}

        public IntervalRule(string name)
        {
            if (name != null)
            {
                Name = name;
            }

            Setting = new Setting {MaxSpeed = 0};
        }

        public IntervalRuleIntervalType IntervalType { get; set; }

        public double FixedInterval { get; set; }

        public IntervalRuleDeadBandType DeadBandType { get; set; }

        public Setting Setting { get; set; }

        /// <summary>
        /// A margin around the setpoint (Timeseries) to avoid unnecessary control actions of the output structure (e.g. avoid
        /// continous on/off switching of pump).
        /// </summary>
        public double DeadbandAroundSetpoint { get; set; }

        public IntervalRuleSetPointType SetPointType { get; set; }

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
                    throw new ArgumentException(string.Format("Interpolation for interval rule does not support {0}", value));
                }

                TimeSeries.Time.InterpolationType = value;
            }
        }

        [NoNotifyPropertyChange]
        public ExtrapolationType Extrapolation
        {
            get
            {
                return TimeSeries.Time.ExtrapolationType;
            }
            set
            {
                if (!Enum.IsDefined(typeof(ExtrapolationTimeSeriesType), (ExtrapolationTimeSeriesType) value))
                {
                    throw new ArgumentException(string.Format("Extrapolation for interval rule does not support {0}", value));
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
                    timeSeries = CreateTimeSeries();
                }

                return timeSeries;
            }

            set
            {
                timeSeries = value;
            }
        }

        [ValidationMethod]
        public static void Validate(IntervalRule intervalRule)
        {
            var exceptions = new List<ValidationException>();

            if (intervalRule.SetPointType == IntervalRuleSetPointType.Variable && intervalRule.TimeSeries.Arguments[0].Values.Count == 0)
            {
                exceptions.Add(new ValidationException(string.Format(Resources.RealTimeControlModelIntervalRule_Interval_rule__0__has_empty_time_series, intervalRule.Name)));
            }

            if (intervalRule.Inputs.Count != 1)
            {
                exceptions.Add(new ValidationException(string.Format(Resources.RealTimeControlModelIntervalRule_Interval_rule__0__requires_1_input, intervalRule.Name)));
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
            return SetPointType == IntervalRuleSetPointType.Signal;
        }

        public override object Clone()
        {
            var intervalRule = new IntervalRule();
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
                DeadBandType = intervalRule.DeadBandType;
                ConstantValue = intervalRule.ConstantValue;
                Extrapolation = intervalRule.Extrapolation;
            }
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            if (timeSeries != null)
            {
                yield return timeSeries;
            }
        }

        private TimeSeries CreateTimeSeries()
        {
            var localTimeSeries = new TimeSeries {Name = "Setpoints"};
            localTimeSeries.Time.InterpolationType = InterpolationType.Constant;
            localTimeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
            localTimeSeries.Components.Add(new Variable<double>
            {
                Name = "Value",
                NoDataValue = -999.0
            });
            localTimeSeries.Components[0].Attributes[FunctionAttributes.StandardName] =
                FunctionAttributes.StandardNames.RtcIntervalRule;

            return localTimeSeries;
        }
    }
}