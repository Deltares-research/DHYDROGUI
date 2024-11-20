using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for a <see cref="StandardCondition"/>.
    /// </summary>
    /// <seealso cref="ConditionSerializerBase"/>
    public class StandardConditionSerializer : ConditionSerializerBase
    {
        private readonly StandardCondition standardCondition;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardConditionSerializer"/> class.
        /// </summary>
        /// <param name="standardCondition"> The standard condition to serialize. </param>
        public StandardConditionSerializer(StandardCondition standardCondition) : base(standardCondition)
        {
            this.standardCondition = standardCondition;
            XmlTag = RtcXmlTag.StandardCondition;
        }

        /// <summary>
        /// Converts the information of the condition needed for writing the
        /// tools config file to a collection of <see cref="XElement"/>.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <param name="inputName"> The input name. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix, string inputName)
        {
            XElement result = base.ToXml(xNamespace, prefix).First();
            var standard = new XElement(xNamespace + "standard", new XAttribute("id", GetXmlNameWithTag(prefix)));
            standard.Add(new XElement(xNamespace + "condition",
                                      new XElement(xNamespace + "x1Series",
                                                   standardCondition.Reference == string.Empty
                                                       ? null
                                                       : new XAttribute("ref", standardCondition.Reference), inputName),
                                      new XElement(xNamespace + "relationalOperator",
                                                   standardCondition.Operation.ToString()),
                                      // see above comment
                                      GetX2Element(xNamespace, prefix)));
            if (standardCondition.TrueOutputs.OfType<RuleBase>().Any())
            {
                //rules
                standard.Add(new XElement(xNamespace + "true",
                                          standardCondition.TrueOutputs.OfType<RuleBase>().Select(
                                              rule =>
                                              {
                                                  RtcSerializerBase serializer =
                                                      SerializerCreator.CreateSerializerType(rule);
                                                  return serializer.ToXmlReference(
                                                      xNamespace, prefix);
                                              })));
            }

            if (standardCondition.TrueOutputs.OfType<ConditionBase>().Any() ||
                standardCondition.TrueOutputs.OfType<MathematicalExpression>().Any())
            {
                //conditions or mathematical expressions
                standard.Add(new XElement(xNamespace + "true", standardCondition
                                                               .TrueOutputs
                                                               .Where(to => to is ConditionBase ||
                                                                            to is MathematicalExpression).Select(to =>
                                                               {
                                                                   RtcSerializerBase serializer =
                                                                       SerializerCreator.CreateSerializerType(
                                                                           to);
                                                                   return
                                                                       serializer
                                                                           .ToXml(
                                                                               xNamespace,
                                                                               prefix);
                                                               })));
            }

            if (standardCondition.FalseOutputs.OfType<RuleBase>().Any())
            {
                //rules
                standard.Add(new XElement(xNamespace + "false",
                                          standardCondition.FalseOutputs.OfType<RuleBase>().Select(
                                              rule =>
                                              {
                                                  RtcSerializerBase serializer =
                                                      SerializerCreator.CreateSerializerType(rule);
                                                  return serializer.ToXmlReference(
                                                      xNamespace, prefix);
                                              })));
            }

            if (standardCondition.FalseOutputs.OfType<ConditionBase>().Any() ||
                standardCondition.FalseOutputs.OfType<MathematicalExpression>().Any())
            {
                //conditions
                standard.Add(new XElement(xNamespace + "false", standardCondition
                                                                .FalseOutputs
                                                                .Where(fo => fo is ConditionBase ||
                                                                             fo is MathematicalExpression).Select(fo =>

                                                                {
                                                                    RtcSerializerBase serializer =
                                                                        SerializerCreator.CreateSerializerType(
                                                                            fo);
                                                                    return serializer.ToXml(
                                                                        xNamespace,
                                                                        prefix);
                                                                })));
            }

            // output series with status info is required by RTC
            standard.Add(new XElement(xNamespace + "output",
                                      new XElement(xNamespace + "status",
                                                   RtcXmlTag.Status + GetXmlNameWithoutTag(prefix))));
            result.Add(standard);

            yield return result;
        }

        // Example of ToXmlInputReference:
        //     <standard id = "[StandardCondition]control_group_1/standard_condition">
        //         <condition>
        //             < x1Series ref="EXPLICIT">[Input]ObservationPoint1/Water level(op)</x1Series>
        //             <relationalOperator>LessEqual</relationalOperator>
        //             <x2Value>5</x2Value>
        //         </condition>
        //         <true>
        //             <trigger>
        //                 <ruleReference>[PID]control_group_1/pid_rule</ruleReference>
        //             </trigger>
        //         </true>
        //         <output>
        //             <status>[Status]control_group_1/standard_condition</status>
        //         </output>
        //     </standard>

        /// <summary>
        /// Converts the standard condition to a collection of <see cref="XElement"/>
        /// to be written to the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            return ToXml(xNamespace, prefix, GetInputName(prefix));
        }

        /// <summary>
        /// Gets the x2 element for the condition element in the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix">A string that can be used to prepend or append to the returned value. </param>
        /// <returns> The x2 element. </returns>
        protected virtual XElement GetX2Element(XNamespace xNamespace, string prefix)
        {
            return new XElement(xNamespace + "x2Value", standardCondition.Value);
        }
    }
}