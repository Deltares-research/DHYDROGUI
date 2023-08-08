using System.Linq;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class DelftIniCategoryExtensionTest
    {
        [Test]
        public void AddSedimentPropertyTest()
        {
            var category = new DelftIniCategory("category");
            category.AddSedimentProperty(SedimentFile.Name.Key, "MyValue", "", "");
            DelftIniProperty addedProperty = category.Properties.FirstOrDefault();
            Assert.IsNotNull(addedProperty);
            Assert.That(addedProperty.Value, Is.Not.Contains("#")); // Don't automatically add hashes, responsibility of caller!
            Assert.AreEqual("MyValue", addedProperty.Value);
        }
    }
}