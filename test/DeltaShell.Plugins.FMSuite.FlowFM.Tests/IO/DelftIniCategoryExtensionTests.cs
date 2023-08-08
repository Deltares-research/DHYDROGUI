using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class DelftIniCategoryExtensionTests
    {
        [Test]
        public void AddSedimentPropertyTest()
        {
            var category = new DelftIniCategory("category");
            category.AddSedimentProperty(SedimentFile.Name.Key,"MyValue","","");
            var addedProperty = category.Properties.FirstOrDefault();
            Assert.IsNotNull(addedProperty);
            Assert.That(addedProperty.Value, Is.Not.Contains("#")); // Don't automaticlly add hashes, responsibility of caller!
            Assert.AreEqual("MyValue", addedProperty.Value);
        }
    }
}