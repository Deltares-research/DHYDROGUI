using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlXmlDirectoryLookupTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConvertJsonFileToObject()
        {
            string pathToFile = Path.Combine(TestHelper.GetTestDataDirectory(), "JsonConvert", "settings.json");
            string file = File.ReadAllText(pathToFile);
            var fileObject = JsonConvert.DeserializeObject<RealTimeControlXmlDirectoryLookup>(file);

            Assert.That(fileObject, Is.TypeOf<RealTimeControlXmlDirectoryLookup>());
            Assert.That(fileObject.XmlDirectory, Is.EqualTo("pathToXml"));
            Assert.That(fileObject.SchemaDirectory, Is.EqualTo("pathToSchema"));
        }
    }
}