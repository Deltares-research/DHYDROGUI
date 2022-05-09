using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// Time  rule does not use IEventedList<Input> Inputs { get; set; }
    /// </summary>
    [Entity]
    public class TimeRule : RuleBase, ITimeDependentRtcObject
    {
        private TimeSeries timeSeries;

        public TimeRule() : this(null) {}

        public TimeRule(string name)
        {
            if (name != null)
            {
                Name = name;
            }

            Reference = string.Empty; // = default EXPLICIT
        }

        /// <summary>
        /// valid values are "EXPLICIT" "IMPLICIT"; default is EXPLICIT
        /// </summary>
        public string Reference { get; set; }

        [NoNotifyPropertyChange]
        public InterpolationType InterpolationOptionsTime
        {
            get
            {
                return TimeSeries.Time.InterpolationType;
            }
            set
            {
                TimeSeries.Time.InterpolationType = value;
            }
        }

        [NoNotifyPropertyChange]
        public ExtrapolationType Periodicity
        {
            get
            {
                return TimeSeries.Time.ExtrapolationType;
            }
            set
            {
                if (!Enum.IsDefined(typeof(ExtrapolationTimeSeriesType), (ExtrapolationTimeSeriesType) value))
                {
                    throw new ArgumentException(string.Format("Extrapolation for time rule does not support {0}", value));
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
                    timeSeries = new TimeSeries();
                    timeSeries.Components.Add(new Variable<double>
                    {
                        Name = "Value",
                        NoDataValue = -999.0
                    });
                    timeSeries.Components[0].Attributes[FunctionAttributes.StandardName] =
                        FunctionAttributes.StandardNames.RtcTimeRule;
                    timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
                    timeSeries.Name = "Time series";
                }

                return timeSeries;
            }

            set
            {
                timeSeries = value;
            }
        }

        [ValidationMethod]
        public static void Validate(TimeRule timeRule)
        {
            var exceptions = new List<ValidationException>();

            if (timeRule.Inputs.Count > 0)
            {
                exceptions.Add(new ValidationException(string.Format("Time rule '{0}' does not support input items.", timeRule.Name)));
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }

        public override object Clone()
        {
            var timeRule = new TimeRule();
            timeRule.CopyFrom(this);
            return timeRule;
        }

        public override void CopyFrom(object source)
        {
            var timeRule = source as TimeRule;
            if (timeRule != null)
            {
                base.CopyFrom(source);
                InterpolationOptionsTime = timeRule.InterpolationOptionsTime;
                Periodicity = timeRule.Periodicity;
                TimeSeries = (TimeSeries) timeRule.TimeSeries.Clone();
                Reference = timeRule.Reference;
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