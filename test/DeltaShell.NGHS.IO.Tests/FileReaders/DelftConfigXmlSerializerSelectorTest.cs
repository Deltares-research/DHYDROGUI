using System.Xml.Serialization;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Factories;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
   public class DelftConfigXmlSerializerSelectorTest
    {
        private DelftConfigXmlSerializerSelector selector;

        [TestFixtureSetUp]
        public void Setup()
        {
            selector = new DelftConfigXmlSerializerSelector();
        }

        [Test]
        public void GetXmlSerializerWithDimrXmlFileType()
        {
            var rootName = "dimrConfig";

            var result = selector.ReturnSerializer(rootName);

            var typeName = TypeUtils.GetField(result, "mapping");
            var xmlFileType = TypeUtils.GetPropertyValue(typeName, "TypeName");

            Assert.That(result, Is.TypeOf<XmlSerializer>());
            Assert.That(xmlFileType, Is.EqualTo("dimrXML"));
        }

        [Test]
        public void GetXmlSerializerWithRtcDataConfigXmlFileType()
        {
            var rootName = "rtcDataConfig";

            var result = selector.ReturnSerializer(rootName);

            var typeName = TypeUtils.GetField(result, "mapping");
            var xmlFileType = TypeUtils.GetPropertyValue(typeName, "TypeName");

            Assert.That(result, Is.TypeOf<XmlSerializer>());
            Assert.That(xmlFileType, Is.EqualTo("RTCDataConfigXML"));
        }

        [Test]
        public void GetXmlSerializerWithRtcRuntimeConfigXmlFileType()
        {
            var rootName = "rtcRuntimeConfig";

            var result = selector.ReturnSerializer(rootName);

            var typeName = TypeUtils.GetField(result, "mapping");
            var xmlFileType = TypeUtils.GetPropertyValue(typeName, "TypeName");

            Assert.That(result, Is.TypeOf<XmlSerializer>());
            Assert.That(xmlFileType, Is.EqualTo("RtcRuntimeConfigXML"));
        }

        [Test]
        public void GetXmlSerializerWithRtcToolsConfigXmlFileType()
        {
            var rootName = "rtcToolsConfig";

            var result = selector.ReturnSerializer(rootName);

            var typeName = TypeUtils.GetField(result, "mapping");
            var xmlFileType = TypeUtils.GetPropertyValue(typeName, "TypeName");

            Assert.That(result, Is.TypeOf<XmlSerializer>());
            Assert.That(xmlFileType, Is.EqualTo("RtcToolsConfigXML"));
        }

        [Test]
        public void GetXmlSerializerWithStateImportConfigXmlFileType()
        {
            var rootName = "treeVectorFile";

            var result = selector.ReturnSerializer(rootName);

            var typeName = TypeUtils.GetField(result, "mapping");
            var xmlFileType = TypeUtils.GetPropertyValue(typeName, "TypeName");

            Assert.That(result, Is.TypeOf<XmlSerializer>());
            Assert.That(xmlFileType, Is.EqualTo("TreeVectorFileXML"));
        }
    }
}
