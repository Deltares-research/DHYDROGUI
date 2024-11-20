using System.Collections.Generic;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for a <see cref="RuleBase"/>.
    /// </summary>
    /// <seealso cref="RtcSerializerBase"/>
    public abstract class RuleSerializerBase : RtcSerializerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleSerializerBase"/> class.
        /// </summary>
        /// <param name="ruleBase"> The rule base to serialize. </param>
        protected RuleSerializerBase(RuleBase ruleBase) : base(ruleBase) {}

        /// <summary>
        /// some rule might require their output logged
        /// eg. Integral part for PID rule.
        /// </summary>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public virtual IEnumerable<XElement> OutputAsInputToDataConfigXml(XNamespace xNamespace)
        {
            yield break;
        }

        /// <summary>
        /// Converts the rule to a collection of <see cref="XElement"/>
        /// to be written to the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            yield return new XElement(xNamespace + "rule");
        }
    }
}