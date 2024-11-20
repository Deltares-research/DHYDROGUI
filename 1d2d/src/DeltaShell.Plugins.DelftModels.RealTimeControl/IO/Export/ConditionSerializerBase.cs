using System.Collections.Generic;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for a <see cref="ConditionBase"/>.
    /// </summary>
    /// <seealso cref="RtcSerializerBase"/>
    public abstract class ConditionSerializerBase : RtcSerializerBase
    {
        private readonly ConditionBase conditionBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionSerializerBase"/> class.
        /// </summary>
        /// <param name="conditionBase"> The condition to serialize. </param>
        protected ConditionSerializerBase(ConditionBase conditionBase) : base(conditionBase)
        {
            this.conditionBase = conditionBase;
        }

        /// <summary>
        /// Converts the condition to a collection of <see cref="XElement"/>
        /// to be written to the import series in the data config xml file.
        /// </summary>
        /// <param name="prefix"> The prefix. </param>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public virtual IEnumerable<XElement> ToDataConfigImportSeries(string prefix, XNamespace xNamespace)
        {
            yield break;
        }

        /// <summary>
        /// Converts the condition to a collection of <see cref="XElement"/>
        /// to be written to export series in the data config xml file.
        /// </summary>
        /// <param name="prefix"> The prefix. </param>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public virtual IEnumerable<XElement> ToDataConfigExportSeries(XNamespace xNamespace, string prefix)
        {
            yield return new XElement(xNamespace + "timeSeries",
                                      new XAttribute("id", RtcXmlTag.Status + GetXmlNameWithoutTag(prefix)));
        }

        /// <summary>
        /// Converts the condition to a collection of <see cref="XElement"/>
        /// to be written to the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            yield return new XElement(xNamespace + "trigger");
        }

        /// <summary>
        /// Gets the xml name of the input of the condition.
        /// </summary>
        /// <returns>The xml name of the input of the condition.</returns>
        protected string GetInputName()
        {
            IInput conditionInput = conditionBase.Input;

            return conditionInput == null
                       ? "|no input|"
                       : SerializerCreator.CreateSerializerType<InputSerializerBase>(
                           (RtcBaseObject) conditionInput).GetXmlName();
        }
    }
}