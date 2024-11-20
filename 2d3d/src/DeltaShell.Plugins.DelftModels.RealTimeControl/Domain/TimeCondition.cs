using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using log4net;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    /// <summary>
    /// TimeCondition gives true or false based on a time
    /// The TimeCondition has effect on 3 generated xmls
    /// - rtcToolsConfig.xml : same as standardcondition but input is times series found in
    /// - rtcDataConfig.xml : time series definition
    /// - timeseries_import.xml : the time series contents
    /// </summary>
    [Entity]
    public class TimeCondition : StandardCondition, IItemContainer, ITimeDependentRtcObject
    {
        private TimeSeries timeSeries;

        public TimeCondition() : base(false)
        {
            Reference = ReferenceType.Implicit; // = IMPLICIT -> timeseries
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
                TimeSeries.Time.ExtrapolationType = value;
            }
        }

        public TimeSeries TimeSeries
        {
            get
            {
                if (timeSeries == null)
                {
                    timeSeries = new TimeSeries { Name = "Time Series" };
                    timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
                    timeSeries.Time.InterpolationType = InterpolationType.Constant;
                    timeSeries.Components.Add(new Variable<bool>
                    {
                        Name = "On",
                        DefaultValue = false,
                        NoDataValue = false,
                        IsAutoSorted = false,
                        Unit = new Unit("true/false", "true/false")
                    });
                    timeSeries.Components[0].Attributes[FunctionAttributes.StandardName] =
                        FunctionAttributes.StandardNames.RtcTimeCondition;
                    TimeSeries = timeSeries;
                }

                return timeSeries;
            }
            set
            {
                timeSeries = value;
            }
        }

        [ValidationMethod]
        public static void Validate(TimeCondition timeCondition)
        {
            var exceptions = new List<ValidationException>();

            if (timeCondition.Input != null)
            {
                exceptions.Add(new ValidationException(
                                   string.Format("Condition '{0}' has input; this is not supported for time conditions.", timeCondition.Name)));
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }

        public override string GetDescription() => "";

        public override object Clone()
        {
            var timeCondition = new TimeCondition();
            timeCondition.CopyFrom(this);
            return timeCondition;
        }

        public override void CopyFrom(object source)
        {
            var timeCondition = source as TimeCondition;
            if (timeCondition != null)
            {
                base.CopyFrom(source);
                Reference = timeCondition.Reference;
                TimeSeries = (TimeSeries)timeCondition.TimeSeries.Clone();
                InterpolationOptionsTime = timeCondition.InterpolationOptionsTime;
                Extrapolation = timeCondition.Extrapolation;
            }
        }

        public IEnumerable<object> GetDirectChildren()
        {
            if (timeSeries != null)
            {
                yield return timeSeries;
            }
        }
    }
}