using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class IniSectionExtensionsTests
    {
        [Test]
        public void AddSedimentPropertyTest()
        {
            var iniSection = new IniSection("section");
            iniSection.AddSedimentProperty(SedimentFile.Name.Key,"MyValue","","");
            var addedProperty = iniSection.Properties.FirstOrDefault();
            Assert.IsNotNull(addedProperty);
            Assert.That(addedProperty.Value, Is.Not.Contains("#")); // Don't automaticlly add hashes, responsibility of caller!
            Assert.AreEqual("MyValue", addedProperty.Value);
        }
    }
}