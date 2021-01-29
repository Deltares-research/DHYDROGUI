using DelftTools.TestUtils;
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
            var xmlDocument = FileToObject.ConvertToXDocument(TestHelper.GetTestWorkingDirectory(@"..\XmlValidation\XMLTest.xml"));
            Assert.IsNotNull(xmlDocument);
        }

        [Test]
        public void ConvertXmlSchemaFielToXmlSchemaObject()
        {
            var xmlSchema = FileToObject.ConvertToXmlSchema(TestHelper.GetTestWorkingDirectory(@"..\XmlValidation\XMLValidationTest.xsd"));
            Assert.IsNotNull(xmlSchema);
        }
    }
}
