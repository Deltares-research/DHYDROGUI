using System.Xml.Linq;
using System.Xml.Schema;
using DeltaShell.Plugins.DelftModels.RealTimeControl.XmlValidation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.XmlValidation
{
    [TestFixture]
    public class FileToObjectTests
    {
        [Test]
        public void ConvertXmlDocumentFileToXmlDocumentObject()
        {
            XDocument xmlDocument = FileToObject.ConvertToXDocument(@"XmlValidation\XMLTest.xml");
            Assert.IsNotNull(xmlDocument);
        }

        [Test]
        public void ConvertXmlSchemaFielToXmlSchemaObject()
        {
            XmlSchema xmlSchema = FileToObject.ConvertToXmlSchema(@"XmlValidation\XMLValidationTest.xsd");
            Assert.IsNotNull(xmlSchema);
        }
    }
}