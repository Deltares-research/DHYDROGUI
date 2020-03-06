using System.Collections.Generic;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public abstract class RuleSerializerBase : RtcSerializerBase
    {
        protected RuleSerializerBase(RuleBase ruleBase) : base(ruleBase) {}

        /// <summary>
        /// Converts the information of the rule needed for writing the tools config file to an xml element.
        /// </summary>
        /// <param name="xNamespace"> The x namespace. </param>
        /// <param name="prefix"> The control group name. </param>
        /// <returns> The Xml Element. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            yield return new XElement(xNamespace + "rule");
        }

        /// <summary>
        /// some rule might require their output logged
        /// eg. Integral part for PID rule
        /// </summary>
        /// <returns> </returns>
        public virtual IEnumerable<XElement> OutputAsInputToDataConfigXml(XNamespace xNamespace)
        {
            yield break;
        }

        /// <summary>
        /// implement this if the rule needs to write some state to the
        /// state_import.xml file.
        /// eg. Integral part for PID rule
        /// </summary>
        /// <returns> </returns>
        public virtual IEnumerable<XElement> ToImportState(XNamespace xNamespace)
        {
            yield break;
        }
    }
}