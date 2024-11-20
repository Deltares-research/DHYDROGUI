using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for a <see cref="RelativeTimeRule"/>.
    /// </summary>
    /// <seealso cref="RuleSerializerBase"/>
    public class RelativeTimeRuleSerializer : RuleSerializerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeTimeRuleSerializer"/> class.
        /// </summary>
        /// <param name="relativeTimeRule">The relative time rule to serialize.</param>
        public RelativeTimeRuleSerializer(RelativeTimeRule relativeTimeRule) : base(relativeTimeRule)
        {
            RelativeTimeRule = relativeTimeRule;
        }

        // Example of ToXmlInputReference:
        //  <timeRelative id = "[RelativeTimeRule]control_group_1/relative_time_rule">
        //      <mode>RETAINVALUEWHENINACTIVE</mode>
        //      <valueOption>ABSOLUTE</valueOption>
        //      <maximumPeriod>0</maximumPeriod>
        //      <controlTable>
        //          <record time="60" value="10"/>
        //          <record time ="600" value="9"/>
        //          <record time ="1800" value="8"/>
        //      </controlTable>
        //      <output>
        //          <y>[Output] Weir1/Crest level(s)</y>
        //          <timeActive>control_group_1/relative_time_rule</timeActive>
        //      </output>
        //  </timeRelative>

        /// <summary>
        /// Converts the relative time rule to a collection of <see cref="XElement"/>
        /// to be written to the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            XElement result = base.ToXml(xNamespace, prefix).First();
            IList<Record> table = GetTable();
            foreach (Output output in RelativeTimeRule.Outputs)
            {
                output.IntegralPart = GetXmlNameWithoutTag(prefix); // also in data export and statevector
            }

            string interpolationString =
                RelativeTimeRule.Interpolation == InterpolationType.Linear ? "LINEAR" : "BLOCK";

            var element = new XElement(xNamespace + "timeRelative",
                                       new XAttribute("id", GetXmlNameWithTag(prefix)),
                                       new XElement(xNamespace + "mode", "RETAINVALUEWHENINACTIVE"),
                                       new XElement(xNamespace + "valueOption", TimeValueOption),
                                       new XElement(xNamespace + "maximumPeriod",
                                                    RelativeTimeRule.MinimumPeriod.ToString()),
                                       new XElement(xNamespace + "interpolationOption", interpolationString),
                                       new XElement(xNamespace + "controlTable",
                                                    table.Select(record => record.ToXml(xNamespace)))
            );
            if (RelativeTimeRule.FromValue)
            {
                // set an extra input to RtcTools. This input is the same output of the rule
                // this is not visible in the UI; the user does not have to make the connection
                var extraInput = new XElement(xNamespace + "input");
                extraInput.Add(new XElement(xNamespace + "y", OutputAsInput(RelativeTimeRule.Outputs[0])));
                element.Add(extraInput);
            }

            element.Add(RelativeTimeRule.Outputs.Select(output =>
            {
                var serializer = new OutputSerializer(output);
                return serializer.ToXmlOutputReference(xNamespace, "y", "timeActive");
            }));
            result.Add(element);
            yield return result;
        }

        /// <summary>
        /// Converts the relative time rule to a collection of <see cref="XElement"/>
        /// to be written to the import series in the data config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> OutputAsInputToDataConfigXml(XNamespace xNamespace)
        {
            if (RelativeTimeRule.FromValue)
            {
                Output outputAsInput = RelativeTimeRule.Outputs[0];

                // if RelativeTimeRuleFromValue set by rule also be set as input.
                // RTCTools has problem with duplicate series with same name (input and output)
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

        /// <summary>
        /// Converts the relative time rule to a collection of <see cref="XElement"/>
        /// to be written to the export series in the data config xml file.
        /// </summary>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<IXmlTimeSeries> XmlExportTimeSeries(string prefix)
        {
            yield return GetExportTimeSeries(prefix);
        }

        protected override string XmlTag { get; } = RtcXmlTag.RelativeTimeRule;
        private RelativeTimeRule RelativeTimeRule { get; }

        private string TimeValueOption => RelativeTimeRule.FromValue ? "RELATIVE" : "ABSOLUTE";

        private string OutputAsInput(Output outputAsInput)
        {
            var serializer = new OutputSerializer(outputAsInput);
            return serializer.GetXmlName() + RtcXmlTag.OutputAsInput + RelativeTimeRule.Name;
        }

        private IList<Record> GetTable()
        {
            var table = new List<Record>();
            foreach (object x in RelativeTimeRule.Function.Arguments[0].Values)
            {
                table.Add(new Record
                {
                    XLabel = "time",
                    YLabel = "value",
                    X = (double) x,
                    Y = (double) RelativeTimeRule.Function[x]
                });
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

            if (table.Count > 0 && !HasExtrapolationFix(table))
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

        /// <summary>
        /// Determines whether <paramref name="table"/> already contains a fix for the extrapolation
        /// behaviour of RTCTools.
        /// </summary>
        /// <param name="table"> The table. </param>
        /// <returns>
        /// <c> true </c> if [the specified table] [has extrapolation fix] ; otherwise, <c> false </c>.
        /// </returns>
        /// <remarks> table != null && table.Count >= 1 </remarks>
        private static bool HasExtrapolationFix(IList<Record> table)
        {
            int nElements = table.Count;
            if (nElements == 1)
            {
                return false;
            }

            return table[nElements - 1].Y == table[nElements - 2].Y;
        }

        private IXmlTimeSeries GetExportTimeSeries(string prefix)
        {
            var xmlTimeSeries = new XmlTimeSeries
            {
                Name = GetXmlNameWithoutTag(prefix),
                LocationId = GetXmlNameWithTag(prefix),
                ParameterId = "t"
            };
            return xmlTimeSeries;
        }
    }
}