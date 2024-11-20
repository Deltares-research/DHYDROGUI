using System.Collections.Generic;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for a <see cref="SignalBase"/>.
    /// </summary>
    /// <seealso cref="RtcSerializerBase"/>
    public abstract class SignalSerializerBase : RtcSerializerBase
    {
        private readonly SignalBase signalBase;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalSerializerBase"/> class.
        /// </summary>
        /// <param name="signalBase">The signal base to serialize.</param>
        protected SignalSerializerBase(SignalBase signalBase) : base(signalBase)
        {
            this.signalBase = signalBase;
        }

        /// <summary>
        /// Converts the signal to a collection of <see cref="XElement"/>
        /// to be written to the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            yield return signalBase.StoreAsRule
                             ? new XElement(xNamespace + "rule")
                             : new XElement(xNamespace + "signal");
        }
    }
}