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
    public class RelativeTimeRule : RuleBase
    {
        private const string LookupTable = "lookupTable";

        private static readonly ILog Log = LogManager.GetLogger(typeof(RelativeTimeRule));
        private int minimumPeriod = 0;

        /// <summary>
        /// Function that holds the relative time series
        /// </summary>
        private Function function;

        public RelativeTimeRule()
            : this(null, false) {}

        public RelativeTimeRule(string name, bool fromValue)
        {
            if (name != null)
            {
                Name = name;
            }

            FromValue = fromValue;
        }

        public Function Function
        {
            get
            {
                if (function == null)
                {
                    function = DefineFunction();
                }

                return function;
            }
            set
            {
                function = value;
            }
        }

        /// <summary>
        /// If true the rule uses the controlled parameter to determine the start position in the
        /// function
        /// </summary>
        public bool FromValue { get; set; }

        /// <summary>
        /// Minimum period between two active periods of the time controller.
        /// </summary>
        public int MinimumPeriod
        {
            get
            {
                return minimumPeriod;
            }
            set
            {
                if (value >= 0)
                {
                    minimumPeriod = value;
                }
                else
                {
                    Log.Error("Minimum Period must be 0 or greater.");
                }
            }
        }

        [NoNotifyPropertyChange]
        public InterpolationType Interpolation
        {
            get
            {
                return Function.Arguments.First().InterpolationType;
            }
            set
            {
                Function.Arguments.First().InterpolationType = value;
            }
        }

        public static Function DefineFunction()
        {
            var function = new Function();
            function.Arguments.Add(
                new Variable<double>("seconds")
                {
                    InterpolationType = InterpolationType.Constant,
                    ExtrapolationType = ExtrapolationType.None
                }
            );
            function.Components.Add(new Variable<double>("value"));
            function.Name = LookupTable;
            return function;
        }

        [ValidationMethod]
        public static void Validate(RelativeTimeRule relativeTimeRule)
        {
            var exceptions = new List<ValidationException>();

            if (relativeTimeRule.Inputs.Count > 0)
            {
                exceptions.Add(
                    new ValidationException(string.Format("Time rule '{0}' does not support input items.",
                                                          relativeTimeRule.Name)));
            }

            if (relativeTimeRule.Function.Arguments[0].Values.Count == 0)
            {
                exceptions.Add(new ValidationException(string.Format("RelativeTimeRule '{0}' has empty time series.",
                                                                     relativeTimeRule.Name)));
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }

        public override object Clone()
        {
            var relativeTimeRule = new RelativeTimeRule();
            relativeTimeRule.CopyFrom(this);
            return relativeTimeRule;
        }

        public override void CopyFrom(object source)
        {
            var relativeTimeRule = source as RelativeTimeRule;
            if (relativeTimeRule != null)
            {
                base.CopyFrom(source);
                FromValue = relativeTimeRule.FromValue;
                Function = (Function) relativeTimeRule.Function.Clone();
                Interpolation = relativeTimeRule.Interpolation;
                MinimumPeriod = relativeTimeRule.MinimumPeriod;
            }
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            if (function != null)
            {
                yield return function;
            }
        }
    }
}