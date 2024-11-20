using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for a <see cref="HydraulicRule"/>.
    /// </summary>
    /// <seealso cref="RuleSerializerBase"/>
    public class HydraulicRuleSerializer : RuleSerializerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HydraulicRuleSerializer"/> class.
        /// </summary>
        /// <param name="hydraulicRule">The hydraulic rule to serialize.</param>
        public HydraulicRuleSerializer(HydraulicRule hydraulicRule) : base(hydraulicRule)
        {
            HydraulicRule = hydraulicRule;
            XmlTag = RtcXmlTag.HydraulicRule;
        }

        // Example of ToXmlInputReference:
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
        /// Converts the hydraulic rule to a collection of <see cref="XElement"/>
        /// to be written to the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            XElement result = base.ToXml(xNamespace, prefix).First();
            IEventedList<Record> table = new EventedList<Record>();
            foreach (object x in HydraulicRule.Function.Arguments[0].Values)
            {
                table.Add(new Record
                {
                    X = (double) x,
                    Y = (double) HydraulicRule.Function[x]
                });
            }

            List<XElement> xElementsInput =
                HydraulicRule.Inputs.Select(input =>
                {
                    var serializer = SerializerCreator.CreateSerializerType<InputSerializerBase>(input);
                    return serializer.ToXmlInputReference(xNamespace, "x");
                }).ToList();

            if (HydraulicRule.TimeLagInTimeSteps > 0)
            {
                //input will be an unitDelay component with the name "delayed<Name>"
                //a time lag index is needed as 'pointer' and will be added to the name [timeLagInTimeSteps]
                foreach (XElement xElementInput in xElementsInput)
                {
                    XElement xElement = xElementInput.Elements().First();
                    // we need the last element from vector with length (timeLagInTimeSteps-1)
                    xElement.Value =
                        string.Format(RtcXmlTag.Delayed + xElement.Value + $"[{HydraulicRule.TimeLagInTimeSteps - 2}]");
                    xElement.Add(new XAttribute("ref", "EXPLICIT"));
                }
            }
            else
            {
                foreach (XElement xElementInput in xElementsInput)
                {
                    XElement xElement = xElementInput.Elements().First();
                    xElement.Add(new XAttribute("ref", "IMPLICIT"));
                }
            }

            result.Add(new XElement(xNamespace + "lookupTable",
                                    new XAttribute("id", GetXmlNameWithTag(prefix)),
                                    new XElement(xNamespace + "table",
                                                 table.Select(record => record.ToXml(xNamespace))),
                                    new XElement(xNamespace + "interpolationOption",
                                                 HydraulicRule.Interpolation == InterpolationType.Constant
                                                     ? "BLOCK"
                                                     : "LINEAR"),
                                    new XElement(xNamespace + "extrapolationOption",
                                                 HydraulicRule.Extrapolation == ExtrapolationType.Constant
                                                     ? "BLOCK"
                                                     : "LINEAR"),
                                    xElementsInput,
                                    HydraulicRule.Outputs.Select(
                                        output =>
                                        {
                                            var serializer = new OutputSerializer(output);
                                            return serializer.ToXmlOutputReference(xNamespace, "y", null);
                                        })));
            yield return result;
        }

        private HydraulicRule HydraulicRule { get; }
    }
}