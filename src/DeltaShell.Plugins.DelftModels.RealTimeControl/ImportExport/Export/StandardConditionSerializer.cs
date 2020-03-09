using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public class StandardConditionSerializer : ConditionSerializerBase
    {
        private readonly StandardCondition standardCondition;

        public StandardConditionSerializer(StandardCondition standardCondition) : base(standardCondition)
        {
            this.standardCondition = standardCondition;
        }

        protected override string XmlTag { get; } = RtcXmlTag.StandardCondition;

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
        /// Converts the information of the standard condition needed for writing the tools config file to an xml element.
        /// </summary>
        /// <param name="xNamespace"> The x namespace. </param>
        /// <param name="prefix"> The control group name. </param>
        /// <returns> The Xml Element. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            return ToXml(xNamespace, prefix, GetInputName());
        }
        
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
                                      GetX2Element(xNamespace)));
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

        protected virtual XElement GetX2Element(XNamespace xNamespace)
        {
            return new XElement(xNamespace + "x2Value", standardCondition.Value);
        }
    }
}