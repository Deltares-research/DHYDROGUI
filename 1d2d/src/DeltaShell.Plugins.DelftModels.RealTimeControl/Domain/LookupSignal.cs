using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public class LookupSignal : SignalBase, IItemContainer
    {
        private const string lookupTable = "lookupTable";

        public LookupSignal() : this(null) {}

        public LookupSignal(string name)
        {
            if (name != null)
            {
                Name = name;
            }

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
            function.Name = lookupTable;
            return function;
        }

        [ValidationMethod]
        public static void Validate(LookupSignal lookupSignal)
        {
            var exceptions = new List<ValidationException>();

            if (lookupSignal.Function.Arguments[0].Values.Count == 0)
            {
                exceptions.Add(new ValidationException(string.Format("Lookup signal '{0}' has an empty lookup table.", lookupSignal.Name)));
            }

            if (lookupSignal.Inputs.Count != 1)
            {
                exceptions.Add(new ValidationException(string.Format("Lookup signal '{0}' requires exactly 1 input.", lookupSignal.Name)));
            }

            if (lookupSignal.RuleBases.Count != 1)
            {
                exceptions.Add(new ValidationException(string.Format("Lookup signal '{0}' requires exactly 1 rule.", lookupSignal.Name)));
            }

            if (exceptions.Count > 0)
            {
                throw new ValidationContextException(exceptions);
            }
        }

        public override object Clone()
        {
            var lookupSignal = new LookupSignal();
            lookupSignal.CopyFrom(this);
            return lookupSignal;
        }

        public override void CopyFrom(object source)
        {
            var lookupSignal = source as LookupSignal;
            if (lookupSignal != null)
            {
                base.CopyFrom(source);
                Function = (Function) lookupSignal.Function.Clone();
                Interpolation = lookupSignal.Interpolation;
            }
        }

        public IEnumerable<object> GetDirectChildren()
        {
            yield return Function;
        }
    }
}