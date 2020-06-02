using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Import
{
    [TestFixture]
    public class RtcXmlDirectoryLookupTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConvertJsonFileToObject()
        {
            string pathToFile = Path.Combine(TestHelper.GetTestDataDirectory(), "JsonConvert", "settings.json");
            string file = File.ReadAllText(pathToFile);
            var fileObject = JsonConvert.DeserializeObject<RtcXmlDirectoryLookup>(file);

            Assert.That(fileObject, Is.TypeOf<RtcXmlDirectoryLookup>());
            Assert.That(fileObject.XmlDirectory, Is.EqualTo("pathToXml"));
            Assert.That(fileObject.SchemaDirectory, Is.EqualTo("pathToSchema"));
        }
    }
}