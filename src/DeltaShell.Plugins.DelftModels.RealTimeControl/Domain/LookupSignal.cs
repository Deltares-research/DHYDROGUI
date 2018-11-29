using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;
using log4net;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public class LookupSignal : SignalBase, IItemContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LookupSignal));
        private const string LookupTable = "lookupTable";

        public LookupSignal() : this(null)
        {
        }

        public LookupSignal(string name)
        {
            if (name != null) Name = name;
            Function = DefineFunction();
            XmlTag = RtcXmlTag.LookupSignal;
        }

        /// <summary>
        /// A function to store the table to make the hydraulic conversion
        /// This can either be a Discharge or 
        /// </summary>
        public Function Function { get; set; }

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
        /// Returns a IXmlTimeSeries that is written to rtcDataConfig.xml and only used internally by RTCTools.
        /// Only Name is required for this series.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static IXmlTimeSeries GetExportTimeSeries(string name)
        {
            return new XmlTimeSeries { Name = name };
        }

        public override IEnumerable<IXmlTimeSeries> XmlExportTimeSeries(string prefix)
        {
            yield return GetExportTimeSeries(prefix + Name);
        }

        public override XElement ToXml(XNamespace xNamespace, string prefix)
        {
            var result = base.ToXml(xNamespace, prefix);
            IEventedList<Record> table = new EventedList<Record>();
            foreach (var x in Function.Arguments[0].Values)
            {
                table.Add(new Record{X = (double) x, Y = (double) Function[x]});
            }

            var xElementsInput = Inputs.Select(input => input.ToXml(xNamespace, "x")).ToList();
            foreach (var xElementInput in xElementsInput)
            {
                var xElement = xElementInput.Elements().First();
                xElement.Add(new XAttribute("ref", "IMPLICIT"));
            }

            result.Add(new XElement(xNamespace + "lookupTable", new XAttribute("id", prefix + "/" + Name),
                       new XElement(xNamespace + "table", table.Select(record => record.ToXml(xNamespace))),
                       new XElement(xNamespace + "interpolationOption", Interpolation == InterpolationType.Constant ? "BLOCK" : "LINEAR"),
                       new XElement(xNamespace + "extrapolationOption", Extrapolation == ExtrapolationType.Constant ? "BLOCK" : "LINEAR"),
                       xElementsInput,
                       new XElement(xNamespace + "output", new XElement(xNamespace + "y", prefix + Name))));
            return result;
        }

        [NoNotifyPropertyChange]
        public InterpolationType Interpolation
        {
            get { return Function.Arguments.First().InterpolationType; }
            set
            {
                if (!Enum.IsDefined(typeof(InterpolationHydraulicType), (InterpolationHydraulicType)value))
                {
                    throw new ArgumentException(string.Format("Interpolation for lookup table rule does not support {0}", value));
                }
                Function.Arguments.First().InterpolationType = value;
            }
        }

        [NoNotifyPropertyChange]
        public ExtrapolationType Extrapolation
        {
            get { return Function.Arguments.First().ExtrapolationType; }
            set
            {
                if (!Enum.IsDefined(typeof(ExtrapolationHydraulicType), (ExtrapolationHydraulicType)value))
                {
                    throw new ArgumentException(string.Format("Extrapolation for lookup table rule does not support {0}", value));
                }
                Function.Arguments.First().ExtrapolationType = value;
            }
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
            var lookupSignal = (LookupSignal)Activator.CreateInstance(GetType());
            lookupSignal.CopyFrom(this);
            return lookupSignal;
        }

        public override void CopyFrom(object source)
        {
            var lookupSignal = source as LookupSignal;
            if (lookupSignal != null)
            {
                base.CopyFrom(source);
                Function = (Function)lookupSignal.Function.Clone();
                Interpolation = lookupSignal.Interpolation;
            }
        }

        public IEnumerable<object> GetDirectChildren()
        {
            yield return Function;
        }
    }
}
