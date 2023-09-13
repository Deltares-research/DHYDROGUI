using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class IniSectionExtensionsTest
    {
        [Test]
        public void AddSedimentPropertyTest()
        {
            var section = new IniSection("section");
            section.AddSedimentProperty(SedimentFile.Name.Key, "MyValue", "", "");
            IniProperty addedProperty = section.Properties.FirstOrDefault();
            Assert.IsNotNull(addedProperty);
            Assert.That(addedProperty.Value, Is.Not.Contains("#")); // Don't automatically add hashes, responsibility of caller!
            Assert.AreEqual("MyValue", addedProperty.Value);
        }
    }
}