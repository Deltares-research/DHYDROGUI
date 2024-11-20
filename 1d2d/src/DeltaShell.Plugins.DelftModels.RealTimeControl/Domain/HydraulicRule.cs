using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop;
using log4net;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public class HydraulicRule : RuleBase
    {
        private const string LookupTable = "lookupTable";
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydraulicRule));
        private int timeLag = 0;
        private int timeLagInTimeSteps = 0;

        public HydraulicRule()
        {
            Function = DefineFunction();
        }

        /// <summary>
        /// A function to store the table to make the hydraulic conversion
        /// This can either be a Discharge or
        /// </summary>
        public Function Function { get; set; }

        [NoNotifyPropertyChange]
        public InterpolationType Interpolation
        {
            get
            {
                return Function.Arguments.First().InterpolationType;
            }
            set
            {
                if (!Enum.IsDefined(typeof(InterpolationHydraulicType), (InterpolationHydraulicType) value))
                {
                    throw new ArgumentException(string.Format("Interpolation for lookup table rule does not support {0}", value));
                }

                Function.Arguments.First().InterpolationType = value;
            }
        }

        [NoNotifyPropertyChange]
        public ExtrapolationType Extrapolation
        {
            get
            {
                return Function.Arguments.First().ExtrapolationType;
            }
            set
            {
                if (!Enum.IsDefined(typeof(ExtrapolationHydraulicType), (ExtrapolationHydraulicType) value))
                {
                    throw new ArgumentException(string.Format("Extrapolation for lookup table rule does not support {0}", value));
                }

                Function.Arguments.First().ExtrapolationType = value;
            }
        }

        //In seconds
        public int TimeLag
        {
            get
            {
                return timeLag;
            }
            set
            {
                if (value >= 0)
                {
                    timeLag = value;
                }
                else
                {
                    Log.Error("Time Lag must be 0 or greater.");
                }
            }
        }

        // TimeLag in n TimeSteps. Calculated after SetTimeLagToTimeSteps(TimeSpan modelTimeSte)
        public int TimeLagInTimeSteps
        {
            get
            {
                return timeLagInTimeSteps;
            }
        }

        public static Function DefineFunction()
        {
            var function = new Function();
            function.Arguments.Add(new Variable<double>
            {
                Name = "x",
                InterpolationType = InterpolationType.Constant,
                ExtrapolationType = ExtrapolationType.Constant
            });
            function.Components.Add(new Variable<double>("f"));
            function.Name = LookupTable;
            return function;
        }

        /// <summary>
        /// Sets the time lag in amount of time steps
        /// Requered value is amount of time steps, so timeLag/modelTimeStep
        /// </summary>
        /// <param name="modelTimeStep"></param>
        public void SetTimeLagToTimeSteps(TimeSpan modelTimeStep)
        {
            double factorTimeSteps = (double) timeLag / modelTimeStep.TotalSeconds;
            double nTimeSteps = Math.Floor(factorTimeSteps);

            if (factorTimeSteps != nTimeSteps)
            {
                Log.WarnFormat("Rule {0} has a timelag ({1} seconds) which is not a multiple model time step ({2} seconds). The timelag has been set on {3} timesteps.", Name, timeLag, modelTimeStep.Seconds, nTimeSteps.ToString("N0"));
            }

            timeLagInTimeSteps = Convert.ToInt32(nTimeSteps);
        }

        [ValidationMethod]
        public static void Validate(HydraulicRule hydraulicRule)
        {
            var exceptions = new List<ValidationException>();

            if (hydraulicRule.Function.Arguments[0].Values.Count == 0)
            {
                exceptions.Add(new ValidationException(string.Format("Lookup table rule '{0}' has an empty lookup table.", hydraulicRule.Name)));
            }

            if (hydraulicRule.Inputs.Count != 1)
            {
                exceptions.Add(new ValidationException(string.Format("Lookup table rule '{0}' requires exactly 1 input.", hydraulicRule.Name)));
            }

            if (hydraulicRule.Outputs.Count != 1)
            {
                exceptions.Add(new ValidationException(string.Format("Lookup table rule '{0}' requires exactly 1 output.", hydraulicRule.Name)));
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }

        public override object Clone()
        {
            var hydraulicRule = new HydraulicRule();
            hydraulicRule.CopyFrom(this);
            return hydraulicRule;
        }

        public override void CopyFrom(object source)
        {
            var hydraulicRule = source as HydraulicRule;
            if (hydraulicRule != null)
            {
                base.CopyFrom(source);
                Function = (Function) hydraulicRule.Function.Clone();
                Interpolation = hydraulicRule.Interpolation;
                Extrapolation = hydraulicRule.Extrapolation;
                TimeLag = hydraulicRule.TimeLag;
                timeLagInTimeSteps = hydraulicRule.TimeLagInTimeSteps;
            }
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            yield return Function;
        }
    }
}