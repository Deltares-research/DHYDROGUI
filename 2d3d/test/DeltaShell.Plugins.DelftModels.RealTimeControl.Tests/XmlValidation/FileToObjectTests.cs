using System.IO;
using System.Xml.Linq;
using System.Xml.Schema;
using DeltaShell.Plugins.DelftModels.RealTimeControl.XmlValidation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.XmlValidation
{
    [TestFixture]
    public class FileToObjectTests
    {
        private static readonly string xmlValidationTestXsd = Path.Combine(TestContext.CurrentContext.TestDirectory, @"XmlValidation\XMLValidationTest.xsd");
        private static readonly string xmlTestFile = Path.Combine(TestContext.CurrentContext.TestDirectory, @"XmlValidation\XMLTest.xml");

        [Test]
        public void ConvertXmlDocumentFileToXmlDocumentObject()
        {
            XDocument xmlDocument = FileToObject.ConvertToXDocument(xmlTestFile);
            Assert.IsNotNull(xmlDocument);
        }

        [Test]
        public void ConvertXmlSchemaFileToXmlSchemaObject()
        {
            XmlSchema xmlSchema = FileToObject.ConvertToXmlSchema(xmlValidationTestXsd);
            Assert.IsNotNull(xmlSchema);
        }
    }
}