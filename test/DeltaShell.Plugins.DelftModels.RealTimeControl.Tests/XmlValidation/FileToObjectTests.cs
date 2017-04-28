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
            var xmlDocument = FileToObject.ConvertToXDocument(@"XmlValidation\XMLTest.xml");
            Assert.IsNotNull(xmlDocument);
        }

        [Test]
        public void ConvertXmlSchemaFielToXmlSchemaObject()
        {
            var xmlSchema = FileToObject.ConvertToXmlSchema(@"XmlValidation\XMLValidationTest.xsd");
            Assert.IsNotNull(xmlSchema);
        }
    }
}
