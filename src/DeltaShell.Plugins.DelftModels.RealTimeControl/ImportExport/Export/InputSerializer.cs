using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    public class InputSerializer : InputSerializerBase 
    {
        private readonly Input input;

        public InputSerializer(Input input) : base(input)
        {
            this.input = input;
            XmlTag = RtcXmlTag.Input;
        }

        public override string GetXmlName()
        {
            return XmlTag + input.LocationName.Replace("##", "~~") + "/" + input.ParameterName;
        }

        protected override string XmlTag { get; }

        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            throw new NotSupportedException();
        }
    }
}