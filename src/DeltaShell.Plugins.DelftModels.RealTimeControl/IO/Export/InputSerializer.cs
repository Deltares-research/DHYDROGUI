using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for an <see cref="Input"/>.
    /// </summary>
    /// <seealso cref="InputSerializerBase"/>
    public class InputSerializer : InputSerializerBase
    {
        private readonly Input input;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputSerializer"/> class.
        /// </summary>
        /// <param name="input"> The input to serialize. </param>
        public InputSerializer(Input input) : base(input)
        {
            this.input = input;
            XmlTag = RtcXmlTag.Input;
        }

        /// <summary>
        /// Gets the xml name of the input.
        /// </summary>
        /// <returns> The xml name of the input </returns>
        public override string GetXmlName()
        {
            return XmlTag + input.LocationName.Replace("##", "~~") + "/" + input.ParameterName;
        }

        /// <summary>
        /// Throws a <exception cref="NotSupportedException"/> when called.
        /// </summary>
        /// <param name="xNamespace"> This parameter is not used. </param>
        /// <param name="prefix"> This parameter is not used. </param>
        /// <exception cref="NotSupportedException">
        /// Thrown when this method is called.
        /// </exception>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            throw new NotSupportedException();
        }

        protected override string XmlTag { get; }
    }
}