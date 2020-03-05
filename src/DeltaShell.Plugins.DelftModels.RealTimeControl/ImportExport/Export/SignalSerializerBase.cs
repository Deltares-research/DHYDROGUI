using System.Collections.Generic;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public abstract class SignalSerializerBase : RtcSerializerBase
    {
        private readonly SignalBase signalBase;

        public SignalSerializerBase(SignalBase signalBase) : base(signalBase)
        {
            this.signalBase = signalBase;
        }

        /// <summary>
        /// Converts the information the signal needed for writing the tools config file to an xml element.
        /// </summary>
        /// <param name="xNamespace"> The x namespace. </param>
        /// <param name="prefix"> The control group name. </param>
        /// <returns> The Xml Element. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            yield return signalBase.StoreAsRule
                             ? new XElement(xNamespace + "rule")
                             : new XElement(xNamespace + "signal");
        }
    }
}