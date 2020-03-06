using System.Collections.Generic;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public abstract class ConditionSerializerBase : RtcSerializerBase
    {
        private readonly ConditionBase conditionBase;

        protected ConditionSerializerBase(ConditionBase conditionBase) : base(conditionBase)
        {
            this.conditionBase = conditionBase;
        }

        /// <summary>
        /// Converts the information of the condition needed for writing the tools config file to an xml element.
        /// </summary>
        /// <param name="xNamespace"> The x namespace. </param>
        /// <param name="prefix"> The control group name. </param>
        /// <returns> The Xml Element. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            yield return new XElement(xNamespace + "trigger");
        }

        public virtual IEnumerable<XElement> ToDataConfigImportSeries(string prefix, XNamespace xNamespace)
        {
            yield break;
        }

        public virtual IEnumerable<XElement> ToDataConfigExportSeries(XNamespace xNamespace, string prefix)
        {
            yield return new XElement(xNamespace + "timeSeries",
                                      new XAttribute("id", RtcXmlTag.Status + GetXmlNameWithoutTag(prefix)));
        }

        protected string GetInputName()
        {
            IInput conditionInput = conditionBase.Input;

            var serializer =
                SerializerCreator.CreateSerializerType<InputSerializerBase>((RtcBaseObject)conditionInput);

            return serializer == null ? "|no input|" : serializer.GetXmlName();
        }
    }
}