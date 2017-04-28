using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;
using log4net;
using ValidationAspects;
using ValidationAspects.Exceptions;
using ValidationAspects.Factories;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public class RelativeTimeRule : RuleBase, IItemContainer
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(RelativeTimeRule));
        private int minimumPeriod = 0;

        private string TimeValueOption
        {
            get { return FromValue ? "RELATIVE" : "ABSOLUTE"; }
        }

        /// <summary>
        /// Function that holds the relative time series
        /// </summary>
        private Function function;
        public Function Function
        {
            get
            {
                if(function == null)
                {
                    function = DefineFunction();
                }

                return function;
            } 
            set { function = value; }
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
            get { return minimumPeriod; }
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

        private const string LookupTable = "lookupTable";

        public RelativeTimeRule()
            : this(null, false)
        {
        }

        public RelativeTimeRule(string name, bool fromValue)
        {
            if (name != null) Name = name;
            FromValue = fromValue;
        }

        public static Function DefineFunction()
        {
            var function = new Function();
            function.Arguments.Add(new Variable<double>("seconds"){
                InterpolationType = InterpolationType.Constant,
                ExtrapolationType = ExtrapolationType.None}
                );
            function.Components.Add(new Variable<double>("value"));
            function.Name = LookupTable;
            return function;
        }

        [NoNotifyPropertyChange]
        public InterpolationType Interpolation
        {
            get { return Function.Arguments.First().InterpolationType; }
            set { Function.Arguments.First().InterpolationType = value; }
        }

        public string TimeActive
        {
            get { return Name + "_t"; }
        }

        public override IEnumerable<IXmlTimeSeries> XmlExportTimeSeries(string prefix)
        {
            yield return GetExportTimeSeries(prefix);
        }

        private string OutputAsInput(Output outputAsInput)
        {
            return outputAsInput.Name + "_AsInputFor_" + Name;
        }

        public override XElement ToXml(XNamespace xNamespace, string prefix)
        {
            var result = base.ToXml(xNamespace, prefix);
            IList<Record> table = GetTable();

            foreach (var output in Outputs)
            {
                output.IntegralPart = prefix + TimeActive; // also in data export and statevector
            }
            var element = new XElement(xNamespace + "timeRelative",
                                       new XAttribute("id", prefix + Name),
                                       new XElement(xNamespace + "mode", "RETAINVALUEWHENINACTIVE"),
                                       new XElement(xNamespace + "valueOption", TimeValueOption),
                                       new XElement(xNamespace + "maximumPeriod", MinimumPeriod.ToString()),
                                       new XElement(xNamespace + "controlTable",
                                                    table.Select(record => record.ToXml(xNamespace)))
                                       );
            if (FromValue)
            {
                // set an extra input to RtcTools. This input is the same output of the rule
                // this is not visible in the UI; the user does not have to make the connection
                var extraInput = new XElement(xNamespace + "input");
                extraInput.Add(new XElement(xNamespace + "y", OutputAsInput(Outputs[0])));
                element.Add(extraInput);

            }
            element.Add(Outputs.Select(output => output.ToXml(xNamespace, "y", "timeActive")));
            result.Add(element);
            return result;
        }

        public override IEnumerable<XElement> OutputAsInputToDataConfigXml(XNamespace xNamespace)
        {
            if (FromValue)
            {
                var outputAsInput = Outputs[0];
                
                // if RelativeTimeRuleFromValue set by rule also be set as input.
                // RTCTools has problem with dupicate series with same name (input and output)
                // and will not function correctly. Generate new name for RTCTools in XML.
                // When result are passed back to controlled model identification is based on
                // Feature and  and thus connect to the same Connection.
                var tempElement = new XElement(xNamespace + "timeSeries",
                                               new XAttribute("id", OutputAsInput(outputAsInput)));

                tempElement.Add(new XElement(xNamespace + "OpenMIExchangeItem",
                                             new XElement(xNamespace + "elementId",
                                                          outputAsInput.LocationName),
                                             new XElement(xNamespace + "quantityId",
                                                          outputAsInput.ParameterName),
                                             new XElement(xNamespace + "unit", "m")
                                    ));
                yield return tempElement;
            }
        }


        private IList<Record> GetTable()
        {
            var table = new List<Record>();
            foreach (var x in Function.Arguments[0].Values)
            {
                table.Add(new Record {XLabel = "time", YLabel = "value", X = (double) x, Y = (double) Function[x]});
            }
            
            // add an extra record to the end to fix the behaviour of RTCTools where the extrapolation
            // of the table is always set to extrapolation
            // email D. Schwanenberg 20110321 
            // "The extrapolation is set to linear by default. 
            // Thus
            //         <controlTable>
            //           <record time="0" value="33" />
            //           <record time="10000" value="66" />
            //         </controlTable>
            // means
            //         <controlTable>
            //           <record time="0" value="33" />
            //           <record time="10000" value="66" />
            // <record time="20000" value="99" />
            //         </controlTable>
            // Otherwise, use
            //         <controlTable>
            //           <record time="0" value="33" />
            //           <record time="10000" value="66" />
            // <record time="10001" value="66" />
            //         </controlTable>"

            if (table.Count > 0)
            {
                table.Add(new Record
                              {
                                  XLabel = "time",
                                  YLabel = "value",
                                  X = table[table.Count - 1].X + 1,
                                  Y = table[table.Count - 1].Y
                              });
            }

            return table;
        }

        private IXmlTimeSeries GetExportTimeSeries(string prefix)
        {
            var xmlTimeSeries = new XmlTimeSeries
                                    {
                                        Name = prefix + TimeActive,
                                        LocationId = prefix + Name,
                                        ParameterId = "t",
                                    };
            return xmlTimeSeries;
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
            var relativeTimeRule = (RelativeTimeRule)Activator.CreateInstance(GetType());
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

        public IEnumerable<object> GetDirectChildren()
        {
            if (function != null)
                yield return function;
        }
    }
}