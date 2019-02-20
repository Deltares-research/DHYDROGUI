using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using log4net;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public class HydraulicRule : RuleBase, IItemContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydraulicRule));
        private const string LookupTable = "lookupTable";
        private int timeLag = 0;
        private int timeLagInTimeSteps = 0;

        public HydraulicRule()
        {
            Function = DefineFunction();
            XmlTag = RtcXmlTag.HydraulicRule;
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

        // Example of ToXml:
        //  <lookupTable id ="[HydraulicRule]control_group_1/lookup_table_rule" >
        //      <table>
        //          <record x="1" y="5"/>
        //          <record x="2" y="4"/>
        //          <record x="3" y="3"/>
        //      </table>
        //      <interpolationOption>LINEAR</interpolationOption>
        //      <extrapolationOption>BLOCK</extrapolationOption>
        //      <input>
        //          <x ref="EXPLICIT">[Delayed][Input]ObservationPoint1/Water level(op)[0]</x>
        //      </input>
        //      <output>
        //          <y>[Output]Weir1/Crest level(s)</y>
        //      </output>
        //  </lookupTable>

        /// <summary>
        /// Converts the information of the hydraulic rule needed for writing the tools config file to an xml element.
        /// </summary>
        /// <param name="xNamespace">The x namespace.</param>
        /// <param name="prefix">The control group name.</param>
        /// <returns>The Xml Element.</returns>
        public override XElement ToXml(XNamespace xNamespace, string prefix)
        {
            var result = base.ToXml(xNamespace, prefix);
            IEventedList<Record> table = new EventedList<Record>();
            foreach (var x in Function.Arguments[0].Values)
            {
                table.Add(new Record{X = (double) x, Y = (double) Function[x]});
            }

            var xElementsInput = Inputs.Select(input => input.ToXml(xNamespace, "x")).ToList();

            if(timeLagInTimeSteps > 0)
            {
                //input will be an unitDelay component with the name "delayed<Name>"
                //a time lag index is needed as 'pointer' and will be added to the name [timeLagInTimeSteps]
                foreach (var xElementInput in xElementsInput)
                {
                    var xElement = xElementInput.Elements().First();
                    // we need the last element from vector with length (timeLagInTimeSteps-1)
                    xElement.Value = string.Format(RtcXmlTag.Delayed + xElement.Value + $"[{timeLagInTimeSteps - 2}]");
                    xElement.Add(new XAttribute("ref","EXPLICIT"));
                }
            }
            else
            {
                foreach (var xElementInput in xElementsInput)
                {
                    var xElement = xElementInput.Elements().First();
                    xElement.Add(new XAttribute("ref", "IMPLICIT"));
                }

            }

            result.Add(new XElement(xNamespace + "lookupTable",
                            new XAttribute("id", GetXmlNameWithTag(prefix)),
                            new XElement(xNamespace + "table", table.Select(record => record.ToXml(xNamespace))),
                            new XElement(xNamespace + "interpolationOption", Interpolation == InterpolationType.Constant ? "BLOCK" : "LINEAR"),
                            new XElement(xNamespace + "extrapolationOption", Extrapolation == ExtrapolationType.Constant ? "BLOCK" : "LINEAR"),
                            xElementsInput,
                            Outputs.Select(output => output.ToXml(xNamespace, "y", null))));
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

        //In seconds
        public int TimeLag
        {
            get { return timeLag; }
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
            get { return timeLagInTimeSteps; }
        }

        /// <summary>
        /// Sets the time lag in amount of time steps
        /// Requered value is amount of time steps, so timeLag/modelTimeStep
        /// </summary>
        /// <param name="modelTimeStep"></param>
        public void SetTimeLagToTimeSteps(TimeSpan modelTimeStep)
        {
            double factorTimeSteps = (double)timeLag/modelTimeStep.TotalSeconds;
            double nTimeSteps = Math.Floor(factorTimeSteps);

            if (factorTimeSteps != nTimeSteps)
            {
                Log.WarnFormat("Rule {0} has a timelag ({1} seconds) which is not a multiple model time step ({2} seconds). The timelag has been set on {3} timesteps.", this.Name, timeLag, modelTimeStep.Seconds, nTimeSteps.ToString("N0"));
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
            var hydraulicRule = (HydraulicRule)Activator.CreateInstance(GetType());
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

        public IEnumerable<object> GetDirectChildren()
        {
            yield return Function;
        }
    }
}
